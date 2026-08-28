#Region "Microsoft.VisualBasic::00000000000000000000000000000000, R#\Interpreter\Debugger\ConsoleDebuggerFrontend.vb"

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


'     Class ConsoleDebuggerFrontend
' 
'         Function: parseLocation
' 
'         Sub: Attach, Detach, printBreakpoints, printCallStack, printHeader
'              printHelp, printSource, printVariables, processBreakCommand, processCommand
'              processEvaluate
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter

    ''' <summary>
    ''' 调试器的控制台交互前端
    ''' </summary>
    ''' <remarks>
    ''' 这个类型是 <see cref="DebuggerContext"/> 之上的一个默认的交互实现, 
    ''' 其通过订阅 <see cref="DebuggerContext.OnBreakpointHit"/> 事件的方式
    ''' 来与调试器的内核进行交互. 因为调试器的内核之中并不包含任何的
    ''' 控制台相关的代码, 所以GUI的宿主程序也可以采用完全相同的方式来
    ''' 实现自己的调试界面
    ''' </remarks>
    Public Class ConsoleDebuggerFrontend

        ReadOnly debugger As DebuggerContext

        ''' <summary>
        ''' 用于展示当前所执行到的源代码行的源文件内容缓存
        ''' </summary>
        ReadOnly sourceCache As New Dictionary(Of String, String())

        Dim attached As Boolean = False

        Sub New(debugger As DebuggerContext)
            Me.debugger = debugger
        End Sub

        ''' <summary>
        ''' 将当前的这个控制台前端挂载至调试器之上
        ''' </summary>
        Public Sub Attach()
            If Not attached Then
                AddHandler debugger.OnBreakpointHit, AddressOf OnPause
                attached = True
            End If
        End Sub

        ''' <summary>
        ''' 将当前的这个控制台前端从调试器之上卸载掉
        ''' </summary>
        Public Sub Detach()
            If attached Then
                RemoveHandler debugger.OnBreakpointHit, AddressOf OnPause
                attached = False
            End If
        End Sub

        ''' <summary>
        ''' 断点命中的事件处理程序
        ''' </summary>
        ''' <remarks>
        ''' 这个函数是在被暂停的执行线程之上被同步调用的, 
        ''' 在这个函数返回了之后, 执行线程会继续阻塞在
        ''' <see cref="DebuggerContext.Pause"/> 之中, 
        ''' 直到用户所输入的命令调用了 <see cref="DebuggerContext.Resume"/> 为止
        ''' </remarks>
        Private Sub OnPause(frame As DebugFrame)
            Call printHeader(frame)
            Call printSource(frame)

            ' 命令循环: 一直读取用户所输入的命令, 直到用户输入了
            ' 某一个会使得程序继续执行下去的命令为止
            Do While True
                Call VBDebugger.Write($"(rdb) ", ConsoleColor.Cyan)

                Dim commandLine As String = Console.ReadLine

                If commandLine Is Nothing Then
                    ' 标准输入流已经被关闭掉了, 在这里直接继续执行下去, 
                    ' 避免陷入死循环
                    Call debugger.Resume(DebugAction.Continue)
                    Return
                End If

                If processCommand(commandLine.Trim, frame) Then
                    ' 用户所输入的是一个会使得程序继续执行下去的命令
                    Return
                End If
            Loop
        End Sub

        ''' <summary>
        ''' 处理用户所输入的一条调试命令
        ''' </summary>
        ''' <returns>
        ''' 返回True表示程序应该继续执行下去(即需要退出命令循环), 
        ''' 返回False则表示应该继续等待用户输入下一条命令
        ''' </returns>
        Private Function processCommand(commandLine As String, frame As DebugFrame) As Boolean
            If commandLine.StringEmpty(, True) Then
                ' 直接按下回车键等同于执行单步跳过
                Call debugger.Resume(DebugAction.StepOver)
                Return True
            End If

            Dim tokens As String() = commandLine.Split(" "c)
            Dim command As String = tokens(0).ToLower
            Dim arguments As String = commandLine.Substring(tokens(0).Length).Trim

            Select Case command
                Case "c", "continue"
                    Call debugger.Resume(DebugAction.Continue)
                    Return True
                Case "n", "next"
                    Call debugger.Resume(DebugAction.StepOver)
                    Return True
                Case "s", "step"
                    Call debugger.Resume(DebugAction.StepInto)
                    Return True
                Case "o", "out", "finish"
                    Call debugger.Resume(DebugAction.StepOut)
                    Return True
                Case "q", "quit", "stop"
                    Call debugger.Resume(DebugAction.Stop)
                    Return True

                Case "vars", "v", "ls"
                    Call printVariables(frame, inherits:=arguments.TextEquals("all"))
                Case "bt", "where", "backtrace"
                    Call printCallStack(frame)
                Case "p", "print", "eval"
                    Call processEvaluate(arguments, frame)
                Case "break", "b"
                    Call processBreakCommand(arguments, frame)
                Case "delete", "d"
                    Call processDeleteCommand(arguments, frame)
                Case "info", "breakpoints"
                    Call printBreakpoints()
                Case "l", "list"
                    Call printSource(frame)
                Case "h", "help", "?"
                    Call printHelp()

                Case Else
                    Call VBDebugger.WriteLine($"unknown debugger command: '{command}', type 'help' for more details.", ConsoleColor.Red)
            End Select

            Return False
        End Function

        Private Sub printHeader(frame As DebugFrame)
            Call Console.WriteLine()

            If frame.breakpoint Is Nothing Then
                Call VBDebugger.WriteLine($"[break] {frame.GetSourceLocation()}", ConsoleColor.Yellow)
            Else
                Call VBDebugger.WriteLine($"[breakpoint#{frame.breakpoint.hitCount}] {frame.GetSourceLocation()}", ConsoleColor.Yellow)
            End If

            Call VBDebugger.WriteLine($"  {frame.expression}", ConsoleColor.White)
        End Sub

        ''' <summary>
        ''' 展示当前所执行到的位置附近的源代码内容
        ''' </summary>
        Private Sub printSource(frame As DebugFrame, Optional context As Integer = 2)
            If frame.file.StringEmpty(, True) OrElse frame.line < 0 Then
                Return
            End If

            Dim lines As String() = Nothing

            If Not sourceCache.TryGetValue(frame.file, lines) Then
                Try
                    lines = If(frame.file.FileExists, frame.file.ReadAllLines, {})
                Catch ex As Exception
                    lines = {}
                End Try

                sourceCache(frame.file) = lines
            End If

            If lines.IsNullOrEmpty Then
                Return
            End If

            ' 源代码的行号是从1开始计数的
            Dim start As Integer = Math.Max(1, frame.line - context)
            Dim ends As Integer = Math.Min(lines.Length, frame.line + context)

            For i As Integer = start To ends
                Dim isCurrent As Boolean = (i = frame.line)
                Dim indicator As String = If(isCurrent, "=>", "  ")
                Dim text As String = $" {indicator} {i.ToString.PadLeft(5)} | {lines(i - 1)}"

                Call VBDebugger.WriteLine(text, If(isCurrent, ConsoleColor.Yellow, ConsoleColor.DarkGray))
            Next
        End Sub

        Private Sub printVariables(frame As DebugFrame, [inherits] As Boolean)
            Dim vars As DebugInspector.VariableInfo() = debugger.inspector.GetVariables(frame.environment, [inherits])

            If vars.IsNullOrEmpty Then
                Call VBDebugger.WriteLine("no variable symbols in current environment.", ConsoleColor.DarkGray)
                Return
            End If

            Dim width As Integer = Aggregate v As DebugInspector.VariableInfo
                                   In vars
                                   Into Max(v.name.Length)

            For Each v As DebugInspector.VariableInfo In vars
                Call VBDebugger.Write($"  {v.name.PadRight(width)}", ConsoleColor.Green)
                Call VBDebugger.Write($"  <{v.type}>", ConsoleColor.DarkGray)
                Call VBDebugger.WriteLine($"  {v.value}", ConsoleColor.White)
            Next
        End Sub

        Private Sub printCallStack(frame As DebugFrame)
            Dim stacks As String() = debugger.inspector.GetCallStack(frame.environment)

            If stacks.IsNullOrEmpty Then
                Call VBDebugger.WriteLine("call stack is not available.", ConsoleColor.DarkGray)
            Else
                For Each line As String In stacks
                    Call VBDebugger.WriteLine($"  {line}", ConsoleColor.White)
                Next
            End If
        End Sub

        ''' <summary>
        ''' 在当前的暂停点之上对用户所输入的表达式进行求值
        ''' </summary>
        Private Sub processEvaluate(expression As String, frame As DebugFrame)
            If expression.StringEmpty(, True) Then
                Call VBDebugger.WriteLine("usage: p <expression>", ConsoleColor.Red)
                Return
            End If

            Dim result As Object = debugger.inspector.Evaluate(expression, frame.environment)

            If TypeOf result Is Message Then
                Call VBDebugger.WriteLine(DirectCast(result, Message).ToString, ConsoleColor.Red)
            Else
                Call VBDebugger.WriteLine($"  {DebugInspector.FormatValue(result, maxLength:=1024)}", ConsoleColor.White)
            End If
        End Sub

        ''' <summary>
        ''' 处理断点的设置命令
        ''' </summary>
        ''' <remarks>
        ''' 命令的语法为``break [文件名:]行号 [if 条件表达式]``, 
        ''' 当文件名被省略掉了的时候则默认使用当前所暂停的位置所处的文件
        ''' </remarks>
        Private Sub processBreakCommand(arguments As String, frame As DebugFrame)
            If arguments.StringEmpty(, True) Then
                Call printBreakpoints()
                Return
            End If

            Dim condition As String = Nothing
            Dim location As String = arguments
            Dim ifIndex As Integer = arguments.IndexOf(" if ", StringComparison.OrdinalIgnoreCase)

            If ifIndex > -1 Then
                condition = arguments.Substring(ifIndex + 4).Trim
                location = arguments.Substring(0, ifIndex).Trim
            End If

            Dim file As String = Nothing
            Dim line As Integer = -1

            If Not parseLocation(location, frame, file, line) Then
                Call VBDebugger.WriteLine($"invalid breakpoint location: '{location}'", ConsoleColor.Red)
                Call VBDebugger.WriteLine("usage: break [<file>:]<line> [if <condition>]", ConsoleColor.DarkGray)
                Return
            End If

            Dim bp As Breakpoint = debugger.AddBreakpoint(file, line, condition)

            Call VBDebugger.WriteLine($"breakpoint has been created at {bp}", ConsoleColor.Green)
        End Sub

        ''' <summary>
        ''' 处理断点的删除命令
        ''' </summary>
        Private Sub processDeleteCommand(arguments As String, frame As DebugFrame)
            If arguments.StringEmpty(, True) Then
                Call debugger.ClearBreakpoints()
                Call VBDebugger.WriteLine("all of the breakpoints has been removed.", ConsoleColor.Green)
                Return
            End If

            Dim file As String = Nothing
            Dim line As Integer = -1

            If Not parseLocation(arguments.Trim, frame, file, line) Then
                Call VBDebugger.WriteLine($"invalid breakpoint location: '{arguments}'", ConsoleColor.Red)
            ElseIf debugger.RemoveBreakpoint(file, line) Then
                Call VBDebugger.WriteLine($"breakpoint at {file}:{line} has been removed.", ConsoleColor.Green)
            Else
                Call VBDebugger.WriteLine($"no breakpoint was found at {file}:{line}.", ConsoleColor.DarkGray)
            End If
        End Sub

        ''' <summary>
        ''' 解析``[文件名:]行号``形式的源代码位置描述字符串
        ''' </summary>
        Private Shared Function parseLocation(location As String,
                                              frame As DebugFrame,
                                              ByRef file As String,
                                              ByRef line As Integer) As Boolean

            If location.StringEmpty(, True) Then
                Return False
            End If

            ' 因为在Windows平台之上文件的路径之中会包含有类似于``C:``这样的
            ' 盘符分隔符, 所以在这里需要从字符串的末尾开始查找分隔符
            Dim index As Integer = location.LastIndexOf(":"c)

            If index = -1 Then
                ' 只给出了行号, 使用当前所暂停的位置所处的文件
                file = frame.file
            Else
                file = location.Substring(0, index).Trim
                location = location.Substring(index + 1)
            End If

            If file.StringEmpty(, True) Then
                Return False
            Else
                Return Integer.TryParse(location.Trim, line) AndAlso line > 0
            End If
        End Function

        Private Sub printBreakpoints()
            Dim list As Breakpoint() = debugger.ListBreakpoints

            If list.IsNullOrEmpty Then
                Call VBDebugger.WriteLine("no breakpoints has been created yet.", ConsoleColor.DarkGray)
            Else
                For i As Integer = 0 To list.Length - 1
                    Call VBDebugger.WriteLine($"  [{i}] {list(i)}", ConsoleColor.White)
                Next
            End If
        End Sub

        Private Shared Sub printHelp()
            Dim helps As String() = {
                "  c, continue          continute running until next breakpoint",
                "  n, next              step over, run to the next statement in current block",
                "  s, step              step into, run into the function/loop/branch body",
                "  o, out               step out, run until returns to the outer code block",
                "  q, quit              stop running of the script program",
                "",
                "  v, vars [all]        list the variable symbols in current environment",
                "  bt                   print the current call stack",
                "  p <expression>       evaluate a R# expression at current break point",
                "",
                "  b, break [<file>:]<line> [if <condition>]",
                "                       create a new breakpoint at the target location",
                "  d, delete [[<file>:]<line>]",
                "                       delete the target breakpoint, or clear all if no location",
                "  info                 list all of the breakpoints",
                "  l, list              print the source code around current location",
                "  h, help              print this help information"
            }

            For Each line As String In helps
                Call VBDebugger.WriteLine(line, ConsoleColor.DarkGray)
            Next
        End Sub
    End Class
End Namespace
