#Region "Microsoft.VisualBasic::9adf9990b18273bdcd4656d3f996d1b6, G:/GCModeller/src/R-sharp/R#//System/Document/FunctionDeclare.vb"

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

    '   Total Lines: 102
    '    Code Lines: 84
    ' Comment Lines: 0
    '   Blank Lines: 18
    '     File Size: 4.29 KB


    '     Class FunctionDeclare
    ' 
    '         Properties: name, parameters, sourceMap
    ' 
    '         Function: GetArgument, (+2 Overloads) ToString, valueText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

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

            Static literals As Index(Of String) = {"NULL", "NA", "TRUE", "FALSE", "Inf", "NaN"}

            If optionals.Length > 0 Then
                part2 = optionals _
                    .Select(Function(a)
                                Dim valHtml As String = a.text

                                If Not valHtml.StringEmpty Then
                                    If (valHtml.First = """"c AndAlso valHtml.Last = """"c) OrElse
                                       (valHtml.First = "`"c AndAlso valHtml.Last = "`"c) OrElse
                                       (valHtml.First = "'"c AndAlso valHtml.Last = "'"c) Then

                                        Dim isColor As Boolean = False
                                        Dim color As Color = valHtml.Trim(""""c, "'"c, "`"c).TranslateColor(throwEx:=False, success:=isColor)

                                        If isColor Then
                                            valHtml = $"<span style='color: {color.ToHtmlColor};'><strong>{valHtml}</strong></span>"
                                        Else
                                            valHtml = $"<span style='color: brown;'><strong>{valHtml}</strong></span>"
                                        End If
                                    End If
                                Else
                                    valHtml = "NULL"
                                End If

                                If valHtml Like literals Then
                                    valHtml = $"<span style='color: blue;'>{valHtml}</span>"
                                End If

                                Return $"    <i>{a.name}</i> = {valHtml}"
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
