Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging

Namespace Runtime

    ''' <summary>
    ''' The warning message and exception message
    ''' </summary>
    Public Class Message : Implements IEnumerable(Of String)

        Public Property Message As String()
        Public Property MessageLevel As MSG_TYPES
        Public Property EnvironmentStack As StackFrame()
        Public Property Trace As StackFrame()

        Public Iterator Function GetEnumerator() As IEnumerator(Of String) Implements IEnumerable(Of String).GetEnumerator
            For Each msg As String In Message
                Yield msg
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace