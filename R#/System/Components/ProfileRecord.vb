Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Development.Components

    Public Class ProfileRecord

        Public Property tag As Long
        Public Property elapse_time As Long
        Public Property memory_delta As Double
        Public Property stackframe As StackFrame

    End Class
End Namespace