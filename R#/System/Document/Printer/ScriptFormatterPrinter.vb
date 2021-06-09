Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development

    Public Module ScriptFormatterPrinter

        Public Function Format(literal As Literal) As String
            Select Case literal.type
                Case TypeCodes.boolean : Return literal.Evaluate(Nothing).ToString.ToUpper
                Case TypeCodes.double, TypeCodes.integer : Return literal.Evaluate(Nothing).ToString
                Case TypeCodes.string : Return literal.Evaluate(Nothing).ToString
                Case Else

                    If literal.isNull Then
                        Return "NULL"
                    Else
                        Throw New InvalidCastException
                    End If

            End Select
        End Function

        Public Function Format(funCall As FunctionInvoke) As String
            Dim args As String() = funCall.parameters _
                .Select(AddressOf Format) _
                .ToArray
            Dim refName As String = Format(funCall.funcName)

            Return $"{refName}({args.JoinBy(", ")})"
        End Function

        Public Function Format(assign As ValueAssignExpression) As String
            Dim symbol As String() = assign.targetSymbols.Select(AddressOf Format).ToArray
            Dim symbolText As String

            If symbol.Length = 1 Then
                symbolText = symbol(Scan0)
            Else
                symbolText = $"[{symbol.JoinBy(", ")}]"
            End If

            Return $"{symbolText} = {Format(assign.value)}"
        End Function

        Public Function Format(ref As SymbolIndexer) As String
            Select Case ref.indexType
                Case SymbolIndexers.nameIndex
                    Return $"{Format(ref.symbol)}[[{Format(ref.index)}]]"
                Case SymbolIndexers.vectorIndex
                    Return $"{Format(ref.symbol)}[{Format(ref.index)}]"
                Case SymbolIndexers.dataframeRows
                    Return $"{Format(ref.symbol)}[{Format(ref.index)}, ]"
                Case SymbolIndexers.dataframeColumns
                    Return $"{Format(ref.symbol)}[, {Format(ref.index)}]"
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

        Public Function Format(expr As Expression) As String
            Select Case expr.GetType
                Case GetType(Literal) : Return Format(DirectCast(expr, Literal))
                Case GetType(VectorLiteral) : Return $"[{DirectCast(expr, VectorLiteral).Select(AddressOf Format).JoinBy(", ")}]"
                Case GetType(SymbolReference) : Return DirectCast(expr, SymbolReference).symbol
                Case GetType(FunctionInvoke) : Return Format(DirectCast(expr, FunctionInvoke))
                Case GetType(ValueAssignExpression) : Return Format(DirectCast(expr, ValueAssignExpression))
                Case GetType(SymbolIndexer) : Return Format(DirectCast(expr, SymbolIndexer))
                Case Else
                    Throw New NotImplementedException(expr.ToString)
            End Select
        End Function
    End Module
End Namespace