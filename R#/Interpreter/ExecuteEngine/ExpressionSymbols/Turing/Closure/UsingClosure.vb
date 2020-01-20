Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class UsingClosure : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly params As DeclareNewVariable
        ReadOnly closure As ClosureExpression

        Sub New(params As DeclareNewVariable, closure As ClosureExpression)
            Me.params = params
            Me.closure = closure
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Using env As New Environment(envir, params.ToString)
                Dim resource As Object = params.Evaluate(env)
                Dim result As Object

                If resource Is Nothing Then
                    Return Internal.stop("Target is nothing!", env)
                ElseIf Program.isException(resource) Then
                    Return resource
                ElseIf Not resource.GetType.ImplementInterface(GetType(IDisposable)) Then
                    Return Message.InCompatibleType(GetType(IDisposable), resource.GetType, env)
                End If

                ' run using closure and get result
                result = closure.Evaluate(env)

                ' then dispose the target
                ' release handle and clean up the resource
                Call DirectCast(resource, IDisposable).Dispose()

                Return result
            End Using
        End Function

        Public Overrides Function ToString() As String
            Return $"using {params} {{
    # using closure internal
    {closure}
}}"
        End Function
    End Class
End Namespace