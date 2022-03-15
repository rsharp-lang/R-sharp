#Region "Microsoft.VisualBasic::887e596ec30ac5e4416a98bfe43b7f76, R-sharp\studio\Rsharp_kit\roxygenNet\RoxygenDocument.vb"

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

    '   Total Lines: 137
    '    Code Lines: 115
    ' Comment Lines: 2
    '   Blank Lines: 20
    '     File Size: 5.32 KB


    ' Class RoxygenDocument
    ' 
    '     Function: continuteLines, ParseDocument, ParseDocuments, SplitBlocks
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components

Public Class RoxygenDocument

    Public Shared Iterator Function ParseDocuments(R As Rscript) As IEnumerable(Of Document)
        Dim script As Program = Program.CreateProgram(R)
        Dim symbols As Expression() = (From line As Expression In script Where line.IsFunctionDeclare).ToArray
        Dim list As Dictionary(Of String, Document) = SplitBlocks(R.script) _
            .ToDictionary(Function(a) a.Name,
                          Function(a)
                              Return a.Value
                          End Function)

        For Each item As Expression In symbols
            Dim func As DeclareNewFunction = item.MakeFunction

            If func Is Nothing Then
                Throw New NotImplementedException(item.GetType.FullName)
            End If

            Dim docs As Document = list.TryGetValue(func.funcName)

            If docs Is Nothing Then
                docs = New Document With {
                    .title = func.funcName
                }
            End If

            docs.declares = New FunctionDeclare With {
                .name = func.funcName,
                .parameters = func.parameters _
                    .Select(AddressOf FunctionDeclare.GetArgument) _
                    .ToArray,
                .sourceMap = func.stackFrame
            }

            Yield docs
        Next
    End Function

    Private Shared Iterator Function SplitBlocks(scriptText As String) As IEnumerable(Of NamedValue(Of Document))
        Dim buffer As New List(Of String)

        For Each line As String In scriptText.LineIterators
            If line.StartsWith("#'") Then
                buffer.Add(line.Substring(2).Trim)
            ElseIf buffer > 0 Then
                Dim name As String() = line.Trim _
                    .Split _
                    .Where(Function(s) Not s.StringEmpty) _
                    .ToArray
                Dim newLines = buffer.PopAll _
                    .Where(Function(str) Not str.StringEmpty) _
                    .DoCall(AddressOf continuteLines)

                If newLines.Length = 0 Then
                    Continue For
                End If

                ' const xxx as function -> name(1)
                ' xxx = function -> name(0)
                Yield New NamedValue(Of Document) With {
                    .Name = If(name(1) = "=", name(0), name(1)),
                    .Value = ParseDocument(newLines)
                }
            End If
        Next
    End Function

    Private Shared Function continuteLines(lines As IEnumerable(Of String)) As String()
        Dim list As New List(Of String)
        Dim continuteLine As String = Nothing

        For Each line As String In lines
            If line.FirstOrDefault = "@"c Then
                If Not continuteLine Is Nothing Then
                    list.Add(continuteLine)
                    continuteLine = Nothing
                End If

                continuteLine = line
            Else
                continuteLine = continuteLine & " " & line
            End If
        Next

        If Not continuteLine Is Nothing Then
            list.Add(continuteLine)
        End If

        Return list.ToArray
    End Function

    Private Shared Function ParseDocument(lines As String()) As Document
        Dim title As String = lines(Scan0).Trim
        Dim tagsData As Dictionary(Of String, String()) = lines _
            .Skip(1) _
            .Select(Function(line)
                        Return line.GetTagValue(, trim:=True)
                    End Function) _
            .GroupBy(Function(a) a.Name) _
            .ToDictionary(Function(a) a.Key,
                          Function(g)
                              Return g.Select(Function(t) t.Value).ToArray
                          End Function)

        Return New Document With {
            .author = tagsData.TryGetValue("@author"),
            .title = title,
            .description = tagsData.TryGetValue("@description").JoinBy(vbCrLf),
            .details = tagsData.TryGetValue("@details").JoinBy(vbCrLf),
            .keywords = tagsData.TryGetValue("@keywords").SafeQuery.ToArray,
            .returns = tagsData.TryGetValue("@return").DefaultFirst,
            .parameters = tagsData.TryGetValue("@param") _
                .SafeQuery _
                .Select(Function(line)
                            Dim tag = line.GetTagValue(trim:=True)
                            Dim val As New NamedValue With {
                                .name = tag.Name,
                                .text = tag.Value
                            }

                            Return val
                        End Function) _
                .ToArray
        }
    End Function
End Class
