Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class Require : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Property packages As Expression()

        Sub New(names As Token())
            packages = names _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(b) Not b.isComma) _
                .Select(AddressOf Expression.CreateExpression) _
                .ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace