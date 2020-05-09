Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' ``new xxx(...)``
    ''' </summary>
    Public Class CreateObject : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Sub New()

        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace