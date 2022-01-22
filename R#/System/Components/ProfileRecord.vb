Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Development.Components

    Public Class ProfileRecord

        Public Property tag As Long
        Public Property elapse_time As Long

        ''' <summary>
        ''' memory delta size in bytes unit ``MB``.
        ''' </summary>
        ''' <returns></returns>
        Public Property memory_delta As Double
        Public Property stackframe As StackFrame

    End Class

    Public Class ProfilerFrames

        Public Property timestamp As Double
        Public Property traceback As StackFrame
        Public Property profiles As ProfileRecord()

    End Class
End Namespace