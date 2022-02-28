#Region "Microsoft.VisualBasic::44f699ec3497fe37b0c946bd1815d284, R#\System\Package\PackageFile\PackageModel.vb"

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

    '     Class PackageModel
    ' 
    '         Properties: assembly, dataSymbols, info, loading, symbols
    '                     unixman
    ' 
    '         Function: ToString, writeSymbols
    ' 
    '         Sub: copyAssembly, Flush, saveDataSymbols, saveDependency, saveSymbols
    '              saveUnixManIndex, writeIndex, writeRuntime
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Zip
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.SecurityString
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models
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
        Public Property unixman As Dictionary(Of String, String)

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

                    Using file As New Writer(zip.CreateEntry($"src/{symbolRef}").Open)
                        checksum = checksum & file.Write(symbol.Value)
                        sourceMaps = sourceMaps + file.GetSymbols
                    End Using

                    Call symbols.Add(symbol.Name, symbolRef)
                End If
            Next

            Dim REngine As New RInterpreter
            Dim plugin As String = LibDLL.GetDllFile($"devkit.dll", REngine.globalEnvir)

            If plugin.FileExists Then
                Using file As New StreamWriter(zip.CreateEntry($"source.map").Open)
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

        Private Sub copyAssembly(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String
            Dim asset As Value(Of String) = ""

            Using file As New StreamWriter(zip.CreateEntry("manifest/assembly.json").Open)
                text = assembly _
                    .AsEnumerable _
                    .ToDictionary(Function(path) path.FileName,
                                  Function(fileName)
                                      Return md5.GetMd5Hash(fileName.ReadBinary)
                                  End Function) _
                    .GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("assembly/readme.txt").Open)
                text = ".NET assembly files"
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            ' dll is the file path
            ' pack assembly folder
            Dim contents As String() = assembly.GetFileContents
            Dim assemblyDirFull As String = assembly.directory.GetDirectoryFullPath

            Call zip.WriteFiles(
                files:=contents,
                mode:=ZipArchiveMode.Create,
                fileOverwrite:=Overwrite.Always,
                compression:=CompressionLevel.Fastest,
                relativeDir:=assemblyDirFull,
                parent:="assembly"
            )
        End Sub

        Private Sub saveDependency(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider

            Using file As New StreamWriter(zip.CreateEntry("manifest/dependency.json").Open)
                Dim text = loading.GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub saveSymbols(zip As ZipArchive, symbols As Dictionary(Of String, String), ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String

            Using file As New StreamWriter(zip.CreateEntry("manifest/symbols.json").Open)
                text = symbols.GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub saveDataSymbols(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String

            Using file As New StreamWriter(zip.CreateEntry("manifest/data.json").Open)
                text = dataSymbols _
                    .ToDictionary(Function(d) d.Key.BaseName,
                                  Function(d)
                                      Return New NamedValue(d.Key.FileName, d.Value)
                                  End Function) _
                    .GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            For Each ref As KeyValuePair(Of String, String) In dataSymbols
                Using file As Stream = zip.CreateEntry($"data/{ref.Key.FileName}").Open
                    Dim buffer As Byte() = ref.Key.ReadBinary

                    Call file.Write(buffer, Scan0, buffer.Length)
                    Call file.Flush()
                End Using
            Next
        End Sub

        Private Sub writeIndex(zip As ZipArchive, ByRef checksum$)
            Dim text As String
            Dim md5 As New Md5HashProvider

            Using file As New StreamWriter(zip.CreateEntry("index.json").Open)
                info.meta("builtTime") = Now.ToString
                text = info.GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

        Private Sub writeRuntime(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String

            Using file As New StreamWriter(zip.CreateEntry("manifest/runtime.json").Open)
                text = GetType(RInterpreter).Assembly _
                    .FromAssembly _
                    .GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using

            Using file As New StreamWriter(zip.CreateEntry("manifest/framework.json").Open)
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

            For Each man As String In unixman.Values
                text = man.ReadAllText
                manIndex(man.BaseName) = md5.GetMd5Hash(text)
                checksum = checksum & manIndex(man.BaseName)

                Using file As New StreamWriter(zip.CreateEntry($"man/{man.BaseName}.1").Open)
                    Call file.WriteLine(text)
                    Call file.Flush()
                End Using
            Next

            Using file As New StreamWriter(zip.CreateEntry("manifest/unixman.json").Open)
                text = manIndex.GetJson(indent:=True)
                checksum = checksum & md5.GetMd5Hash(text)

                Call file.WriteLine(text)
                Call file.Flush()
            End Using
        End Sub

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

                Using file As New StreamWriter(zip.CreateEntry("CHECKSUM").Open)
                    Call file.WriteLine(md5.GetMd5Hash(checksum).ToUpper)
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
