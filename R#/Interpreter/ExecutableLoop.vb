#Region "Microsoft.VisualBasic::bc7514eced2ddcf7b2f897515a759395, D:/GCModeller/src/R-sharp/R#//Interpreter/ExecutableLoop.vb"

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

    '   Total Lines: 235
    '    Code Lines: 149
    ' Comment Lines: 52
    '   Blank Lines: 34
    '     File Size: 9.33 KB


    '     Enum DebugLevels
    ' 
    '         Memory, None, Stack
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class ExecutableLoop
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Execute, ExecuteCodeLine, isBreakSignal
    ' 
    '         Sub: configException, printMemoryProfile, runRefreshMemory
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports SMRUCC.Rsharp.Development.Components
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

        ' 20220920
        ' shared object is not working well on the parallel
        ' due to the reason of synclock will block the 
        ' parallel threads job
        '
        ' so this execute job module code should be a class
        ' instance, not shared code for run some parallel code
        ' example like parLapply and parSapply.

        ReadOnly Rsharp As Process = Process.GetCurrentProcess()

        Dim memSize As Double
        Dim memSize2 As Double
        Dim memoryDelta As Double
        Dim refreshMemory As Boolean = False

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New()
            memSize = Rsharp.WorkingSet64 / 1024 / 1024
        End Sub

        ''' <summary>
        ''' function/forloop/if/else/elseif/repeat/while, etc...
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Function Execute(execQueue As IEnumerable(Of Expression), env As Environment) As Object
            Dim last As Object = Nothing
            Dim breakLoop As Boolean = False
            Dim debug As Boolean = env.globalEnvironment.debugMode
            Dim showMemory As Boolean = debug AndAlso (env.globalEnvironment.debugLevel = DebugLevels.All OrElse env.globalEnvironment.debugLevel = DebugLevels.Memory)
            Dim showExpression As Boolean = debug AndAlso (env.globalEnvironment.debugLevel = DebugLevels.All OrElse env.globalEnvironment.debugLevel = DebugLevels.Stack)
            Dim benchmark As Long
            Dim timestamp As Long

            ' The program code loop
            For Each expression As Expression In execQueue
                benchmark = App.NanoTime
                timestamp = App.UnixTimeStamp
                refreshMemory = False
                last = ExecuteCodeLine(expression, env, breakLoop, showExpression)
                benchmark = App.NanoTime - benchmark

                If Not env.profiler Is Nothing Then
                    Call runRefreshMemory()
                    Call env.profiler.Add(New ProfileRecord(expression) With {
                        .elapse_time = benchmark,
                        .memory_delta = memoryDelta,
                        .stackframe = New StackFrame(env.stackFrame),
                        .tag = timestamp,
                        .memory_size = memSize2
                    })
                End If

                If showExpression Then
                    Call VBDebugger.WriteLine($"[elapse_time] {TimeSpan.FromTicks(benchmark).FormatTime}", ConsoleColor.Green)
                End If

                If showMemory Then
                    Call runRefreshMemory()
                    Call printMemoryProfile()
                End If

                If breakLoop Then
                    Call configException(env, last, expression)
                    Exit For
                End If
            Next

            Return last
        End Function

        Private Sub runRefreshMemory()
            If Not refreshMemory Then
                SyncLock Rsharp
                    memSize2 = Rsharp.WorkingSet64 / 1024 / 1024
                    memoryDelta = memSize2 - memSize
                    memSize = memSize2

                    Call Rsharp.Refresh()
                End SyncLock

                refreshMemory = True
            End If
        End Sub

        Private Sub printMemoryProfile()
            If memoryDelta > 0 Then
                Call VBDebugger.WriteLine($"[app_memory] {memSize2.ToString("F2")} MB, delta {memoryDelta.ToString("F2")} MB", ConsoleColor.Red)
            Else
                Call VBDebugger.WriteLine($"[app_memory] {memSize2.ToString("F2")} MB, delta {memoryDelta.ToString("F2")} MB", ConsoleColor.Blue)
            End If
        End Sub

        Private Sub configException(env As Environment, last As Object, expression As Expression)
            If Not last Is Nothing AndAlso Program.isException(last) Then
                Dim err As Message = last

                If err.source Is Nothing Then
                    err.source = expression
                End If

                env.globalEnvironment.lastException = err
            End If
        End Sub

        ''' <summary>
        ''' For execute lambda function
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <param name="envir"></param>
        ''' <param name="breakLoop"></param>
        ''' <returns></returns>
        Public Function ExecuteCodeLine(expression As Expression, envir As Environment,
                                        Optional ByRef breakLoop As Boolean = False,
                                        Optional showExpression As Boolean = False) As Object
            Dim last As Object

            If showExpression Then
                Call VBDebugger.WriteLine(expression.ToString, ConsoleColor.White)
            End If

            last = expression.Evaluate(envir)

            ' next keyword will break current closure 
            ' and then goto execute next iteration loop
            If TypeOf expression Is ReturnValue Then
                ' return keyword will break the function
                ' current program maybe is a for loop, if closure, etc
                ' so we needs wrap the last value with 
                ' return keyword.
                last = New ReturnValue(New RuntimeValueLiteral(last))
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

            breakLoop = breakLoop OrElse isBreakSignal(expression)

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
                    Dim prom As IfPromise = last

                    If prom.elseIf Then
                        Dim lastHit As IfPromise = envir.ifPromise.Last

                        lastHit.Result = prom.Result
                        lastHit.Value = prom.Value
                    ElseIf TypeOf expression Is ElseBranch Then
                        ' envir.ifPromise.Pop(strict:=False)
                        ' do nothing
                    Else
                        envir.ifPromise.Add(last)
                    End If

                    last = DirectCast(last, IfPromise).Value

                    If envir.ifPromise.Last.Result AndAlso isBreakSignal(last) Then
                        breakLoop = True
                    End If
                End If
            End If

            Return last
        End Function

        Private Shared Function isBreakSignal(last As Object) As Boolean
            If last Is Nothing Then
                Return False
            ElseIf TypeOf last Is ReturnValue Then
                Return True
            ElseIf TypeOf last Is BreakLoop Then
                Return True
            ElseIf TypeOf last Is ContinuteFor Then
                Return True
            Else
                Return False
            End If
        End Function
    End Class
End Namespace
