Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class ExecutableContext

        ReadOnly environment As Environment

        Sub New(env As Environment)
            environment = env
        End Sub

        Public Sub AddSymbol(symbolName$, type As TypeCodes)
            Call environment.Push(symbolName, Nothing, [readonly]:=False, mode:=type)
        End Sub

        Public Overrides Function ToString() As String
            Return environment.ToString
        End Function
    End Class
End Namespace