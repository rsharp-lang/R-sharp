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

                End If
            End If
        End Function

        Private Function rightNull(left As Object, env As Environment) As Object

        End Function

        Private Function noneValue(env As Environment) As Object
            Dim tVoid As RType = RType.GetRSharpType(GetType(Void))
            Dim hashKey As String = $"{tVoid.GetHashCode}|{tVoid.GetHashCode}"

            If hashIndexCache.ContainsKey(hashKey) Then
                Return hashIndexCache(hashKey).operation(Nothing, Nothing, env)
            Else
                Return Internal.debug.stop($"operator symbol '{symbol}' is not defined for binary expression (NULL {symbol} NULL)", env)
            End If
        End Function

        Private Function leftNull(right As Object, env As Environment) As Object

        End Function
    End Class
End Namespace