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


    ' Code Statistics:

    '   Total Lines: 83
    '    Code Lines: 47 (56.63%)
    ' Comment Lines: 21 (25.30%)
    '    - Xml Docs: 42.86%
    ' 
    '   Blank Lines: 15 (18.07%)
    '     File Size: 3.07 KB


    '     Class DebuggerContext
    ' 
    '         Properties: Breakpoints, CurrentAction, CurrentEnvironment, IsDebugging
    ' 
    '         Function: ShouldPause
    ' 
    '         Sub: Pause, PrintVariables, WaitForInput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    ''' <summary>
    ''' 调试器状态管理类
    ''' </summary>
    Public Class DebuggerContext
        ' 断点集合（存储行号，也可以存储文件名+行号）
        Public Property Breakpoints As New HashSet(Of Integer)

        ' 当前调试动作
        Public Property CurrentAction As DebugAction = DebugAction.StepOver

        ' 是否处于调试模式
        Public Property IsDebugging As Boolean = False

        ' 用于外部（如UI界面）访问当前变量环境
        Public Property CurrentEnvironment As Environment

        ' 触发断点时的回调事件（可以用来弹窗或在控制台交互）
        Public Event OnBreakpointHit As Action(Of Expression, Environment)

        ''' <summary>
        ''' 检查是否应该暂停执行
        ''' </summary>
        Public Function ShouldPause(expr As Expression) As Boolean
            ' 1. 如果是单步模式，总是暂停
            If CurrentAction = DebugAction.StepOver OrElse CurrentAction = DebugAction.StepInto Then
                Return True
            End If

            ' 2. 如果是继续模式，检查是否命中断点
            If CurrentAction = DebugAction.Continue Then
                Return Breakpoints.Contains(expr.GetBreakPointHashCode)
            End If

            Return False
        End Function

        ''' <summary>
        ''' 进入暂停状态，等待用户指令
        ''' </summary>
        Public Sub Pause(expr As Expression, env As Environment)
            CurrentEnvironment = env
            ' 触发事件，通知外部界面更新变量监视
            RaiseEvent OnBreakpointHit(expr, env)
        End Sub

        ' 在 DebuggerContext 类中添加
        Public Sub WaitForInput()
            Console.WriteLine(">>> Debug Mode: [C]ontinue, [S]tep, [V]ariables, [Q]uit")

            While True
                Dim key As ConsoleKeyInfo = Console.ReadKey(True)

                If key.Key = ConsoleKey.C Then
                    Me.CurrentAction = DebugAction.Continue
                    Exit While
                ElseIf key.Key = ConsoleKey.S Then
                    Me.CurrentAction = DebugAction.StepOver
                    Exit While
                ElseIf key.Key = ConsoleKey.Q Then
                    Me.CurrentAction = DebugAction.Stop
                    Exit While
                ElseIf key.Key = ConsoleKey.V Then
                    ' 打印变量
                    PrintVariables()
                End If
            End While
        End Sub

        Private Sub PrintVariables()
            If CurrentEnvironment IsNot Nothing Then
                ' 假设 Environment 有一个存储变量的字典
                ' For Each kvp In CurrentEnvironment.Variables ...
                Console.WriteLine("... listing variables ...")
            End If
        End Sub

    End Class
End Namespace
