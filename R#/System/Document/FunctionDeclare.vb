﻿#Region "Microsoft.VisualBasic::e6bcc0829366b4af451dedaff3d3546c, R#\System\Document\FunctionDeclare.vb"

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

    '   Total Lines: 165
    '    Code Lines: 121 (73.33%)
    ' Comment Lines: 19 (11.52%)
    '    - Xml Docs: 26.32%
    ' 
    '   Blank Lines: 25 (15.15%)
    '     File Size: 6.45 KB


    '     Class FunctionDeclare
    ' 
    '         Properties: name, parameters, sourceMap
    ' 
    '         Function: GetArgument, RenderDefaultValueColor, (+2 Overloads) RenderValueColor, (+2 Overloads) ToString, valueText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language

Namespace Development

    Public Class FunctionDeclare

        Public Property name As String
        Public Property parameters As NamedValue()
        Public Property sourceMap As StackFrame

        Public Overrides Function ToString() As String
            Return ToString(html:=False)
        End Function

        Public Overloads Function ToString(html As Boolean) As String
            Dim required = parameters.Where(Function(a) a.text.StringEmpty).ToArray
            Dim optionals = parameters.Where(Function(a) Not a.text.StringEmpty).ToArray
            Dim part1 As String
            Dim part2 As String = ""

            If html Then
                part1 = $"<strong>{name}</strong>({required _
                    .Select(Function(a)
                                Return $"<i>{a.name}</i>"
                            End Function) _
                    .JoinBy(", ")}"
            Else
                part1 = $"{name}({required _
                    .Select(Function(a)
                                Return a.name
                            End Function) _
                    .JoinBy(", ")}"
            End If

            If optionals.Length > 0 Then
                part2 = optionals _
                    .Select(Function(a)
                                Return RenderValueColor(a)
                            End Function) _
                    .JoinBy("," & vbCrLf)

                If required.IsNullOrEmpty Then
                    Return $"{part1}{vbCrLf}{part2})"
                Else
                    Return $"{part1},{vbCrLf}{part2})"
                End If
            Else
                Return $"{part1})"
            End If
        End Function

        ''' <summary>
        ''' rendering the color for the default value
        ''' </summary>
        ''' <param name="a"></param>
        ''' <returns></returns>
        Private Shared Function RenderValueColor(a As NamedValue) As String
            Dim valHtml As String = a.text
            valHtml = RenderValueColor(valHtml)
            Return $"    <i>{a.name}</i> = {valHtml}"
        End Function

        Private Shared Function RenderDefaultValueColor(valHtml As String) As String
            ' the given string is in string literal value
            ' check for html color string or 
            ' just a simple string literal
            If (valHtml.First = """"c AndAlso valHtml.Last = """"c) OrElse
               (valHtml.First = "`"c AndAlso valHtml.Last = "`"c) OrElse
               (valHtml.First = "'"c AndAlso valHtml.Last = "'"c) Then

                Dim isColor As Boolean = False
                Dim color As Color = valHtml.Trim(""""c, "'"c, "`"c).TranslateColor(throwEx:=False, success:=isColor)

                If isColor Then
                    ' current string value is color literal string
                    ' rendering color as its literal value
                    valHtml = $"<span style='color: {color.ToHtmlColor};'><strong>{valHtml}</strong></span>"
                Else
                    ' just a string
                    ' rendering color in brown
                    valHtml = $"<span style='color: brown;'><strong>{valHtml}</strong></span>"
                End If
            ElseIf valHtml.IsNumeric(True, True) Then
                ' current default value is a number
                ' render color in green
                valHtml = $"<span style='color: green;'>{valHtml}</span>"
            Else
                ' may be other data type
                ' vector literal?
                ' list literal?
                ' dataframe literal?
                If valHtml.StartsWith("[") Then
                    ' is a vector
                    Dim vector As VectorLiteral = Code.ParseVector(valHtml)
                    Dim sb As New StringBuilder
                    Dim max_width As Integer = 9
                    Dim i As Integer = 0

                    Call sb.Append("[")

                    If vector.length > 0 Then
                        For Each val As Expression In vector.Skip(1)
                            Call sb.Append(RenderDefaultValueColor(valueText(val)))
                            Call sb.Append(",")

                            If i > max_width Then
                                i = 0
                                sb.AppendLine()
                            Else
                                i += 1
                            End If
                        Next
                    End If

                    valHtml = sb.ToString.TrimEnd(","c) & "]"
                End If
            End If

            Return valHtml
        End Function

        Private Shared Function RenderValueColor(valHtml As String) As String
            If Not valHtml.StringEmpty Then
                valHtml = RenderDefaultValueColor(valHtml)
            Else
                valHtml = "NULL"
            End If

            Static literals As Index(Of String) = {"NULL", "NA", "TRUE", "FALSE", "Inf", "NaN"}

            If valHtml Like literals Then
                valHtml = $"<span style='color: blue !important;'>{valHtml}</span>"
            End If

            Return valHtml
        End Function

        Public Shared Function GetArgument(arg As DeclareNewSymbol) As NamedValue
            Dim name As String = arg.names.JoinBy(", ")
            Dim val As String = Nothing

            If arg.hasInitializeExpression Then
                val = valueText(arg.m_value)
            End If

            Return New NamedValue With {
                .name = name.Replace("_", "."),
                .text = val
            }
        End Function

        Private Shared Function valueText(expr As Expression) As String
            Return ScriptFormatterPrinter.Format(expr)
        End Function
    End Class
End Namespace
