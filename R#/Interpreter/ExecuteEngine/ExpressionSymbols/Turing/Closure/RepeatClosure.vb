Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    Public Class RepeatClosure : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return closure.type
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        ReadOnly closure As ClosureExpression

        Sub New(closure As ClosureExpression, stackframe As StackFrame)
            Me.closure = closure
            Me.stackFrame = stackframe
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim break As Boolean = False
            Dim value As Object = Nothing

            Using env As New Environment(envir, stackFrame, isInherits:=False)
                Do While Not break
                    value = closure.Evaluate(env)

                    If Program.isException(value) Then
                        Return value
                    End If


                Loop
            End Using

            Return value
        End Function
    End Class
End Namespace