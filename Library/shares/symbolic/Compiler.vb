Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.MIME.application.xml.MathML
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports MathExp = Microsoft.VisualBasic.Math.Scripting.MathExpression.Impl.Expression

Friend Module Compiler

    Public Function GetLambda(exp As FormulaExpression) As LambdaExpression

    End Function

    Public Function GetLambda(exp As DeclareNewFunction) As LambdaExpression

    End Function

    Public Function GetLambda(exp As DeclareLambdaFunction) As LambdaExpression

    End Function

    Public Function GetLambda(raw As String, ParamArray args As String()) As LambdaExpression
        Dim exp As MathExp = ExpressionEngine.Parse(DirectCast(raw, String))

    End Function
End Module