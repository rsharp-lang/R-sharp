#Region "Microsoft.VisualBasic::cbb8945e54339e6f022cef04d94bf49c, studio\Rsharp_kit\roxygenNet\RoxygenDocument.vb"

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

'   Total Lines: 155
'    Code Lines: 119 (76.77%)
' Comment Lines: 16 (10.32%)
'    - Xml Docs: 62.50%
' 
'   Blank Lines: 20 (12.90%)
'     File Size: 6.06 KB


' Class RoxygenDocument
' 
'     Function: continuteLines, ParseDocument, ParseDocuments, SplitBlocks
' 
' /********************************************************************************/

#End Region

Imports System.IO
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

    ''' <summary>
    ''' parse the help document content for symbols from a given script text
    ''' </summary>
    ''' <param name="R">the script text data</param>
    ''' <returns></returns>
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

    ''' <summary>
    ''' parse the help documents
    ''' </summary>
    ''' <param name="scriptText">the R# script text</param>
    ''' <returns>a collection of the tuple of [symbol => help document]</returns>
    Private Shared Iterator Function SplitBlocks(scriptText As String) As IEnumerable(Of NamedValue(Of Document))
        Dim buffer As New List(Of String)
        Dim offset As Integer = 0

        For Each line As String In scriptText.LineIterators
            offset += 1

            If Strings.Trim(line).StringEmpty Then
                Continue For
            ElseIf line.StartsWith("#'") Then
                buffer.Add(line.Substring(2).Trim)
            ElseIf Strings.InStr(line, "function") < 1 Then
                ' 20250519
                ' is attribute data
                ' example as [@app "xxxx"]
                ' skip this line
                Continue For
            ElseIf buffer > 0 Then
                Dim name As String() = line.Trim _
                    .Split _
                    .Where(Function(s) Not s.StringEmpty) _
                    .ToArray
                Dim newLines As String() = buffer.PopAll _
                    .Where(Function(str) Not str.StringEmpty) _
                    .ToArray

                If newLines.IsNullOrEmpty OrElse name.IsNullOrEmpty Then
                    Throw New InvalidDataException($"ROxygon document parser error: {vbCrLf}{vbCrLf} {scriptText} {vbCrLf}{vbCrLf} at line: {offset}.")
                End If

                Dim title As String = newLines(0)
                Dim func_name As String = If(name(1) = "=", name(0), name(1))

                If title.StartsWith("@") Then
                    ' no title
                    title = func_name
                    newLines = continuteLines(newLines)
                Else
                    newLines = newLines.Skip(1).DoCall(AddressOf continuteLines)
                End If

                If newLines.Length = 0 Then
                    Continue For
                End If

                ' const xxx as function -> name(1)
                ' xxx = function -> name(0)
                Yield New NamedValue(Of Document) With {
                    .Name = func_name,
                    .Value = ParseDocument(title, newLines)
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

    Private Shared Function ParseDocument(title As String, lines As String()) As Document
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
                .ToArray,
            .examples = tagsData.TryGetValue("@examples").JoinBy(vbCrLf),
            .see_also = tagsData.TryGetValue("@seealso").JoinBy(vbCrLf)
        }
    End Function
End Class
