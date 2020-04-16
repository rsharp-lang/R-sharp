Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Interop

    Public Class BinaryIndex : Implements IReadOnlyId

        ''' <summary>
        ''' the operator symbol text
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property symbol As String Implements IReadOnlyId.Identity

        ReadOnly operators As New List(Of BinaryOperator)

        ''' <summary>
        ''' key=hascode1|hashcode2
        ''' </summary>
        ReadOnly hashIndexCache As New Dictionary(Of String, BinaryOperator)

        Sub New(symbol As String)
            Me.symbol = symbol
        End Sub

        Public Sub addOperator(left As RType, right As RType, [operator] As IBinaryOperator, env As Environment)
            Dim hashKey As String = $"{left.GetHashCode}|{right.GetHashCode}"
            Dim bin As New BinaryOperator([operator]) With {
                .left = left,
                .right = right,
                .operatorSymbol = symbol
            }

            If hashIndexCache.ContainsKey(hashKey) Then
                If Not env Is Nothing Then
                    Call env.AddMessage({
                        $"operator '{hashKey}' is replace by {bin}",
                        $"hash key: {hashKey}",
                        $"binary: {bin}"
                    }, MSG_TYPES.WRN)
                End If

                hashIndexCache.Remove(hashKey)
            End If

            operators.Add(bin)
            hashIndexCache.Add(hashKey, bin)
        End Sub

        Public Function Evaluate(left As Object, right As Object, env As Environment) As Object
            If left Is Nothing Then
                If Not right Is Nothing Then
                    Return leftNull(right, env)
                Else
                    Return noneValue(env)
                End If
            ElseIf right Is Nothing Then
                Return rightNull(left, env)
            Else
                Dim t1 As RType = left.GetType.DoCall(AddressOf RType.GetRSharpType)
                Dim t2 As RType = right.GetType.DoCall(AddressOf RType.GetRSharpType)
                Dim hashKey As String = $"{t1.GetHashCode}|{t2.GetHashCode}"

                If hashIndexCache.ContainsKey(hashKey) Then
                    Return hashIndexCache(hashKey).operation(left, right, env)
                Else
                    ' do type match and then create hashKey index cache
                    For Each op As BinaryOperator In operators
                        If t1 Like op.left AndAlso t2 Like op.right Then
                            hashIndexCache.Add(hashKey, op)
                            Return hashIndexCache(hashKey).operation(left, right, env)
                        End If
                    Next

                    Return Internal.debug.stop({
                        $"operator symbol '{symbol}' is not defined for binary expression ({t1} {symbol} {t2})",
                        $"symbol: {symbol}",
                        $"typeof left: {t1}",
                        $"typeof right: {t2}"
                    }, env)
                End If
            End If
        End Function

        Private Function rightNull(left As Object, env As Environment) As Object
            Dim t As RType = left.GetType.DoCall(AddressOf RType.GetRSharpType)

            For Each op As BinaryOperator In operators
                If t Like op.left Then
                    Return op.operation(left, Nothing, env)
                End If
            Next

            Return Internal.debug.stop({
                $"operator symbol '{symbol}' is not defined for binary expression ({t} {symbol} NA)",
                $"symbol: {symbol}",
                $"typeof left: {t}",
                $"typeof right: NA"
            }, env)
        End Function

        Private Function noneValue(env As Environment) As Object
            Dim tVoid As RType = RType.GetRSharpType(GetType(Void))
            Dim hashKey As String = $"{tVoid.GetHashCode}|{tVoid.GetHashCode}"

            If hashIndexCache.ContainsKey(hashKey) Then
                Return hashIndexCache(hashKey).operation(Nothing, Nothing, env)
            Else
                Return Internal.debug.stop({
                     $"operator symbol '{symbol}' is not defined for binary expression (NULL {symbol} NULL)",
                     $"symbol: {symbol}"
                }, env)
            End If
        End Function

        Private Function leftNull(right As Object, env As Environment) As Object
            Dim t As RType = right.GetType.DoCall(AddressOf RType.GetRSharpType)

            For Each op As BinaryOperator In operators
                If t Like op.right Then
                    Return op.operation(Nothing, right, env)
                End If
            Next

            Return Internal.debug.stop({
                $"operator symbol '{symbol}' is not defined for binary expression (NA {symbol} {t})",
                $"symbol: {symbol}",
                $"typeof left: NA",
                $"typeof right: {t}"
            }, env)
        End Function
    End Class
End Namespace