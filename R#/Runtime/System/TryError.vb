Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Runtime.Components

    ''' <summary>
    ''' try-error
    ''' </summary>
    Public Class TryError

        Public ReadOnly Property [error] As Message
        Public ReadOnly Property stackframe As StackFrame

        Sub New(err As Message, stackframe As StackFrame)
            Me.error = err
            Me.stackframe = stackframe
        End Sub
    End Class
End Namespace