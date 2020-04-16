Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    Public Module BinaryOperatorEngine

        ReadOnly index As New Dictionary(Of String, BinaryIndex)

        Sub New()
            Call arithmeticOperators()
        End Sub

        Private Sub arithmeticOperators()
            Dim left, right As RType

            left = RType.GetRSharpType(GetType(Integer))
            right = RType.GetRSharpType(GetType(Integer))

            Call addBinary(left, right, "+", Function(a, b, env) BinaryCoreInternal(Of Integer, Integer, Integer)(a, b, Function(x, y) x + y).ToArray, Nothing)
            Call addBinary(left, right, "-", Function(a, b, env) BinaryCoreInternal(Of Integer, Integer, Integer)(a, b, Function(x, y) x - y).ToArray, Nothing)
            Call addBinary(left, right, "*", Function(a, b, env) BinaryCoreInternal(Of Integer, Integer, Integer)(a, b, Function(x, y) x * y).ToArray, Nothing)
            Call addBinary(left, right, "/", Function(a, b, env) BinaryCoreInternal(Of Integer, Integer, Double)(a, b, Function(x, y) x / y).ToArray, Nothing)
            Call addBinary(left, right, "%", Function(a, b, env) BinaryCoreInternal(Of Integer, Integer, Integer)(a, b, Function(x, y) x Mod y).ToArray, Nothing)
            Call addBinary(left, right, "^", Function(a, b, env) BinaryCoreInternal(Of Integer, Integer, Long)(a, b, Function(x, y) x ^ y).ToArray, Nothing)

            left = RType.GetRSharpType(GetType(Long))
            right = RType.GetRSharpType(GetType(Long))

            Call addBinary(left, right, "+", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(a, b, Function(x, y) x + y).ToArray, Nothing)
            Call addBinary(left, right, "-", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(a, b, Function(x, y) x - y).ToArray, Nothing)
            Call addBinary(left, right, "*", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(a, b, Function(x, y) x * y).ToArray, Nothing)
            Call addBinary(left, right, "/", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Double)(a, b, Function(x, y) x / y).ToArray, Nothing)
            Call addBinary(left, right, "%", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Integer)(a, b, Function(x, y) x Mod y).ToArray, Nothing)
            Call addBinary(left, right, "^", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(a, b, Function(x, y) x ^ y).ToArray, Nothing)

            left = RType.GetRSharpType(GetType(Double))
            right = RType.GetRSharpType(GetType(Double))

            Call addBinary(left, right, "+", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(a, b, Function(x, y) x + y).ToArray, Nothing)
            Call addBinary(left, right, "-", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(a, b, Function(x, y) x - y).ToArray, Nothing)
            Call addBinary(left, right, "*", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(a, b, Function(x, y) x * y).ToArray, Nothing)
            Call addBinary(left, right, "/", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(a, b, Function(x, y) x / y).ToArray, Nothing)
            Call addBinary(left, right, "%", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Integer)(a, b, Function(x, y) x Mod y).ToArray, Nothing)
            Call addBinary(left, right, "^", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(a, b, Function(x, y) x ^ y).ToArray, Nothing)

            left = RType.GetRSharpType(GetType(Single))
            right = RType.GetRSharpType(GetType(Single))

            Call addBinary(left, right, "+", Function(a, b, env) BinaryCoreInternal(Of Single, Single, Single)(a, b, Function(x, y) x + y).ToArray, Nothing)
            Call addBinary(left, right, "-", Function(a, b, env) BinaryCoreInternal(Of Single, Single, Single)(a, b, Function(x, y) x - y).ToArray, Nothing)
            Call addBinary(left, right, "*", Function(a, b, env) BinaryCoreInternal(Of Single, Single, Double)(a, b, Function(x, y) x * y).ToArray, Nothing)
            Call addBinary(left, right, "/", Function(a, b, env) BinaryCoreInternal(Of Single, Single, Single)(a, b, Function(x, y) x / y).ToArray, Nothing)
            Call addBinary(left, right, "%", Function(a, b, env) BinaryCoreInternal(Of Single, Single, Integer)(a, b, Function(x, y) x Mod y).ToArray, Nothing)
            Call addBinary(left, right, "^", Function(a, b, env) BinaryCoreInternal(Of Single, Single, Double)(a, b, Function(x, y) x ^ y).ToArray, Nothing)
        End Sub

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