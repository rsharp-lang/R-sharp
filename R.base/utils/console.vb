
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

    <ExportAPI("progressbar.pin.clear")>
    Public Sub ClearProgressBarPinned()
        ProgressBar.ClearPinnedTop()
    End Sub

    ReadOnly names As Dictionary(Of String, ConsoleColor) = Enums(Of ConsoleColor).ToDictionary(Function(cl) cl.ToString.ToLower)

    <ExportAPI("fore.color")>
    Public Function ConsoleForeColor(Optional color$ = Nothing) As String
        If color.StringEmpty Then
            color = System.Console.ForegroundColor.ToString.ToLower
        Else
            System.Console.ForegroundColor = names.TryGetValue(color.ToLower, [default]:=ConsoleColor.White)
        End If

        Return color
    End Function

    <ExportAPI("back.color")>
    Public Function ConsoleBackColor(Optional color$ = Nothing) As String
        If color.StringEmpty Then
            color = System.Console.BackgroundColor.ToString.ToLower
        Else
            System.Console.BackgroundColor = names.TryGetValue(color.ToLower, [default]:=ConsoleColor.White)
        End If

        Return color
    End Function
End Module
