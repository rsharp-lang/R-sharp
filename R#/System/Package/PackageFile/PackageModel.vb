#Region "Microsoft.VisualBasic::04178aa6cc1b65561e89e124418e80aa, D:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/PackageModel.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 418
    '    Code Lines: 317
    ' Comment Lines: 30
    '   Blank Lines: 71
    '     File Size: 17.92 KB


    '     Class PackageModel
    ' 
    '         Properties: assembly, clr, dataSymbols, info, loading
    '                     pkg_dir, symbols, unixman, vignettes
    ' 
    '         Function: getDataSymbolName, normPath, ToString, writeSymbols
    ' 
    '         Sub: copyAssembly, Flush, saveClrDcouments, saveDataSymbols, saveDependency
    '              saveSymbols, saveUnixManIndex, writeIndex, writeRuntime
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.SecurityString
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports Microsoft.VisualBasic.Text.Xml.OpenXml
Imports SMRUCC.Rsharp.Development.Package.NuGet.metadata
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace Development.Package.File

    Public Class PackageModel

        Public Property info As DESCRIPTION

        ''' <summary>
        ''' only allows function and constant.
        ''' </summary>
        ''' <returns></returns>
        Public Property symbols As Dictionary(Of String, Expression)
        ''' <summary>
        ''' the file names in ``data/`` directory.
        ''' </summary>
        ''' <returns></returns>
        Public Property dataSymbols As Dictionary(Of String, String)
        Public Property loading As Dependency()
        Public Property assembly As AssemblyPack
        Public Property unixman As List(Of String)
        Public Property vignettes As List(Of String)
        ''' <summary>
        ''' documents about the clr data types
        ''' </summary>
        ''' <returns></returns>
        Public Property clr As New Dictionary(Of String, String)
        Public Property pkg_dir As String

        Public Overrides Function ToString() As String
            Return info.ToString
        End Function

        Private Function writeSymbols(zip As ZipArchive, ByRef checksum$) As Dictionary(Of String, String)
            Dim onLoad As DeclareNewFunction
            Dim symbols As New Dictionary(Of String, String)
            Dim sourceMaps As New List(Of StackFrame)

            For Each symbol As NamedValue(Of Expression) In Me.symbols _
                .Select(Function(t)
                            Return New NamedValue(Of Expression) With {
                                .Name = t.Key,
                                .Value = t.Value
                            }
                        End Function)

                If symbol.Name = ".onLoad" Then
                    onLoad = symbol.Value

                    Using file As New Writer(zip.CreateEntry(".onload").Open)
                        checksum = checksum & file.Write(onLoad)
                        sourceMaps = sourceMaps + file.GetSymbols
                    End Using
                Else
                    Dim symbolRef As String = symbol.Name.MD5

                    Using file As New Writer(zip.CreateEntry($"lib/src/{symbolRef}").Open)
                        checksum = checksum & file.Write(symbol.Value)
                        sourceMaps = sourceMaps + file.GetSymbols
                    End Using

                    Call symbols.Add(symbol.Name, symbolRef)
                End If
            Next

            Dim REngine As New RInterpreter
            Dim plugin As String = LibDLL.GetDllFile($"devkit.dll", REngine.globalEnvir)

            If plugin.FileExists Then
                Using file As New StreamWriter(zip.CreateEntry($"package/source.map").Open)
                    Dim encoder As String = "VisualStudio::sourceMap_encode"
                    Dim args As Object() = {sourceMaps.ToArray, info.Package}

                    Call PackageLoader.ParsePackages(plugin) _
                        .Where(Function(pkg) pkg.namespace = "VisualStudio") _
                        .FirstOrDefault _
                        .DoCall(Sub(pkg)
                                    Call REngine.globalEnvir.ImportsStatic(pkg.package)
                                End Sub)

                    Call JsonContract _
                        .GetObjectJson(
                            obj:=REngine.Invoke(encoder, args),
                            indent:=True
                        ) _
                        .DoCall(AddressOf file.WriteLine)
                End Using
            End If

            Return symbols
        End Function

        ''' <summary>
        ''' copy .NET assembly dll files
        ''' </summary>
        ''' <param name="zip"></param>
        ''' <param name="checksum"></param>
        Private Sub copyAssembly(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String
            Dim asset As Value(Of String) = ""

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/assembly.json").Open)
                text = assembly _
                    .AsEnumerable _
                    .ToDictionary(Function(path) assembly.getRelativePath(path),
                                  Function(fileName)
                                      Return md5.GetMd5Hash(fileName.ReadBinary)
                                  End Function) _
                    .GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("lib/assembly/readme.txt").Open)
                text = $".NET assembly files [{assembly.framework}]"
                text = text & vbCrLf & vbCrLf
                text = text & assembly.assembly _
                    .Select(Function(path) path.FileName) _
                    .JoinBy(vbCrLf)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            ' dll is the file path
            ' pack assembly folder
            Dim contents As String() = assembly.GetAllPackageContentFiles.ToArray
            Dim assemblyDirFull As String = assembly.directory.GetDirectoryFullPath

            Call zip.WriteFiles(
                files:=contents,
                mode:=ZipArchiveMode.Create,
                fileOverwrite:=Overwrite.Always,
                compression:=CompressionLevel.Fastest,
                relativeDir:=assemblyDirFull,
                parent:="lib/assembly"
            )
        End Sub

        Private Sub saveDependency(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/dependency.json").Open)
                Dim text = loading.GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub saveSymbols(zip As ZipArchive, symbols As Dictionary(Of String, String), ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/symbols.json").Open)
                text = symbols.GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Function getDataSymbolName(dirRoot As String, filepath As String) As String
            If filepath.ExtensionSuffix("csv", "txt", "rda", "rds") Then
                Return filepath.BaseName
            Else
                filepath = normPath(filepath.GetFullPath)
                filepath = filepath.Replace(dirRoot, "")

                Return filepath
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function normPath(path As String) As String
            Return path.Replace("\", "/").Replace("//", "/")
        End Function

        Private Sub saveDataSymbols(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String
            Dim dirBase As String = (pkg_dir & "/data") _
                .GetDirectoryFullPath _
                .DoCall(AddressOf normPath)
            Dim dirRoot As String = pkg_dir.GetDirectoryFullPath.DoCall(AddressOf normPath)

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/data.json").Open)
                text = dataSymbols _
                    .ToDictionary(Function(d) getDataSymbolName(dirRoot, d.Key),
                                  Function(d)
                                      Return New NamedValue(d.Key.FileName, d.Value)
                                  End Function) _
                    .GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            For Each ref As KeyValuePair(Of String, String) In dataSymbols
                Dim relpath As String = ref.Key _
                    .GetFullPath _
                    .DoCall(AddressOf normPath) _
                    .Replace(dirBase, "") _
                    .Replace("\", "/")
                Dim dataKey As String = $"data/{relpath}".Replace("//", "/")

                ' 20220410 在这里为了保持和传统的R语言脚本对system.file函数使用上的一致性
                ' 数据文件夹任然是放在程序包中的最顶层文件夹
                Using file As Stream = zip.CreateEntry(dataKey).Open
                    Dim buffer As Byte() = ref.Key.ReadBinary

                    Call file.Write(buffer, Scan0, buffer.Length)
                    Call file.Flush()
                End Using
            Next
        End Sub

        Private Sub writeIndex(zip As ZipArchive, ByRef checksum$)
            Dim text As String
            Dim md5 As New Md5HashProvider

            Using file As New StreamWriter(zip.CreateEntry("package/index.json").Open)
                info.meta("builtTime") = Now.ToString
                info.meta("os_built") = Environment.OSVersion.VersionString
                info.Date = If(info.Date, Now.ToString)
                text = info.GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub writeRuntime(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/runtime.json").Open)
                text = GetType(RInterpreter).Assembly _
                    .FromAssembly _
                    .GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/framework.json").Open)
                text = GetType(App).Assembly _
                    .FromAssembly _
                    .GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub saveUnixManIndex(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String
            Dim manIndex As New Dictionary(Of String, String)
            Dim htmlIndex As New Dictionary(Of String, String)
            Dim relpath As String
            Dim pkg_dir As String = Me.pkg_dir.GetDirectoryFullPath

            For Each man As String In unixman
                text = man.ReadAllText
                manIndex(man.BaseName) = md5.GetMd5Hash(text)
                checksum = checksum & manIndex(man.BaseName)

                Using file As New StreamWriter(zip.CreateEntry($"package/man/{man.BaseName}.1").Open)
                    Call file.WriteLine(text)
                    Call file.Flush()
                End Using
            Next

            ' for each html file path
            For Each man As String In vignettes
                text = man.ReadAllText
                htmlIndex(man.BaseName) = md5.GetMd5Hash(text)
                checksum = checksum & htmlIndex(man.BaseName)
                relpath = man.GetFullPath.Replace(pkg_dir, "")

                Using file As New StreamWriter(zip.CreateEntry($"package/{relpath.Replace("\", "/")}").Open)
                    Call file.WriteLine(text)
                    Call file.Flush()
                End Using
            Next

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/vignettes.json").Open)
                text = htmlIndex.GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/unixman.json").Open)
                text = manIndex.GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub saveClrDcouments(zip As ZipArchive, ByRef checksum$)
            Using file As New StreamWriter(zip.CreateEntry("package/clr/readme.txt").Open)
                Call file.WriteLine(".NET CLR object documents at here!")
                Call file.Flush()
            End Using
        End Sub

        ''' <summary>
        ''' generate package file from here
        ''' </summary>
        ''' <param name="outfile"></param>
        ''' <param name="assets"></param>
        ''' <remarks>
        ''' generate nuget package format zip archive file.
        ''' </remarks>
        Public Sub Flush(outfile As Stream, assets As Dictionary(Of String, String))
            Dim checksum As String = ""
            Dim md5 As New Md5HashProvider

            Using zip As New ZipArchive(outfile, ZipArchiveMode.Create)
                Dim symbols As Dictionary(Of String, String) = writeSymbols(zip, checksum)

                Call saveSymbols(zip, symbols, checksum)
                Call saveDataSymbols(zip, checksum)
                Call saveUnixManIndex(zip, checksum)
                Call copyAssembly(zip, checksum)
                Call saveDependency(zip, checksum)
                Call writeIndex(zip, checksum)
                Call writeRuntime(zip, checksum)
                Call saveClrDcouments(zip, checksum)

                Dim checksumVal As String = md5.GetMd5Hash(checksum).ToUpper

                Using file As New StreamWriter(zip.CreateEntry("CHECKSUM").Open)
                    Call file.WriteLine(checksumVal)
                    Call file.Flush()
                End Using

                Dim nugetWeb As String = $"package/services/metadata/core-properties/{checksumVal.ToLower}.psmdcp"
                Dim nugetIdx As String = $"{info.Package}.nuspec"
                Dim webId As String
                Dim indexId As String
                Dim text As String

                Using file As New StreamWriter(zip.CreateEntry(nugetWeb).Open)
                    text = coreProperties.CreateMetaData(info).GetXml(xmlEncoding:=XmlEncodings.UTF8)
                    webId = text.MD5.Substring(0, 17).ToUpper

                    Call file.WriteLine(text)
                    Call file.Flush()
                End Using

                Using file As New StreamWriter(zip.CreateEntry(nugetIdx).Open)
                    text = nuspec.CreatePackageIndex(Me).GetXml(xmlEncoding:=XmlEncodings.UTF8)
                    indexId = text.MD5.Substring(0, 17).ToUpper

                    Call file.WriteLine(text)
                    Call file.Flush()
                End Using

                Using file As New StreamWriter(zip.CreateEntry("[Content_Types].xml").Open)
                    Call file.WriteLine(OpenXMLSolver.DefaultContentTypes.GetXml(xmlEncoding:=XmlEncodings.UTF8))
                    Call file.Flush()
                End Using

                Using file As New StreamWriter(zip.CreateEntry("_rels/.rels").Open)
                    Dim webmeta As New Relationship With {.Target = $"/{nugetWeb}", .Type = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties", .Id = webId}
                    Dim indexmeta As New Relationship With {.Target = $"/{nugetIdx}", .Type = "http://schemas.microsoft.com/packaging/2010/07/manifest", .Id = indexId}
                    Dim files As New rels With {
                        .Relationships = {indexmeta, webmeta}
                    }

                    Call file.WriteLine(files.GetXml(xmlEncoding:=XmlEncodings.UTF8))
                    Call file.Flush()
                End Using

                For Each asset As KeyValuePair(Of String, String) In assets
                    Using file As New BinaryWriter(zip.CreateEntry(asset.Key).Open)
                        Call file.Write(asset.Value.ReadBinary)
                        Call file.Flush()
                    End Using
                Next
            End Using
        End Sub

    End Class
End Namespace
