#Region "Microsoft.VisualBasic::64bb6b75558ac2370be16773f1ee84ee, R#\System\Package\PackageFile\PackageModel.vb"

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
    '         Properties: assembly, info, loading, symbols
    ' 
    '         Function: writeSymbols
    ' 
    '         Sub: copyAssembly, Flush, saveDependency, saveSymbols, writeIndex
    '              writeRuntime
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.SecurityString
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace System.Package.File

    Public Class PackageModel

        Public Property info As DESCRIPTION

        ''' <summary>
        ''' only allows function and constant.
        ''' </summary>
        ''' <returns></returns>
        Public Property symbols As Dictionary(Of String, Expression)
        Public Property loading As Dependency()
        ''' <summary>
        ''' dll files
        ''' </summary>
        ''' <returns></returns>
        Public Property assembly As String()

        Private Function writeSymbols(zip As ZipArchive, ByRef checksum$) As Dictionary(Of String, String)
            Dim onLoad As DeclareNewFunction
            Dim symbols As New Dictionary(Of String, String)

            For Each symbol As NamedValue(Of Expression) In Me.symbols.Select(Function(t) New NamedValue(Of Expression)(t.Key, t.Value))
                If symbol.Name = ".onLoad" Then
                    onLoad = symbol.Value

                    Using file As New Writer(zip.CreateEntry(".onload").Open)
                        checksum = checksum & file.Write(onLoad)
                    End Using
                Else
                    Dim symbolRef As String = symbol.Name.MD5

                    Using file As New Writer(zip.CreateEntry($"src/{symbolRef}").Open)
                        checksum = checksum & file.Write(symbol.Value)
                    End Using

                    Call symbols.Add(symbol.Name, symbolRef)
                End If
            Next

            Return symbols
        End Function

        Private Sub copyAssembly(zip As ZipArchive, ByRef checksum$)
            Dim md5 As New Md5HashProvider
            Dim text As String

            Using file As New StreamWriter(zip.CreateEntry("manifest/assembly.json").Open)
                text = assembly _
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

            For Each dll As String In assembly
                Using file As New BinaryWriter(zip.CreateEntry($"assembly/{dll.FileName}").Open)
                    checksum = checksum & md5.GetMd5Hash(dll.ReadBinary)

                    Call file.Write(dll.ReadBinary)
                    Call file.Flush()
                End Using
            Next
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

        Public Sub Flush(outfile As Stream)
            Dim checksum As String = ""
            Dim md5 As New Md5HashProvider

            Using zip As New ZipArchive(outfile, ZipArchiveMode.Create)
                Dim symbols As Dictionary(Of String, String) = writeSymbols(zip, checksum)

                Call saveSymbols(zip, symbols, checksum)
                Call copyAssembly(zip, checksum)
                Call saveDependency(zip, checksum)
                Call writeIndex(zip, checksum)
                Call writeRuntime(zip, checksum)

                Using file As New StreamWriter(zip.CreateEntry("CHECKSUM").Open)
                    Call file.WriteLine(md5.GetMd5Hash(checksum).ToUpper)
                    Call file.Flush()
                End Using
            End Using
        End Sub

    End Class
End Namespace
