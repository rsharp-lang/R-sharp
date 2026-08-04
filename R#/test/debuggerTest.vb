#Region "Microsoft.VisualBasic::7c23c129b76bb716d5f17c9f157d1334, R#\Test\debuggerTest.vb"

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
    ' along with this program if not, see <http://www.gnu.org/licenses/>.




    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 13
    '    Code Lines: 9 (69.23%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (30.77%)
    '     File Size: 285 B


    ' Module debuggerTest
    ' 
    '     Sub: Main, RunScriptBreakpointTest, RunStepTest, RunConditionalBreakpointTest,
    '          DumpBreakpoints, Assert
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.Interpreter

Module debuggerTest

    Dim R As New RInterpreter With {.debug = True}

    ''' <summary>
    ''' 用于驱动断点测试的内嵌脚本: 
    ''' 每一行都会被调试器当作一个单独的顶层语句来执行
    ''' </summary>
    Const script As String =
"let a = 1;" & vbLf &
"let b = 2;" & vbLf &
"let f = function(x) {" & vbLf &
"    let y = x + 1;" & vbLf &
"    return(y * 2)" & vbLf &
"};" & vbLf &
"let c = f(a);" & vbLf &
"let d = a + b + c;" & vbLf &
"print(d);"

    Sub Main()
        Call RunScriptBreakpointTest()
        Call RunStepTest()
        Call RunConditionalBreakpointTest()
    End Sub

    ''' <summary>
    ''' 在临时文件中落盘脚本, 之后以 ``source`` 的方式加载执行,
    ''' 这样 stackFrame 才会带有真实的文件路径与行号, 
    ''' 从而可以触发按照 ``文件 + 行号`` 注册的断点
    ''' </summary>
    Private Function WriteScript() As String
        Dim path As String = System.IO.Path.GetTempFileName().Replace(".tmp", ".R")
        Call File.WriteAllText(path, script)
        Return path
    End Function

    ''' <summary>
    ''' 验证按照文件 + 行号注册的断点会被命中, 并且命中之后
    ''' 可以通过 ``Resume(Continue)`` 继续运行到下一个断点
    ''' </summary>
    Private Sub RunScriptBreakpointTest()
        Dim file As String = WriteScript()
        Dim dbg As DebuggerContext = R.globalEnvir.debugger

        ' 在第三行(function 定义)与第八行(c <- f(a))处设置断点
        Call dbg.AddBreakpoint(file, 3)
        Call dbg.AddBreakpoint(file, 8)

        Call dbg.Start(breakOnEntry:=False)

        Dim hits As New List(Of Integer)

        AddHandler dbg.OnBreakpointHit, Sub(frame As DebugFrame)
                                            Call Console.WriteLine($"[DIAG] hit at {frame.file}:{frame.line}")
                                            Call hits.Add(frame.line)
                                            Call dbg.Resume(DebugAction.Continue)
                                        End Sub

        Call R.Source(file)

        Call Assert(hits.Count = 2, "should hit both breakpoints")

        If hits.Any Then
            Call Assert(hits(0) = 3, "first breakpoint should be at line 3")
            Call Assert(hits(1) = 8, "second breakpoint should be at line 8")
        End If

        Call DumpBreakpoints("script breakpoint test")

        Call dbg.Stop()
        Call dbg.ClearBreakpoints()
    End Sub

    ''' <summary>
    ''' 验证四种执行控制: Continue / StepOver / StepInto / StepOut
    ''' 以及作用域变量查看, 调用栈打印与运行时表达式求值
    ''' </summary>
    Private Sub RunStepTest()
        Dim file As String = WriteScript()
        Dim dbg As DebuggerContext = R.globalEnvir.debugger
        Dim inspector As New DebugInspector(dbg)

        ' 仅在第一行设断点, 之后通过单步指令来推进
        Call dbg.AddBreakpoint(file, 1)
        Call dbg.Start(breakOnEntry:=False)

        Dim stepCount As Integer = 0
        Dim maxSteps As Integer = 12

        AddHandler dbg.OnBreakpointHit, Sub(frame As DebugFrame)
                                            stepCount += 1

                                            If stepCount = 1 Then
                                                ' 第一次命中: 验证断点变量与表达式求值
                                                Dim vars = inspector.GetVariables(frame.environment, inherits:=False)
                                                Call Assert(vars.Any(Function(v) v.name = "a"), "variable 'a' should be visible at breakpoint")

                                                Dim eval = inspector.Evaluate("a + b", frame.environment)
                                                Call Assert(eval IsNot Nothing, "should be able to evaluate 'a + b' at breakpoint")

                                                ' 验证调用栈至少包含当前帧
                                                Dim stack = inspector.GetCallStack(frame.environment)
                                                Call Assert(stack.Length > 0, "call stack should not be empty")

                                                ' 步过第一行
                                                Call dbg.Resume(DebugAction.StepOver)
                                            ElseIf stepCount = 2 Then
                                                ' 第二次(步过之后): 此时应该停在 b <- 2
                                                Call Assert(frame.line = 2, $"step over should land on line 2, but got {frame.line}")
                                                ' 步入函数调用
                                                Call dbg.Resume(DebugAction.StepInto)
                                            ElseIf frame.breakpoint Is Nothing AndAlso stepCount >= 3 Then
                                                ' 步进行走期间: 当进入函数体内部时执行 StepOut 返回
                                                If frame.file = file AndAlso frame.line >= 4 AndAlso frame.line <= 6 Then
                                                    Call dbg.Resume(DebugAction.StepOut)
                                                Else
                                                    Call dbg.Resume(DebugAction.StepOver)
                                                End If
                                            End If

                                            If stepCount >= maxSteps Then
                                                Call dbg.Resume(DebugAction.Stop)
                                            End If
                                        End Sub

        Call R.Source(file)

        Call Assert(stepCount > 2, $"stepping should have advanced multiple times (got {stepCount})")

        Call DumpBreakpoints("step test")

        Call dbg.Stop()
        Call dbg.ClearBreakpoints()
    End Sub

    ''' <summary>
    ''' 验证条件断点: 只有当条件表达式为逻辑真值时才会中断
    ''' </summary>
    Private Sub RunConditionalBreakpointTest()
        Dim file As String = WriteScript()
        Dim dbg As DebuggerContext = R.globalEnvir.debugger

        ' 在第八行设条件断点: 只有当 a 大于 0 的时候才中断
        ' 由于在脚本里面 a 始终为 1, 所以这个条件断点会被命中
        Call dbg.AddBreakpoint(file, 8, condition:="a > 0")
        Call dbg.Start(breakOnEntry:=False)

        Dim conditionHit As Boolean = False

        AddHandler dbg.OnBreakpointHit, Sub(frame As DebugFrame)
                                            If frame.line = 8 Then
                                                conditionHit = True
                                            End If
                                            Call dbg.Resume(DebugAction.Continue)
                                        End Sub

        Call R.Source(file)

        Call Assert(conditionHit, "conditional breakpoint should be hit when a > 0")

        Call DumpBreakpoints("conditional breakpoint test")

        Call dbg.Stop()
        Call dbg.ClearBreakpoints()
    End Sub

    Private Sub DumpBreakpoints(title As String)
        Dim dbg As DebuggerContext = R.globalEnvir.debugger
        Dim all = dbg.ListBreakpoints()

        Call VBDebugger.WriteLine($"[{title}] breakpoints registered: {all.Length}", ConsoleColor.Gray)
        For Each bp As Breakpoint In all
            Call VBDebugger.WriteLine($"    {bp.ToString()}", ConsoleColor.Gray)
        Next
    End Sub

    Private Sub Assert(condition As Boolean, message As String)
        If condition Then
            Call VBDebugger.WriteLine($"[PASS] {message}", ConsoleColor.Green)
        Else
            Call VBDebugger.WriteLine($"[FAIL] {message}", ConsoleColor.Red)
        End If
    End Sub
End Module
