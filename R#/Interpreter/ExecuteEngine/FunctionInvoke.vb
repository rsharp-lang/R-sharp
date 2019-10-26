Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class FunctionInvoke : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim funcName As String
        Dim parameters As Expression()

        Sub New(tokens As Token())
            funcName = tokens(Scan0).text
            parameters = tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .ToArray _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Select(Function(param) Expression.CreateExpression(param)) _
                .ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim paramVals As Object() = parameters _
                .Select(Function(a) a.Evaluate(envir)) _
                .ToArray
            Dim funcVar As Variable = envir(funcName)
            Dim func As RMethodInfo = funcVar.value

            Return func.Invoke(paramVals)
        End Function
    End Class
End Namespace