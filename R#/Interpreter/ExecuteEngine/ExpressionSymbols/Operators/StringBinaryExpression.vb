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