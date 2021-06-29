Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development

    Public Module LambdaCompiler

        <Extension>
        Public Function Compile(lambda As DeclareLambdaFunction) As Expressions.LambdaExpression
            Dim parameters As Dictionary(Of String, Expressions.ParameterExpression) = lambda.parameterNames _
                .ToDictionary(Function(name) name,
                              Function(name)
                                  Return Expressions.Expression.Parameter(GetType(Double), name)
                              End Function)
            Dim body As Expressions.Expression = CreateExpression(parameters, lambda.closure)
            Dim run As Expressions.LambdaExpression = Expressions.Expression.Lambda(body, parameters.Values.ToArray)

            Return run
        End Function

        <Extension>
        Private Function CreateExpression(parameters As Dictionary(Of String, Expressions.ParameterExpression), model As Expression) As Expressions.Expression
            Select Case model.GetType
                Case GetType(Literal)
                    Dim literal As Literal = DirectCast(model, Literal)
                    Dim constVal As Object = literal.Evaluate(Nothing)
                    Dim type As Type = If(constVal Is Nothing, GetType(Object), constVal.GetType)

                    Return Expressions.Expression.Constant(constVal, type)
                Case GetType(SymbolReference)
                    Dim ref As SymbolReference = model

                    If parameters.ContainsKey(ref.symbol) Then
                        Return parameters(ref.symbol)
                    Else
                        ' 引用的是环境中的其他变量

                    End If
                Case GetType(BinaryExpression)
                    Dim bin As BinaryExpression = DirectCast(model, BinaryExpression)
                Case Else

            End Select

            Throw New NotImplementedException(model.GetType().FullName)
        End Function
    End Module
End Namespace