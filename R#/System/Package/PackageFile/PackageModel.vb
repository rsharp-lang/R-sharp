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
Imports SMRUCC.Rsharp.System.Package.File.Expression

Namespace System.Package.File

    Public Class PackageModel

        Public Property info As DESCRIPTION

        ''' <summary>
        ''' only allows function and constant.
        ''' </summary>
        ''' <returns></returns>
        Public Property symbols As Dictionary(Of String, RExpression)

        Public Sub Flush(outfile As Stream)
            Using zip As New ZipArchive(outfile, ZipArchiveMode.Create)
                Using file As New StreamWriter(zip.CreateEntry("index.json").Open)
                    Call file.WriteLine(info.GetJson)
                    Call file.Flush()
                End Using

                Dim symbols As New List(Of String)

                For Each symbol As NamedValue(Of RExpression) In Me.symbols.Select(Function(t) New NamedValue(Of RExpression)(t.Key, t.Value))
                    Using file As New StreamWriter(zip.CreateEntry(symbol.Name).Open)
                        Call file.WriteLine(symbol.Value.GetJson)
                        Call file.Flush()
                    End Using

                    Call symbols.Add(symbol.Name)
                Next

                Using file As New StreamWriter(zip.CreateEntry("symbols.json").Open)
                    Call file.WriteLine(symbols.ToArray.GetJson)
                    Call file.Flush()
                End Using
            End Using
        End Sub

    End Class
End Namespace
