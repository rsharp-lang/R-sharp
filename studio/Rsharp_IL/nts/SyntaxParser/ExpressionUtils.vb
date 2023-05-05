Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Module ExpressionUtils

    Public Function GetPackageModules(mods As Expression) As VectorLiteral
        Select Case mods.GetType
            Case GetType(ClosureExpression)
                Dim closure As ClosureExpression = DirectCast(mods, ClosureExpression)
                Dim lines = closure.program _
                    .Select(AddressOf ExpressionUtils.ToString) _
                    .ToArray

                Return New VectorLiteral(lines)
            Case GetType(VectorLiteral)
                Return New VectorLiteral(DirectCast(mods, VectorLiteral).Select(AddressOf ExpressionUtils.ToString))
            Case Else
                Return New VectorLiteral({ExpressionUtils.ToString(mods)})
        End Select
    End Function

    ''' <summary>
    ''' convert any expression to string literal value
    ''' [works for the js imports expression]
    ''' </summary>
    ''' <param name="exp"></param>
    ''' <returns></returns>
    Private Function ToString(exp As Expression) As Literal
        If TypeOf exp Is Literal Then
            Return exp
        ElseIf TypeOf exp Is SymbolReference Then
            Return New Literal(ValueAssignExpression.GetSymbol(exp))
        Else
            Throw New NotImplementedException
        End If
    End Function
End Module
