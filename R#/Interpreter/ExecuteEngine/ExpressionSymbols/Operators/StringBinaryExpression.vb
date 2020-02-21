#Region "Microsoft.VisualBasic::bbd07d6cbd2acd8f7287c3e420d897c5, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\StringBinaryExpression.vb"

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

    '     Module StringBinaryExpression
    ' 
    '         Function: DoStringBinary, getStringArray, StringBinaryOperator
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text.RegularExpressions
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Module StringBinaryExpression

        Public Function StringBinaryOperator(env As Environment, a As Object, b As Object, [operator] As String) As Object
            Select Case [operator]
                Case "&" : Return DoStringBinary(Of String)(a, b, Function(x, y) x & y)
                Case "==" : Return DoStringBinary(Of Boolean)(a, b, Function(x, y) x = y)
                Case "!=" : Return DoStringBinary(Of Boolean)(a, b, Function(x, y) x <> y)
                Case "like"
                    ' <string-for-test> like regex_patterns
                    Dim patterns As Regex() = getStringArray(b) _
                        .Select(Function(s) New Regex(s)) _
                        .ToArray
                    Dim likePattern = Function(txt, r)
                                          Return DirectCast(r, Regex).Match(DirectCast(txt, String), RegexICSng).Success
                                      End Function
                    Dim text As String() = getStringArray(a).ToArray

                    Return Runtime.Core _
                        .BinaryCoreInternal(Of String, Regex, Boolean)(text, patterns, likePattern) _
                        .ToArray
                Case Else

            End Select

            Return Internal.stop(New NotImplementedException($"<{a.GetType.FullName}> {[operator]} <{b.GetType.FullName}>"), env)
        End Function

        Public Function DoStringBinary(Of Out)(a As Object, b As Object, op As Func(Of Object, Object, Object)) As Out()
            Dim va = getStringArray(a).ToArray
            Dim vb = getStringArray(b).ToArray

            Return Runtime.Core _
                .BinaryCoreInternal(Of String, String, Out)(va, vb, op) _
                .ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function getStringArray(a As Object) As IEnumerable(Of String)
            Return From element As Object
                   In Runtime _
                       .asVector(Of Object)(a) _
                       .AsQueryable
                   Let str As String = Scripting.ToString(element, "NULL")
                   Select str
        End Function
    End Module
End Namespace
