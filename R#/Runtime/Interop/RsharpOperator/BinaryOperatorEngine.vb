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
            Dim numerics As RType() = New Type() {
                GetType(Long),
                GetType(Double)
            }.Select(AddressOf RType.GetRSharpType) _
             .ToArray

            Call addIntegerOperators()
            Call addFloatOperators()

            For Each left As RType In numerics
                For Each right As RType In numerics
                    Call addMixedOperators(left, right)
                Next
            Next
        End Sub

        Private Sub addFloatOperators()
            Dim left As RType = RType.GetRSharpType(GetType(Double))
            Dim right As RType = RType.GetRSharpType(GetType(Double))

            Call addBinary(left, right, "+", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) + DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "-", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) - DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "*", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) * DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "/", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) / DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "%", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) Mod DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "^", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) ^ DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "<", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) < DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, ">", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) > DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "<=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) <= DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, ">=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) >= DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "==", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) = DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "!=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) <> DirectCast(y, Double)).ToArray, Nothing)
        End Sub

        Private Sub addIntegerOperators()
            Dim left As RType = RType.GetRSharpType(GetType(Long))
            Dim right As RType = RType.GetRSharpType(GetType(Long))

            Call addBinary(left, right, "+", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) + DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "-", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) - DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "*", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) * DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "/", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Double)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) / DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "%", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) Mod DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "^", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Double)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) ^ DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "<", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) < DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, ">", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) > DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "<=", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) <= DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, ">=", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) >= DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "==", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) = DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "!=", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) <> DirectCast(y, Long)).ToArray, Nothing)
        End Sub

        Private Sub addMixedOperators(left As RType, right As RType)
            Call addBinary(left, right, "+", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) + DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "-", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) - DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "*", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) * DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "/", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) / DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "%", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) Mod DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "^", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) ^ DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "<", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) < DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, ">", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) > DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "<=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) <= DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, ">=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) >= DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "==", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) = DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "!=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) <> DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
        End Sub

        Public Function getOperator(symbol As String, env As Environment) As [Variant](Of BinaryIndex, Message)
            If index.ContainsKey(symbol) Then
                Return index(symbol)
            Else
                Return Internal.debug.stop({$"missing operator '{symbol}'", $"symbol: {symbol}"}, env)
            End If
        End Function

        Public Sub addBinary(left As RType, right As RType, symbol As String, op As IBinaryOperator, env As Environment, Optional [overrides] As Boolean = True)
            If Not index.ContainsKey(symbol) Then
                index.Add(symbol, New BinaryIndex(symbol))
            End If

            If Not [overrides] AndAlso index(symbol).hasOperator(left, right) Then
                Return
            Else
                Call index(symbol).addOperator(left, right, op, env)
            End If
        End Sub

    End Module
End Namespace