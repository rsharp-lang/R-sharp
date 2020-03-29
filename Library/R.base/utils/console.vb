#Region "Microsoft.VisualBasic::7081e654296aa5d3249cc6d0312a75e3, Library\R.base\utils\console.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    ' Module console
    ' 
    '     Function: ConsoleBackColor, ConsoleForeColor, CreateProgressBar, log, PinProgressBarTop
    ' 
    '     Sub: ClearProgressBarPinned
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports CMD = System.Console

''' <summary>
''' R# console utilities
''' </summary>
<Package("console", Category:=APICategories.SoftwareTools)>
Module console

    ''' <summary>
    ''' Writes the specified string value to the standard output stream.
    ''' </summary>
    ''' <param name="message">The message text value to write.</param>
    ''' <param name="fore_color">sets the foreground color of the console.</param>
    ''' <param name="back_color">sets the background color of the console.</param>
    ''' <returns>The message text value</returns>
    <ExportAPI("log")>
    Public Function log(message As String, Optional fore_color As ConsoleColor? = Nothing, Optional back_color As ConsoleColor? = Nothing) As String
        Dim foreBackup = CMD.ForegroundColor
        Dim backBackup = CMD.BackgroundColor

        If Not fore_color Is Nothing Then
            CMD.ForegroundColor = fore_color
        End If
        If Not back_color Is Nothing Then
            CMD.BackgroundColor = back_color
        End If

        Call CMD.Write(sprintf(message))

        CMD.ForegroundColor = foreBackup
        CMD.BackgroundColor = backBackup

        Return message
    End Function

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

    <ExportAPI("fore.color")>
    Public Function ConsoleForeColor(Optional color As ConsoleColor? = Nothing) As String
        If color Is Nothing Then
            color = CMD.ForegroundColor.ToString.ToLower
        Else
            CMD.ForegroundColor = color
        End If

        Return color
    End Function

    <ExportAPI("back.color")>
    Public Function ConsoleBackColor(Optional color As ConsoleColor? = Nothing) As String
        If color Is Nothing Then
            color = CMD.BackgroundColor.ToString.ToLower
        Else
            CMD.BackgroundColor = color
        End If

        Return color
    End Function
End Module
