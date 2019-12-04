#Region "Microsoft.VisualBasic::141ffeb74b72d072d2149fe3ecb38719, R.base\utils\console.vb"

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
    '     Function: ConsoleBackColor, ConsoleForeColor, CreateProgressBar, PinProgressBarTop
    ' 
    '     Sub: ClearProgressBarPinned
    ' 
    ' /********************************************************************************/

#End Region


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

