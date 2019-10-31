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

        Public Shared Function SymbolNotFound(envir As Environment, symbolName$, type As TypeCodes) As Message
            Dim exception$

            Select Case type
                Case TypeCodes.closure
                    exception = $"Not able to found any invokable symbol '{symbolName}' in environment stack: [{envir.ToString}]"
                Case Else
                    exception = $"Unable found symbol '{symbolName}' in environment stack: [{envir.ToString}]"
            End Select

            Dim messages As String() = {
                exception,
                "environment: " & envir.ToString,
                "symbol: " & symbolName
            }

            Return Internal.stop(messages, envir)
        End Function
    End Class
End Namespace