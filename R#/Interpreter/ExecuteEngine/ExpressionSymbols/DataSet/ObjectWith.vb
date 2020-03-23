Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' syntax implementation for ``with`` keyword
    ''' </summary>
    Public Class ObjectWith : Inherits Expression
        Implements IRuntimeTrace

        Dim closure As ClosureExpression
        Dim target As Expression
        Dim isModifyWith As Boolean

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                If isModifyWith Then
                    Return target.type
                Else
                    Return closure.type
                End If
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Sub New(target As Expression, closure As ClosureExpression, isModifyWith As Boolean, stacktrace As StackFrame)
            Me.target = target
            Me.closure = closure
            Me.isModifyWith = isModifyWith
            Me.stackFrame = stacktrace
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim obj As Object = target.Evaluate(envir)

            If Program.isException(obj) Then
                Return obj
            ElseIf obj Is Nothing Then
                envir.AddMessage($"the expression evaluation result of '{target}' is nothing!", MSG_TYPES.WRN)
                Return Nothing
            End If


        End Function
    End Class
End Namespace