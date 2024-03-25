#Region "Microsoft.VisualBasic::47f80170106eff15f5b5b59dc7af9205, D:/GCModeller/src/R-sharp/R#//System/Package/PackageFile/NuGet/WriteNuGetZip.vb"

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

    '   Total Lines: 420
    '    Code Lines: 338
    ' Comment Lines: 8
    '   Blank Lines: 74
    '     File Size: 17.10 KB


    '     Class NuGetZip
    ' 
    '         Properties: pkg_dir
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CreateNuGetPackage, getDataSymbolName, GetPackageSymbols, normPath, writeSymbols
    ' 
    '         Sub: copyAssembly, saveClrDcouments, saveDataSymbols, saveDependency, saveSymbols
    '              saveUnixManIndex, writeIndex, WriteNuGetAssets, writeRuntime
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.ComponentModel.Collection
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

    Public Class NuGetZip

        Dim zip As ZipArchive
        Dim checksum As New StringBuilder
        Dim pkg As PackageModel

        Public ReadOnly Property pkg_dir As String
            Get
                Return pkg.pkg_dir
            End Get
        End Property

        <DebuggerStepThrough>
        Sub New(zip As ZipArchive, pkg As PackageModel)
            Me.zip = zip
            Me.pkg = pkg
        End Sub

        Private Sub WriteNuGetAssets(checksumVal As String)
            Dim nugetWeb As String = $"package/services/metadata/core-properties/{checksumVal.ToLower}.psmdcp"
            Dim nugetIdx As String = $"{pkg.info.Package}.nuspec"
            Dim webId As String
            Dim indexId As String
            Dim text As String

            Using file As New StreamWriter(zip.CreateEntry(nugetWeb).Open)
                text = coreProperties.CreateMetaData(pkg.info).GetXml(xmlEncoding:=XmlEncodings.UTF8)
                webId = text.MD5.Substring(0, 17).ToUpper

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry(nugetIdx).Open)
                text = nuspec.CreatePackageIndex(pkg).GetXml(xmlEncoding:=XmlEncodings.UTF8)
                indexId = text.MD5.Substring(0, 17).ToUpper

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("[Content_Types].xml").Open)
                Call file.WriteLine(OpenXMLSolver.DefaultContentTypes.GetXml(xmlEncoding:=XmlEncodings.UTF8))
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("_rels/.rels").Open)
                Dim webmeta As New Relationship With {
                    .Target = $"/{nugetWeb}",
                    .Type = "http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties",
                    .Id = webId
                }
                Dim indexmeta As New Relationship With {
                    .Target = $"/{nugetIdx}",
                    .Type = "http://schemas.microsoft.com/packaging/2010/07/manifest",
                    .Id = indexId
                }
                Dim files As New rels With {
                    .Relationships = {indexmeta, webmeta}
                }

                Call file.WriteLine(files.GetXml(xmlEncoding:=XmlEncodings.UTF8))
                Call file.Flush()
            End Using
        End Sub

        Public Function CreateNuGetPackage(assets As Dictionary(Of String, String)) As String
            Dim symbols As Dictionary(Of String, String) = writeSymbols()
            Dim md5 As New Md5HashProvider

            Call saveSymbols(symbols)
            Call saveDataSymbols()
            Call saveUnixManIndex()
            Call copyAssembly()
            Call saveDependency()
            Call writeIndex()
            Call writeRuntime()
            Call saveClrDcouments()

            Dim checksumVal As String = md5.GetMd5Hash(checksum.ToString).ToUpper

            Using file As New StreamWriter(zip.CreateEntry("CHECKSUM").Open)
                Call file.WriteLine(checksumVal)
                Call file.Flush()
            End Using

            Call WriteNuGetAssets(checksumVal)

            For Each asset As KeyValuePair(Of String, String) In assets
                Using file As New BinaryWriter(zip.CreateEntry(asset.Key).Open)
                    Call file.Write(asset.Value.ReadBinary)
                    Call file.Flush()
                End Using
            Next

            Return checksumVal
        End Function

        Private Iterator Function GetPackageSymbols() As IEnumerable(Of NamedValue(Of Expression))
            For Each symbol In pkg.symbols
                Yield New NamedValue(Of Expression) With {
                    .Name = symbol.Key,
                    .Value = symbol.Value
                }
            Next
        End Function

        Private Function writeSymbols() As Dictionary(Of String, String)
            Dim onLoad As DeclareNewFunction
            Dim symbols As New Dictionary(Of String, String)
            Dim sourceMaps As New List(Of StackFrame)

            For Each symbol As NamedValue(Of Expression) In GetPackageSymbols()
                If symbol.Name = ".onLoad" Then
                    onLoad = symbol.Value

                    Using file As New Writer(zip.CreateEntry(".onload").Open)
                        checksum.AppendLine(file.Write(onLoad))
                        sourceMaps = sourceMaps + file.GetSymbols
                    End Using
                Else
                    Dim symbolRef As String = symbol.Name.MD5

                    Using file As New Writer(zip.CreateEntry($"lib/src/{symbolRef}").Open)
                        checksum = checksum.AppendLine(file.Write(symbol.Value))
                        sourceMaps = sourceMaps + file.GetSymbols
                    End Using

                    Call symbols.Add(symbol.Name, symbolRef)
                End If
            Next

            For Each modPkg In pkg.tsd
                Using file As New StreamWriter(zip.CreateEntry($"lib/exports/{modPkg.Key}.d.ts").Open)
                    checksum = checksum.AppendLine(modPkg.Value)
                    file.WriteLine(modPkg.Value)
                End Using
            Next

            Dim REngine As New RInterpreter
            Dim plugin As String = LibDLL.GetDllFile($"devkit.dll", REngine.globalEnvir)

            If plugin.FileExists Then
                Using file As New StreamWriter(zip.CreateEntry($"package/source.map").Open)
                    Dim encoder As String = "VisualStudio::sourceMap_encode"
                    Dim args As Object() = {sourceMaps.ToArray, pkg.info.Package}

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
        Private Sub copyAssembly()
            Dim md5 As New Md5HashProvider
            Dim text As String
            Dim asset As Value(Of String) = ""
            Dim assembly = pkg.assembly

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/assembly.json").Open)
                text = pkg.assembly _
                    .AsEnumerable _
                    .ToDictionary(Function(path) assembly.getRelativePath(path),
                                  Function(fileName)
                                      Return md5.GetMd5Hash(fileName.ReadBinary)
                                  End Function) _
                    .GetJson(indent:=True)
                checksum.AppendLine(md5.GetMd5Hash(text))

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("lib/assembly/readme.txt").Open)
                text = $".NET assembly files [{assembly.framework}]"
                text = text & vbCrLf & vbCrLf
                text = text & assembly.assembly _
                    .Select(Function(path) path.FileName) _
                    .JoinBy(vbCrLf)
                checksum.AppendLine(md5.GetMd5Hash(text))

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            ' dll is the file path
            ' pack assembly folder
            Dim contents As String() = assembly.GetAllPackageContentFiles.ToArray
            Dim assemblyDirFull As String = assembly.directory.GetDirectoryFullPath
            Dim xmls As String() = assembly.GetAllPackageClrXmlDocumentFiles.ToArray

            Call zip.WriteFiles(
                files:=contents,
                mode:=ZipArchiveMode.Create,
                fileOverwrite:=Overwrite.Always,
                compression:=CompressionLevel.Fastest,
                relativeDir:=assemblyDirFull,
                parent:="lib/assembly"
            )
            Call zip.WriteFiles(
                files:=xmls,
                mode:=ZipArchiveMode.Create,
                fileOverwrite:=Overwrite.Always,
                compression:=CompressionLevel.Fastest,
                relativeDir:=assemblyDirFull,
                parent:="package/clr"
            )
        End Sub

        Private Sub saveDependency()
            Dim md5 As New Md5HashProvider

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/dependency.json").Open)
                Dim text = pkg.loading.GetJson(indent:=True)

                Call checksum.AppendLine(md5.GetMd5Hash(text))
                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub saveSymbols(symbols As Dictionary(Of String, String))
            Dim md5 As New Md5HashProvider
            Dim text As String

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/symbols.json").Open)
                text = symbols.GetJson(indent:=True)

                Call checksum.AppendLine(md5.GetMd5Hash(text))
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

        Private Sub saveDataSymbols()
            Dim md5 As New Md5HashProvider
            Dim text As String
            Dim dirBase As String = (pkg_dir & "/data") _
                .GetDirectoryFullPath _
                .DoCall(AddressOf normPath)
            Dim dirRoot As String = pkg_dir.GetDirectoryFullPath.DoCall(AddressOf normPath)

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/data.json").Open)
                Dim check_duplicated_name = pkg.dataSymbols _
                    .GroupBy(Function(d) getDataSymbolName(dirRoot, d.Key)) _
                    .Where(Function(g) g.Count > 1) _
                    .Keys

                If check_duplicated_name.Any Then
                    Throw New DuplicateNameException($"there is some file name in the ``data/`` directory is duplicated: {check_duplicated_name.GetJson}")
                End If

                text = pkg.dataSymbols _
                    .ToDictionary(Function(d) getDataSymbolName(dirRoot, d.Key),
                                  Function(d)
                                      Return New NamedValue(d.Key.FileName, d.Value)
                                  End Function) _
                    .GetJson(indent:=True)

                Call checksum.AppendLine(md5.GetMd5Hash(text))
                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            For Each ref As KeyValuePair(Of String, String) In pkg.dataSymbols
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

        Private Sub writeIndex()
            Dim text As String
            Dim md5 As New Md5HashProvider
            Dim info = pkg.info

            Using file As New StreamWriter(zip.CreateEntry("package/index.json").Open)
                info.meta("builtTime") = Now.ToString
                info.meta("os_built") = Environment.OSVersion.VersionString
                info.Date = If(info.Date, Now.ToString)
                text = info.GetJson(indent:=True)

                Call checksum.AppendLine(md5.GetMd5Hash(text))
                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub writeRuntime()
            Dim md5 As New Md5HashProvider
            Dim text As String

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/runtime.json").Open)
                text = GetType(RInterpreter).Assembly _
                    .FromAssembly _
                    .GetJson(indent:=True)

                Call checksum.AppendLine(md5.GetMd5Hash(text))
                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/framework.json").Open)
                text = GetType(App).Assembly _
                    .FromAssembly _
                    .GetJson(indent:=True)

                Call checksum.AppendLine(md5.GetMd5Hash(text))
                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub saveUnixManIndex()
            Dim md5 As New Md5HashProvider
            Dim text As String
            Dim manIndex As New Dictionary(Of String, String)
            Dim htmlIndex As New Dictionary(Of String, String)
            Dim relpath As String
            Dim pkg_dir As String = Me.pkg_dir.GetDirectoryFullPath

            For Each man As String In pkg.unixman
                text = man.ReadAllText
                manIndex(man.BaseName) = md5.GetMd5Hash(text)

                Call checksum.AppendLine(manIndex(man.BaseName))

                Using file As New StreamWriter(zip.CreateEntry($"package/man/{man.BaseName}.1").Open)
                    Call file.WriteLine(text)
                    Call file.Flush()
                End Using
            Next

            ' for each html file path
            For Each man As String In pkg.vignettes
                text = man.ReadAllText
                htmlIndex(man.BaseName) = md5.GetMd5Hash(text)
                relpath = man.GetFullPath.Replace(pkg_dir, "")
                relpath = $"package/{relpath.Replace("\", "/")}"

                Call checksum.AppendLine(htmlIndex(man.BaseName))

                Using file As New StreamWriter(zip.CreateEntry(relpath).Open)
                    Call file.WriteLine(text)
                    Call file.Flush()
                End Using
            Next

            Using file As New StreamWriter(zip.CreateEntry("package/man/index.json").Open)
                text = $"{pkg_dir}/man/index.json".ReadAllText
                checksum.AppendLine(text.MD5)
                file.WriteLine(text)
                file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/vignettes.json").Open)
                text = htmlIndex.GetJson(indent:=True)

                Call checksum.AppendLine(md5.GetMd5Hash(text))
                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("package/manifest/unixman.json").Open)
                text = manIndex.GetJson(indent:=True)

                Call checksum.AppendLine(md5.GetMd5Hash(text))
                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub saveClrDcouments()
            Using file As New StreamWriter(zip.CreateEntry("package/clr/readme.txt").Open)
                Call file.WriteLine(".NET CLR object documents at here!")
                Call file.Flush()
            End Using
        End Sub
    End Class
End Namespace
