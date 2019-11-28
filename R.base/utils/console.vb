
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Terminal.ProgressBar

<Package("console", Category:=APICategories.SoftwareTools)>
Module console

    ''' <summary>
    ''' Pin the top of progress bar
    ''' </summary>
    ''' <param name="top"></param>
    ''' <returns></returns>
    <ExportAPI("progressbar.pin.top")>
    Public Function PinProgressBarTop(Optional top As Integer = -1) As Integer
        If top <= 0 Then
            top = System.Console.CursorTop
        End If

        Call ProgressBar.PinTop(top)

        Return top
    End Function

    Public Sub ClearProgressBarPinned()
        ProgressBar.ClearPinnedTop()
    End Sub
End Module
