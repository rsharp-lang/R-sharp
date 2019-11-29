
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Terminal.ProgressBar
Imports CMD = System.Console

<Package("console", Category:=APICategories.SoftwareTools)>
Module console

    <ExportAPI("progressbar")>
    Public Function CreateProgressBar(title$, Optional Y% = -1, Optional CLS As Boolean = False, Optional theme As ColorTheme = Nothing) As ProgressBar
        Return New ProgressBar(title, Y Or CMD.CursorTop.When(Y <= 0), CLS, theme)
    End Function

    ''' <summary>
    ''' Pin the top of progress bar
    ''' </summary>
    ''' <param name="top"></param>
    ''' <returns></returns>
    <ExportAPI("progressbar.pin.top")>
    Public Function PinProgressBarTop(Optional top As Integer = -1) As Integer
        If top <= 0 Then
            top = CMD.CursorTop
        End If

        Call ProgressBar.PinTop(top)

        Return top
    End Function

    <ExportAPI("progressbar.pin.clear")>
    Public Sub ClearProgressBarPinned()
        ProgressBar.ClearPinnedTop()
    End Sub

    ReadOnly names As Dictionary(Of String, ConsoleColor) = Enums(Of ConsoleColor).ToDictionary(Function(cl) cl.ToString.ToLower)

    <ExportAPI("fore.color")>
    Public Function ConsoleForeColor(Optional color$ = Nothing) As String
        If color.StringEmpty Then
            color = CMD.ForegroundColor.ToString.ToLower
        Else
            CMD.ForegroundColor = names.TryGetValue(color.ToLower, [default]:=ConsoleColor.White)
        End If

        Return color
    End Function

    <ExportAPI("back.color")>
    Public Function ConsoleBackColor(Optional color$ = Nothing) As String
        If color.StringEmpty Then
            color = CMD.BackgroundColor.ToString.ToLower
        Else
            CMD.BackgroundColor = names.TryGetValue(color.ToLower, [default]:=ConsoleColor.White)
        End If

        Return color
    End Function
End Module
