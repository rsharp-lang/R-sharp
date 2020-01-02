#Region "Microsoft.VisualBasic::373b70eee79ebb02534323aad03bc1f3, R#\Runtime\Internal\debug.vb"

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

    '     Class debug
    ' 
    '         Properties: verbose
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [stop], getMessageColor, getMessagePrefix, PrintMessageInternal
    ' 
    '         Sub: write
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Runtime.Internal

    Public NotInheritable Class debug

        ''' <summary>
        ''' 啰嗦模式下会输出一些调试信息
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property verbose As Boolean = False

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 只在<see cref="verbose"/>啰嗦模式下才会工作
        ''' </summary>
        ''' <param name="message$"></param>
        ''' <param name="color"></param>
        Public Shared Sub write(message$, Optional color As ConsoleColor = ConsoleColor.White)
            If verbose Then
                Call VBDebugger.WaitOutput()
                Call Log4VB.Print(message & ASCII.LF, color)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [stop](message As Object, envir As Environment) As Message
            Return base.stop(message, envir)
        End Function

        Public Shared Function PrintMessageInternal(message As Message) As Object
            Dim execRoutine$ = message.environmentStack _
                .Reverse _
                .Select(Function(frame) frame.Method.Method) _
                .JoinBy(" -> ")
            Dim i As i32 = 1
            Dim backup = Console.ForegroundColor

            Console.ForegroundColor = message.DoCall(AddressOf getMessageColor)
            Console.WriteLine($" {message.DoCall(AddressOf getMessagePrefix)} in {execRoutine}")

            For Each msg As String In message
                Console.WriteLine($"  {++i}. {msg}")
            Next

            If Not message.source Is Nothing Then
                Call Console.WriteLine()
                Call Console.WriteLine($" R# source: {message.source.ToString}")
            End If

            Console.ForegroundColor = backup

            Return Nothing
        End Function

        Private Shared Function getMessagePrefix(message As Message) As String
            Select Case message.level
                Case MSG_TYPES.ERR : Return "Error"
                Case MSG_TYPES.INF : Return "Information"
                Case MSG_TYPES.WRN : Return "Warning"
                Case MSG_TYPES.DEBUG : Return "Debug output"
                Case Else
                    Return "Message"
            End Select
        End Function

        Private Shared Function getMessageColor(message As Message) As ConsoleColor
            Select Case message.level
                Case MSG_TYPES.ERR : Return ConsoleColor.Red
                Case MSG_TYPES.INF : Return ConsoleColor.Blue
                Case MSG_TYPES.WRN : Return ConsoleColor.Yellow
                Case MSG_TYPES.DEBUG : Return ConsoleColor.Green
                Case Else
                    Return ConsoleColor.White
            End Select
        End Function
    End Class
End Namespace
