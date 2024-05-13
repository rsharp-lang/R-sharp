#Region "Microsoft.VisualBasic::af6e7f3fa8a5a8849976bb1f4f83fbb9, Library\base\utils\console.vb"

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


    ' Code Statistics:

    '   Total Lines: 126
    '    Code Lines: 82
    ' Comment Lines: 25
    '   Blank Lines: 19
    '     File Size: 4.39 KB


    ' Module console
    ' 
    '     Function: ConsoleBackColor, ConsoleForeColor, CreateProgressBar, log, passwordInput
    '               PinProgressBarTop
    ' 
    '     Sub: ClearProgressBarPinned
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports CMD = System.Console
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# console utilities
''' </summary>
''' <remarks>
''' all of the api functions in this package module is not affected by 
''' the <see cref="GlobalEnvironment.stdout"/> I/O redirect.
''' </remarks>
<Package("console", Category:=APICategories.SoftwareTools)>
Module console

    ' 这个模块之中的终端输出函数是不会受到输出环境的影响的

    ''' <summary>
    ''' Writes the specified string value to the standard output stream.
    ''' </summary>
    ''' <param name="message">The message text value to write.</param>
    ''' <param name="fore_color">sets the foreground color of the console.</param>
    ''' <param name="back_color">sets the background color of the console.</param>
    ''' <returns>The message text value</returns>
    <ExportAPI("log")>
    Public Function log(<RRawVectorArgument> message As Object,
                        Optional fore_color As ConsoleColor? = Nothing,
                        Optional back_color As ConsoleColor? = Nothing,
                        Optional env As Environment = Nothing) As Object

        Dim foreBackup = CMD.ForegroundColor
        Dim backBackup = CMD.BackgroundColor

        If Not fore_color Is Nothing Then
            CMD.ForegroundColor = fore_color
        End If
        If Not back_color Is Nothing Then
            CMD.BackgroundColor = back_color
        End If

        Dim strings As String() = CLRVector.asCharacter(message)

        If strings.Length = 0 Then
            Call CMD.Write("")
        ElseIf strings.Length = 1 Then
            Call CMD.Write(sprintf(strings(Scan0)))
        Else
            For i As Integer = 0 To strings.Length - 1
                Call CMD.Write($"[{i + 1}] " & sprintf(strings(i)))
            Next
        End If

        CMD.ForegroundColor = foreBackup
        CMD.BackgroundColor = backBackup

        Return message
    End Function

    ''' <summary>
    ''' Input password on console. only works on windows
    ''' </summary>
    ''' <returns></returns>
    ''' 
    <ExportAPI("password")>
    Public Function passwordInput(Optional maxLen As Integer = 64) As String
        Dim passd As New ConsolePasswordInput
        Dim password As String = Nothing
        Call passd.PasswordInput(password, maxLen)
        Return password
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
