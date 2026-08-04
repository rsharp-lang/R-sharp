#Region "Microsoft.VisualBasic::cd702853309c115833a18263b4c03cb0, R#\Interpreter\Debugger\DebuggerContext.vb"

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


'     Class DebuggerContext
' 
'         Properties: breakpoints, CurrentAction, CurrentEnvironment, inspector, IsDebugging
'                     stackDepth
' 
'         Function: AddBreakpoint, EnterBlock, evaluateCondition, ListBreakpoints, RemoveBreakpoint
'                   ShouldPause, TryHitBreakpoint
' 
'         Sub: ClearBreakpoints, ExitBlock, Pause, [Resume], SetEnabled
'              Start, [Stop]
' 
' 
' /********************************************************************************/

#End Region

Imports System.Threading
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Interpreter

    ''' <summary>
    ''' 调试器状态管理类
    ''' </summary>
    ''' <remarks>
    ''' 这个类型是整个调试器的核心, 其内部不包含任何的控制台交互代码, 
    ''' 控制台的交互逻辑位于 <see cref="ConsoleDebuggerFrontend"/> 之中, 
    ''' 使得GUI宿主程序能够通过订阅 <see cref="OnBreakpointHit"/> 事件
    ''' 并且调用 <see cref="[Resume]"/> 函数的方式来复用同一套调试器内核.
    ''' 
    ''' 在整个环境链之上, 所有的 <see cref="Environment"/> 对象都是共享
    ''' 同一个 <see cref="DebuggerContext"/> 实例的, 请参见
    ''' <see cref="Environment"/> 的构造函数之中的相关实现
    ''' </remarks>
    Public Class DebuggerContext

        ''' <summary>
        ''' 断点的集合管理器
        ''' </summary>
        Public ReadOnly Property breakpoints As New BreakpointStore

        ''' <summary>
        ''' 运行时的检查器, 提供查看变量/调用堆栈以及表达式求值等功能
        ''' </summary>
        Public ReadOnly Property inspector As New DebugInspector(Me)

        ''' <summary>
        ''' 当前的调试动作
        ''' </summary>
        Public Property CurrentAction As DebugAction = DebugAction.StepInto

        ''' <summary>
        ''' 是否处于调试模式?
        ''' </summary>
        ''' <remarks>
        ''' 当这个属性值为False的时候, 程序的执行不会受到调试器的任何影响, 
        ''' 在 <see cref="ExecutableLoop"/> 的热点循环之中仅存在一次布尔判断的开销
        ''' </remarks>
        Public Property IsDebugging As Boolean = False

        ''' <summary>
        ''' 用于外部(例如UI界面)访问当前的变量环境
        ''' </summary>
        Public Property CurrentEnvironment As Environment

        ''' <summary>
        ''' 当前的代码块嵌套深度
        ''' </summary>
        ''' <remarks>
        ''' 因为所有的函数体/循环体/分支体最终都是通过
        ''' <see cref="ExecutableLoop.Execute"/> 来执行其内部的语句列表的, 
        ''' 所以在该函数的出入口处对这个计数器做自增自减操作之后, 
        ''' 这个计数器的数值就天然的等于当前的代码块的嵌套深度了
        ''' </remarks>
        Public ReadOnly Property stackDepth As Integer
            Get
                Return _depth
            End Get
        End Property

        ''' <summary>
        ''' <see cref="stackDepth"/> 的后备字段
        ''' </summary>
        ''' <remarks>
        ''' 因为需要通过 <see cref="Interlocked"/> 来进行原子操作, 
        ''' 所以在这里必须要声明为一个可以按引用传递的字段
        ''' </remarks>
        Private _depth As Integer

        ''' <summary>
        ''' 在执行单步操作的时候所记录下来的基准深度
        ''' </summary>
        Private baselineDepth As Integer

        ''' <summary>
        ''' 用于阻塞执行线程的信号量
        ''' </summary>
        ''' <remarks>
        ''' 初始状态为无信号, 使得 <see cref="Pause"/> 能够阻塞住执行线程, 
        ''' 直到宿主程序调用 <see cref="[Resume]"/> 将其置位为止
        ''' </remarks>
        ReadOnly resumeSignal As New ManualResetEventSlim(initialState:=False)

        ''' <summary>
        ''' 创建这个调试器上下文对象的线程的编号
        ''' </summary>
        ''' <remarks>
        ''' 因为 <see cref="DebuggerContext"/> 在整个环境链之上是共享的同一个实例, 
        ''' 而 parLapply/parSapply/%dopar% 等并行操作会在多个工作线程之上同时执行
        ''' R#的表达式, 如果不加以限制的话, 多个工作线程会同时在这里等待同一个信号量
        ''' 而造成死锁. 所以在这里记录下创建者线程(即主线程)的编号, 
        ''' 使得只有主线程才会真正的进入暂停状态
        ''' </remarks>
        ReadOnly ownerThreadId As Integer = Thread.CurrentThread.ManagedThreadId

        ''' <summary>
        ''' 触发断点时的回调事件(可以用来弹窗或者在控制台之中做交互)
        ''' </summary>
        ''' <remarks>
        ''' 这个事件是在执行线程之上被同步触发的, 事件的处理程序返回之后, 
        ''' 执行线程会继续阻塞在 <see cref="Pause"/> 之中, 
        ''' 直到有其他的代码调用了 <see cref="[Resume]"/> 为止
        ''' </remarks>
        Public Event OnBreakpointHit As Action(Of DebugFrame)

        ''' <summary>
        ''' 启动调试会话
        ''' </summary>
        ''' <param name="breakOnEntry">
        ''' 是否在脚本的第一条语句处就暂停下来? 
        ''' 默认为True, 便于用户在脚本开始执行之前设置断点
        ''' </param>
        Public Sub Start(Optional breakOnEntry As Boolean = True)
            IsDebugging = True
            CurrentAction = If(breakOnEntry, DebugAction.StepInto, DebugAction.Continue)
            baselineDepth = 0
            Call resumeSignal.Reset()
        End Sub

        ''' <summary>
        ''' 结束调试会话, 让脚本程序继续正常的执行下去
        ''' </summary>
        Public Sub [Stop]()
            IsDebugging = False
            CurrentAction = DebugAction.Continue
            ' 唤醒可能正在等待之中的执行线程, 避免出现死锁
            Call resumeSignal.Set()
        End Sub

        ''' <summary>
        ''' 进入一个新的代码块
        ''' </summary>
        ''' <remarks>
        ''' 这个函数只应该被 <see cref="ExecutableLoop.Execute"/> 所调用
        ''' </remarks>
        Friend Function EnterBlock() As Integer
            ' 并行任务的工作线程同样会经由 ExecutableLoop 来执行代码块, 
            ' 而整个环境链之上共享的是同一个调试器上下文对象, 
            ' 所以在这里必须要使用原子操作来避免深度计数器被多个线程所破坏
            Return Interlocked.Increment(_depth)
        End Function

        ''' <summary>
        ''' 离开当前的代码块
        ''' </summary>
        ''' <remarks>
        ''' 这个函数只应该在 <see cref="ExecutableLoop.Execute"/> 的
        ''' Finally 代码块之中被调用, 以保证在发生了异常或者是通过
        ''' return/break 提前退出的时候深度计数器不会失衡
        ''' </remarks>
        Friend Sub ExitBlock()
            Call Interlocked.Decrement(_depth)
        End Sub

        ''' <summary>
        ''' 当前的调用是否发生在主线程之上?
        ''' </summary>
        Friend ReadOnly Property isOwnerThread As Boolean
            Get
                Return Thread.CurrentThread.ManagedThreadId = ownerThreadId
            End Get
        End Property

        ''' <summary>
        ''' 检查是否应该暂停执行
        ''' </summary>
        ''' <param name="expr">当前即将被执行的表达式</param>
        ''' <param name="env">当前的运行时环境</param>
        ''' <param name="hit">
        ''' 如果本次暂停是由断点所触发的话, 则通过这个参数返回对应的断点对象
        ''' </param>
        Friend Function ShouldPause(expr As Expression, env As Environment, ByRef hit As Breakpoint) As Boolean
            hit = Nothing

            ' 并行执行的工作线程不参与调试, 避免多个线程同时等待同一个信号量
            ' 而造成死锁, 同时也避免了并行线程污染 CurrentEnvironment
            If Not isOwnerThread Then
                Return False
            End If

            Select Case CurrentAction
                Case DebugAction.Stop
                    Return False

                Case DebugAction.StepInto
                    ' 单步进入: 在任意的嵌套深度之上都暂停
                    Return True

                Case DebugAction.StepOver
                    ' 单步跳过: 只在与基准深度相同或者更外层的位置上暂停, 
                    ' 更深的层级属于被调用的函数体, 直接执行完毕即可
                    If _depth <= baselineDepth Then
                        Return True
                    End If

                Case DebugAction.StepOut
                    ' 单步跳出: 一直执行到返回至外层的代码块之后才暂停
                    If _depth < baselineDepth Then
                        Return True
                    End If
            End Select

            ' 即便是处于单步执行的过程之中, 位于更深层级的断点也应该被命中
            hit = TryHitBreakpoint(expr, env)

            Return Not hit Is Nothing
        End Function

        ''' <summary>
        ''' 判断当前的表达式之上是否命中了某一个断点
        ''' </summary>
        Private Function TryHitBreakpoint(expr As Expression, env As Environment) As Breakpoint
            If breakpoints.isEmpty Then
                Return Nothing
            End If

            Dim bp As Breakpoint = breakpoints.TryHit(expr)

            If bp Is Nothing Then
                Return Nothing
            End If

            ' 条件断点: 只有当条件表达式的求值结果为逻辑真值的时候才会中断
            If Not bp.condition.StringEmpty(, True) AndAlso Not evaluateCondition(bp, env) Then
                Return Nothing
            End If

            bp.hitCount += 1

            ' [DIAG] 临时诊断: 写入文件绕过缓冲, 打印实际命中的断点行号
            System.IO.File.AppendAllText("hit_diag.log", $"line={bp.line} file='{bp.file}' condition='{bp.condition}'" & vbCrLf)

            Return bp
        End Function

        ''' <summary>
        ''' 对条件断点的条件表达式进行求值
        ''' </summary>
        ''' <returns>
        ''' 当条件表达式的求值过程之中发生了错误的时候, 
        ''' 这个函数会返回True(即安全失败, 仍然中断执行), 
        ''' 避免因为条件表达式本身存在问题而导致断点被静默的忽略掉
        ''' </returns>
        Private Function evaluateCondition(bp As Breakpoint, env As Environment) As Boolean
            Dim result As Object = inspector.Evaluate(bp.condition, env)

            If TypeOf result Is Message Then
                Return True
            End If

            Try
                Return CLRVector.asLogical(result).Any(Function(flag) flag)
            Catch ex As Exception
                Return True
            End Try
        End Function

        ''' <summary>
        ''' 进入暂停状态, 等待用户的指令
        ''' </summary>
        ''' <remarks>
        ''' 这是一个阻塞调用. 函数会先触发 <see cref="OnBreakpointHit"/> 事件
        ''' 来通知宿主程序更新其变量监视界面, 然后阻塞住当前的执行线程, 
        ''' 直到宿主程序调用了 <see cref="[Resume]"/> 为止
        ''' </remarks>
        Friend Sub Pause(frame As DebugFrame)
            CurrentEnvironment = frame.environment

            Call resumeSignal.Reset()
            RaiseEvent OnBreakpointHit(frame)

            ' 阻塞执行线程, 等待宿主程序下达下一步的调试指令
            Call resumeSignal.Wait()
        End Sub

        ''' <summary>
        ''' 设置下一步的调试动作并且唤醒被阻塞住的执行线程
        ''' </summary>
        ''' <param name="action">下一步所要执行的调试动作</param>
        ''' <remarks>
        ''' 这个函数是由宿主程序或者是控制台前端所在的线程来调用的
        ''' </remarks>
        Public Sub [Resume](action As DebugAction)
            CurrentAction = action
            ' 以当前所处的深度作为后续单步判断的基准
            baselineDepth = _depth

            If action = DebugAction.Stop Then
                IsDebugging = False
            End If

            Call resumeSignal.Set()
        End Sub

        ''' <summary>
        ''' 在临时挂起调试状态的情况下执行一段R#程序
        ''' </summary>
        ''' <remarks>
        ''' 在暂停点上对监视表达式或者是条件断点的条件表达式进行求值的时候, 
        ''' 其内部同样会经由 <see cref="ExecutableLoop.Execute"/> 来执行, 
        ''' 如果不做特殊处理的话就会再一次的触发暂停逻辑, 
        ''' 从而使得执行线程重入 <see cref="Pause"/> 而造成死锁.
        ''' 
        ''' 所以在这里通过临时的将 <see cref="IsDebugging"/> 置为False
        ''' 的方式来保证求值过程本身不会被调试器所拦截
        ''' </remarks>
        Friend Function EvaluateWithoutDebug(program As Program, env As Environment) As Object
            Dim restore As Boolean = IsDebugging

            Try
                IsDebugging = False
                Return program.Execute(env)
            Finally
                IsDebugging = restore
            End Try
        End Function

        ''' <summary>
        ''' 添加一个断点
        ''' </summary>
        ''' <param name="file">脚本文件的路径</param>
        ''' <param name="line">源代码的行号</param>
        ''' <param name="condition">可选的条件表达式R#源代码</param>
        Public Function AddBreakpoint(file As String, line As Integer, Optional condition As String = Nothing) As Breakpoint
            Return breakpoints.Add(file, line, condition)
        End Function

        ''' <summary>
        ''' 移除掉指定位置上的断点
        ''' </summary>
        Public Function RemoveBreakpoint(file As String, line As Integer) As Boolean
            Return breakpoints.Remove(file, line)
        End Function

        ''' <summary>
        ''' 设置指定位置上的断点的启用状态
        ''' </summary>
        Public Sub SetEnabled(file As String, line As Integer, enabled As Boolean)
            Call breakpoints.SetEnabled(file, line, enabled)
        End Sub

        ''' <summary>
        ''' 清除掉所有的断点
        ''' </summary>
        Public Sub ClearBreakpoints()
            Call breakpoints.Clear()
        End Sub

        ''' <summary>
        ''' 列出当前所注册的全部的断点
        ''' </summary>
        Public Function ListBreakpoints() As Breakpoint()
            Return breakpoints.ListAll
        End Function
    End Class
End Namespace
