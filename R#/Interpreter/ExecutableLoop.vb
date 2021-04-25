#Region "Microsoft.VisualBasic::9e896cbb5a2d32434bc35f0179468147, R#\Interpreter\ExecutableLoop.vb"

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

    '     Class ExecutableLoop
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Execute, ExecuteCodeLine
    ' 
    '         Sub: configException, printDebug, printMemoryProfile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter

    ''' <summary>
    ''' 使用for循环执行脚本语句
    ''' </summary>
    Public NotInheritable Class ExecutableLoop

        Shared ReadOnly Rsharp As Process = Process.GetCurrentProcess()

        Shared memSize As Double
        Shared memSize2 As Double
        Shared memoryDelta As Double

        Shared Sub New()
            memSize = Rsharp.WorkingSet64 / 1024 / 1024
        End Sub

        Private Sub New()
        End Sub

        ''' <summary>
        ''' function/forloop/if/else/elseif/repeat/while, etc...
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Shared Function Execute(execQueue As IEnumerable(Of Expression), env As Environment) As Object
            Dim last As Object = Nothing
            Dim breakLoop As Boolean = False
            Dim debug As Boolean = env.globalEnvironment.debugMode

            ' The program code loop
            For Each expression As Expression In execQueue
                last = ExecuteCodeLine(expression, env, breakLoop, debug)

                If debug Then
                    Call printMemoryProfile()
                End If

                If breakLoop Then
                    Call configException(env, last, expression)
                    Exit For
                End If
            Next

            Return last
        End Function

        Private Shared Sub printMemoryProfile()
            SyncLock Rsharp
                memSize2 = Rsharp.WorkingSet64 / 1024 / 1024
                memoryDelta = memSize2 - memSize
                memSize = memSize2

                Call Rsharp.Refresh()
            End SyncLock

            If memoryDelta > 0 Then
                Call printDebug($"[app_memory] {memSize2.ToString("F2")} MB, delta {memoryDelta.ToString("F2")} MB", ConsoleColor.Red)
            Else
                Call printDebug($"[app_memory] {memSize2.ToString("F2")} MB, delta {memoryDelta.ToString("F2")} MB", ConsoleColor.Blue)
            End If
        End Sub

        Private Shared Sub configException(env As Environment, last As Object, expression As Expression)
            If Not last Is Nothing AndAlso Program.isException(last) Then
                Dim err As Message = last

                If err.source Is Nothing Then
                    err.source = expression
                End If

                env.globalEnvironment.lastException = err
            End If
        End Sub

        Friend Shared Sub printDebug(expression As String, Optional color As ConsoleColor = ConsoleColor.Magenta)
            Dim fore As ConsoleColor = Console.ForegroundColor

            Console.ForegroundColor = color
            Console.WriteLine(expression)
            Console.ForegroundColor = fore
        End Sub

        ''' <summary>
        ''' For execute lambda function
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <param name="envir"></param>
        ''' <param name="breakLoop"></param>
        ''' <returns></returns>
        Public Shared Function ExecuteCodeLine(expression As Expression, envir As Environment,
                                               Optional ByRef breakLoop As Boolean = False,
                                               Optional debug As Boolean = False) As Object
            Dim last As Object
            Dim benchmark As Long = App.NanoTime

            If debug Then
                Call printDebug(expression.ToString)
            End If

            last = expression.Evaluate(envir)
            benchmark = App.NanoTime - benchmark

            If debug Then
                Call printDebug($"[elapse_time] {TimeSpan.FromTicks(benchmark).FormatTime}", ConsoleColor.Green)
            End If

            ' next keyword will break current closure 
            ' and then goto execute next iteration loop
            If TypeOf expression Is ReturnValue Then
                ' return keyword will break the function
                ' current program maybe is a for loop, if closure, etc
                ' so we needs wrap the last value with 
                ' return keyword.
                last = New ReturnValue(New RuntimeValueLiteral(last))
                breakLoop = True
            ElseIf Not last Is Nothing AndAlso last.GetType Is GetType(ReturnValue) Then
                ' the internal closure invoke a returns keyword
                ' so break the current loop
                '
                ' This situation maybe a deep nested closure, example like 
                '
                ' let fun as function() {
                '    for(x in xxx) {
                '       for(y in yyy) {
                '           if (true(x, y)) {
                '              return ooo;
                '           }
                '       }
                '    }
                ' }
                '
                ' Do not break the returns keyword popout chain 
                '
                breakLoop = True
            End If

            If TypeOf expression Is ContinuteFor Then
                breakLoop = True
            ElseIf TypeOf expression Is BreakLoop Then
                breakLoop = True
            End If

            If Not last Is Nothing Then
                If last.GetType Is GetType(Message) Then
                    If DirectCast(last, Message).level = MSG_TYPES.ERR Then
                        ' throw error will break the expression loop
                        breakLoop = True
                        ' populate out this error message to the top stack
                        ' and then print errors
                        Return last
                    ElseIf DirectCast(last, Message).level = MSG_TYPES.DEBUG Then
                    ElseIf DirectCast(last, Message).level = MSG_TYPES.WRN Then
                    Else

                    End If
                ElseIf last.GetType Is GetType(IfPromise) Then
                    envir.ifPromise.Add(last)
                    last = DirectCast(last, IfPromise).Value

                    If envir.ifPromise.Last.Result Then
                        If Not last Is Nothing AndAlso last.GetType Is GetType(ReturnValue) Then
                            breakLoop = True
                        End If
                    End If
                End If
            End If

            Return last
        End Function
    End Class
End Namespace
