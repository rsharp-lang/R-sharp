Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    Public Module BinaryOperatorEngine

        ReadOnly index As New Dictionary(Of String, BinaryIndex)

        Public Function getOperator(symbol As String, env As Environment) As [Variant](Of BinaryIndex, Message)
            If index.ContainsKey(symbol) Then
                Return index(symbol)
            Else
                Return Internal.debug.stop({$"missing operator '{symbol}'", $"symbol: {symbol}"}, env)
            End If
        End Function

        Public Sub addBinary(left As RType, right As RType, symbol As String, op As IBinaryOperator, env As Environment)
            If Not index.ContainsKey(symbol) Then
                index.Add(symbol, New BinaryIndex(symbol))
            End If

            Call index(symbol).addOperator(left, right, op, env)
        End Sub

    End Module
End Namespace