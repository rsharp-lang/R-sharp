Imports Microsoft.VisualBasic.MIME.application.xml.MathML
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Friend Module Compiler

    Public Function GetLambda(exp As FormulaExpression) As LambdaExpression

    End Function

    Public Function GetLambda(exp As DeclareNewFunction) As LambdaExpression

    End Function

    Public Function GetLambda(exp As DeclareLambdaFunction) As LambdaExpression

    End Function

    Public Function GetLambda(exp As String, ParamArray args As String()) As LambdaExpression

    End Function
End Module