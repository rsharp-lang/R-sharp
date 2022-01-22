Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ValueTypes

Namespace Development.Components

    Public Class ProfileRecord

        Public Property tag As Long
        Public Property elapse_time As Long

        ''' <summary>
        ''' memory delta size in bytes unit ``MB``.
        ''' </summary>
        ''' <returns></returns>
        Public Property memory_delta As Double
        Public Property memory_size As Double
        Public Property stackframe As StackFrame

        Public Overrides Function ToString() As String
            Return $"[{tag.FromUnixTimeStamp.ToString}] elapse_time {TimeSpan.FromTicks(elapse_time).FormatTime}, memory delta {memory_delta.ToString("F2")}MB, memory usage {memory_size.ToString("F2")}MB at {stackframe}"
        End Function

    End Class

    Public Class ProfilerFrames

        Public Property timestamp As Double
        Public Property traceback As StackFrame
        Public Property profiles As ProfileRecord()

        Public ReadOnly Property size As Integer
            Get
                Return profiles.Length
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{profiles.Length} frames at {CLng(timestamp).FromUnixTimeStamp.ToString} | {traceback.ToString}"
        End Function

    End Class
End Namespace