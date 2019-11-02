Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal

Namespace Interpreter.ExecuteEngine


    Public Class LinqExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
        Public ReadOnly Property linq As linq

        Sub New(tokens As List(Of Token()))

        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace