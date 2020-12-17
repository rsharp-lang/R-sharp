#Region "Microsoft.VisualBasic::93db2c490af0bc77a91e90860a42ac58, R#\System\Package\PackageFile\PackageModel.vb"

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
'         Properties: info
' 
'         Sub: Flush
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.IO.Compression
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON
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

        Public Sub Flush(outfile As Stream)
            Using zip As New ZipArchive(outfile, ZipArchiveMode.Create)
                info.meta("BuiltTime") = Now.ToString

                Using file As New StreamWriter(zip.CreateEntry("index.json").Open)
                    Call file.WriteLine(info.GetJson(indent:=True))
                    Call file.Flush()
                End Using

                Dim symbols As New Dictionary(Of String, String)
                Dim onLoad As DeclareNewFunction

                For Each symbol As NamedValue(Of Expression) In Me.symbols.Select(Function(t) New NamedValue(Of Expression)(t.Key, t.Value))
                    If symbol.Name = ".onLoad" Then
                        onLoad = symbol.Value

                        Using file As New Writer(zip.CreateEntry(".onload").Open)
                            Call file.Write(onLoad)
                        End Using
                    Else
                        Dim symbolRef As String = symbol.Name.MD5

                        Using file As New Writer(zip.CreateEntry($"src/{symbolRef}").Open)
                            Call file.Write(symbol.Value)
                        End Using

                        Call symbols.Add(symbol.Name, symbolRef)
                    End If
                Next

                Using file As New StreamWriter(zip.CreateEntry("dependency.json").Open)
                    Call file.WriteLine(loading.GetJson(indent:=True))
                    Call file.Flush()
                End Using

                Using file As New StreamWriter(zip.CreateEntry("symbols.json").Open)
                    Call file.WriteLine(symbols.GetJson(indent:=True))
                    Call file.Flush()
                End Using
            End Using
        End Sub

    End Class
End Namespace
