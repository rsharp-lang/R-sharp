Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' ``data[, selector]``
    ''' </summary>
    Public Class DataFrameProjection : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace