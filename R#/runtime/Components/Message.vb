Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Namespace Runtime

    ''' <summary>
    ''' The warning message and exception message
    ''' </summary>
    Public Class Message

        Public Property Message As String()
        Public Property MessageLevel As MSG_TYPES
        Public Property StackTrace As StackFrame()

    End Class
End Namespace