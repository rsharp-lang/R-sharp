Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' ``@stop``
    ''' </summary>
    Public Class BreakPoint : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides Function Evaluate(envir As Environment) As Object
            Pause()
            Return envir.last
        End Function

        Public Overrides Function ToString() As String
            Return "[R#.debugger.breakpoint]"
        End Function
    End Class
End Namespace