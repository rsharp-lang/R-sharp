Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.ExecuteEngine

    Public Class WhileLoop : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return loopBody.type
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Dim test As Expression
        Dim loopBody As ClosureExpression

        Sub New(test As Expression, loopBody As ClosureExpression, stackframe As StackFrame)
            Me.stackFrame = stackframe
            Me.test = test
            Me.loopBody = loopBody
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim result As New List(Of Object)
            Dim test As Object
            Dim value As Object
            Dim env As New Environment(envir, stackFrame, isInherits:=False)

            Do While True
                test = Me.test.Evaluate(envir)

                If Program.isException(test) Then
                    Return test
                ElseIf False = asLogical(test)(Scan0) Then
                    Exit Do
                Else
                    value = Me.loopBody.Evaluate(env)
                End If

                If Program.isException(value) Then
                    Return value
                Else
                    result.Add([single](value))
                End If
            Loop

            Return result.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"do while ({test}){{
    {loopBody}
}}"
        End Function
    End Class
End Namespace