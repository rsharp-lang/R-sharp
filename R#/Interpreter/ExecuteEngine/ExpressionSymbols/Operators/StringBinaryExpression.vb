#Region "Microsoft.VisualBasic::d1d27122ad2960535157ddcbdd398ca7, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\StringBinaryExpression.vb"

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

    '   Total Lines: 98
    '    Code Lines: 79
    ' Comment Lines: 2
    '   Blank Lines: 17
    '     File Size: 3.97 KB


    '     Module StringBinaryExpression
    ' 
    '         Function: DoStringBinary, getStringArray, StringBinaryOperator, TextEquivalency
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Text.Patterns
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Module StringBinaryExpression

        Private Function TextEquivalency(a As Object, b As Object) As Boolean()
            Dim text As String()
            Dim r As Regex

            If TypeOf a Is Regex Then
                r = DirectCast(a, Regex)
                text = getStringArray(b).ToArray
            ElseIf TypeOf b Is Regex Then
                r = DirectCast(b, Regex)
                text = getStringArray(a).ToArray
            Else
                Return DoStringBinary(Of Boolean)(a, b, Function(x, y) x = y)
            End If

            Return text _
                .Select(Function(s) s.IsPattern(r)) _
                .ToArray
        End Function

        Public Function StringBinaryOperator(env As Environment, a As Object, b As Object, [operator] As String) As Object
            Select Case [operator]
                Case "&" : Return DoStringBinary(Of String)(a, b, Function(x, y) x & y)
                Case "==" : Return TextEquivalency(a, b)
                Case "!=" : Return TextEquivalency(a, b).Select(Function(t) Not t).ToArray
                Case "like"
                    Dim text As String() = getStringArray(a).ToArray

                    If Runtime.isVector(Of String)(b) Then
                        ' <string-for-test> like wildcard_patterns
                        Dim patterns As String() = getStringArray(b).ToArray
                        Dim likePattern = Function(txt, wildcard)
                                              Return DirectCast(wildcard, String).WildcardMatch(DirectCast(txt, String))
                                          End Function

                        Return Vectorization _
                           .BinaryCoreInternal(Of String, String, Boolean)(text, patterns, likePattern) _
                           .ToArray
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

        Public Function DoStringBinary(Of Out)(a As Object, b As Object, op As Func(Of Object, Object, Object)) As Out()
            Dim va = getStringArray(a).ToArray
            Dim vb = getStringArray(b).ToArray

            Return Vectorization _
                .BinaryCoreInternal(Of String, String, Out)(va, vb, op) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function getStringArray(a As Object) As IEnumerable(Of String)
            Return From element As Object
                   In Runtime _
                       .asVector(Of Object)(a) _
                       .AsQueryable
                   Let str As String = any.ToString(element, "NULL")
                   Select str
        End Function
    End Module
End Namespace
