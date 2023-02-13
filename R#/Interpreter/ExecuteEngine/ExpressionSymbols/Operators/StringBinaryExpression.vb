#Region "Microsoft.VisualBasic::65e6aa5a218e0fe736107faa3a7590d7, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\StringBinaryExpression.vb"

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
'    Code Lines: 82
' Comment Lines: 2
'   Blank Lines: 18
'     File Size: 4.22 KB


'     Module StringBinaryExpression
' 
'         Function: DoStringBinary, getStringArray, StringBinaryOperator, TextEquivalency
' 
' 
' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text.Patterns
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Module StringBinaryExpression

        Private Function TextEquivalency(a As Object, b As Object, env As Environment) As Object
            Dim text As String()
            Dim r As Regex

            If TypeOf a Is Regex Then
                r = DirectCast(a, Regex)
                text = CLRVector.asCharacter(b)
            ElseIf TypeOf b Is Regex Then
                r = DirectCast(b, Regex)
                text = CLRVector.asCharacter(a)
            Else
                Return DoStringBinary(Of Boolean)(a, b, Function(x, y, env2) x = y, env)
            End If

            Return text _
                .Select(Function(s) s.IsPattern(r)) _
                .ToArray
        End Function

        Public Function StringBinaryOperator(env As Environment, a As Object, b As Object, [operator] As String) As Object
            Select Case [operator]
                Case "&" : Return DoStringBinary(Of String)(a, b, Function(x, y, env2) x & y, env)
                Case "==" : Return TextEquivalency(a, b, env)
                Case "!="
                    Dim flagsObj = TextEquivalency(a, b, env)

                    If TypeOf flagsObj Is Message Then
                        Return flagsObj
                    Else
                        Return UnaryNot.Not(flagsObj)
                    End If
                Case "like"
                    Dim text As String() = CLRVector.asCharacter(a)

                    If Runtime.isVector(Of String)(b) Then
                        ' <string-for-test> like wildcard_patterns
                        Dim patterns As String() = CLRVector.asCharacter(b)
                        Dim likePattern = Function(txt, wildcard, env2)
                                              Return DirectCast(wildcard, String).WildcardMatch(DirectCast(txt, String))
                                          End Function

                        Return Vectorization.BinaryCoreInternal(Of String, String, Boolean)(text, patterns, likePattern, env)
                    ElseIf TypeOf b Is Regex Then
                        ' <string-for-test> link regexp_pattern
                        Return text _
                            .Select(Function(s)
                                        Return DirectCast(b, Regex).Match(s).Success
                                    End Function) _
                            .ToArray
                    End If

                Case Else

            End Select

            Dim left As String, right As String

            If TypeOf a Is vector Then
                left = a.ToString
            Else
                left = a.GetType.FullName
            End If

            If TypeOf b Is vector Then
                right = b.ToString
            Else
                right = b.GetType.FullName
            End If

            Return Internal.debug.stop(New NotImplementedException($"[{left}] {[operator]} [{right}]"), env)
        End Function

        Public Function DoStringBinary(Of Out)(a As Object, b As Object, op As op_evaluator, env As Environment) As Object
            Dim va = CLRVector.asCharacter(a)
            Dim vb = CLRVector.asCharacter(b)

            Return Vectorization.BinaryCoreInternal(Of String, String, Out)(va, vb, op, env)
        End Function
    End Module
End Namespace
