#Region "Microsoft.VisualBasic::00000000000000000000000000000000, R#\Interpreter\Debugger\DebugInspector.vb"

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


    '     Class DebugInspector
    ' 
    '         Function: Evaluate, GetCallStack, GetVariables, PreviewValue
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports RRuntime = SMRUCC.Rsharp.Runtime.Internal

Namespace Interpreter

    ''' <summary>
    ''' 运行时的检查器
    ''' </summary>
    ''' <remarks>
    ''' 为调试器提供查看当前作用域之中的变量/查看调用堆栈以及
    ''' 在暂停点之上对任意的R#表达式进行求值等运行时的检查能力
    ''' </remarks>
    Public Class DebugInspector

        ReadOnly debugger As DebuggerContext

        Sub New(debugger As DebuggerContext)
            Me.debugger = debugger
        End Sub

        ''' <summary>
        ''' 列出目标环境之中所定义的全部的变量符号
        ''' </summary>
        ''' <param name="env">
        ''' 目标运行时环境, 默认是使用调试器当前所处的暂停点上的环境
        ''' </param>
        ''' <returns>[变量名称 =&gt; 变量值的预览文本]</returns>
        Public Function GetVariables(Optional env As Environment = Nothing) As NamedValue(Of String)()
            env = If(env, debugger.CurrentEnvironment)

            If env Is Nothing Then
                Return {}
            End If

            Return env.GetSymbolsNames _
                .Select(Function(name)
                            Return New NamedValue(Of String)(name, PreviewValue(env.GetValue(name)))
                        End Function) _
                .OrderBy(Function(v) v.Name) _
                .ToArray
        End Function

        ''' <summary>
        ''' 获取得到从当前的栈帧一直到最顶层的调用堆栈信息
        ''' </summary>
        ''' <param name="env">
        ''' 目标运行时环境, 默认是使用调试器当前所处的暂停点上的环境
        ''' </param>
        Public Function GetCallStack(Optional env As Environment = Nothing) As StackFrame()
            env = If(env, debugger.CurrentEnvironment)

            If env Is Nothing Then
                Return {}
            Else
                ' 直接复用运行时环境所提供的堆栈追踪能力
                Return env.stackTrace
            End If
        End Function

        ''' <summary>
        ''' 在指定的运行时环境之中对一段R#表达式源代码进行求值
        ''' </summary>
        ''' <param name="expression">所需要进行求值的R#表达式的源代码</param>
        ''' <param name="env">
        ''' 目标运行时环境, 默认是使用调试器当前所处的暂停点上的环境
        ''' </param>
        ''' <returns>
        ''' 表达式的求值结果. 当语法解析或者是求值的过程之中发生了错误的时候, 
        ''' 这个函数会返回一个 <see cref="Message"/> 错误消息对象
        ''' </returns>
        ''' <remarks>
        ''' 这个函数是直接复用了解释器现成的语法解析与求值链路的, 
        ''' 所以在这里可以执行任意合法的R#表达式, 
        ''' 其效果等价于其他的调试器之中的``立即窗口``
        ''' </remarks>
        Public Function Evaluate(expression As String, Optional env As Environment = Nothing) As Object
            env = If(env, debugger.CurrentEnvironment)

            If env Is Nothing Then
                Return RRuntime.debug.stop("no active runtime environment for the expression evaluation!", env)
            ElseIf expression.StringEmpty(, True) Then
                Return Nothing
            End If

            Dim syntaxError As String = Nothing

            Try
                Dim script As Rscript = Rscript.FromText(expression)
                Dim program As Program = Program.CreateProgram(script, [error]:=syntaxError)

                If program Is Nothing Then
                    Return RRuntime.debug.stop($"syntax error in the debug expression: {syntaxError}", env)
                End If

                ' 在求值的过程之中临时的关闭掉调试器, 避免因为求值的表达式
                ' 本身也会流经 ExecutableLoop 而导致调试器发生递归的重入
                Dim restore As Boolean = debugger.IsDebugging
                debugger.IsDebugging = False

                Try
                    Return program.Execute(env)
                Finally
                    debugger.IsDebugging = restore
                End Try
            Catch ex As Exception
                Return RRuntime.debug.stop(ex, env)
            End Try
        End Function

        ''' <summary>
        ''' 将一个运行时的对象值转换为一段简短的预览文本
        ''' </summary>
        Public Shared Function PreviewValue(value As Object, Optional maxLength As Integer = 64) As String
            If value Is Nothing Then
                Return "NULL"
            End If

            Dim text As String

            Try
                If TypeOf value Is String Then
                    text = $"""{value}"""
                ElseIf TypeOf value Is Array Then
                    Dim vec As Array = DirectCast(value, Array)
                    Dim preview As String = vec.AsObjectEnumerator _
                        .Take(8) _
                        .Select(Function(o) If(o Is Nothing, "NULL", o.ToString)) _
                        .JoinBy(", ")

                    text = $"[{vec.Length}] {preview}{If(vec.Length > 8, ", ...", "")}"
                Else
                    text = value.ToString
                End If
            Catch ex As Exception
                text = $"<{value.GetType.Name}>"
            End Try

            If text.Length > maxLength Then
                text = text.Substring(0, maxLength) & "..."
            End If

            Return text
        End Function
    End Class
End Namespace
