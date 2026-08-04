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
    '         Function: Evaluate, FormatValue, GetCallStack, GetVariables
    ' 
    '         Class VariableInfo
    ' 
    '             Properties: name, type, value
    ' 
    '             Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Interpreter

    ''' <summary>
    ''' 调试器的运行时检查器
    ''' </summary>
    ''' <remarks>
    ''' 这个类型提供了在程序暂停之后所需要用到的各种运行时状态的检查功能, 
    ''' 其内部的实现都是复用的解释器所已经具备的基础设施:
    ''' 
    ''' + 变量列表: 复用 <see cref="Environment.GetSymbolsNames"/> 以及 <see cref="Environment.FindSymbol"/>
    ''' + 调用堆栈: 复用 <see cref="Environment.stackTrace"/>
    ''' + 表达式求值: 复用 <see cref="Program.BuildProgram"/> 的语法解析链路
    ''' </remarks>
    Public Class DebugInspector

        ReadOnly debugger As DebuggerContext

        Sub New(debugger As DebuggerContext)
            Me.debugger = debugger
        End Sub

        ''' <summary>
        ''' 一个变量的描述信息
        ''' </summary>
        Public Class VariableInfo

            Public Property name As String
            Public Property type As String
            Public Property value As String

            Public Overrides Function ToString() As String
                Return $"{name} <{type}> = {value}"
            End Function
        End Class

        ''' <summary>
        ''' 列出目标环境之中的所有的变量符号
        ''' </summary>
        ''' <param name="env">
        ''' 目标运行时环境, 默认为当前所暂停的位置上的环境对象
        ''' </param>
        ''' <param name="inherits">
        ''' 是否同时列出定义于父环境之中的变量符号?
        ''' </param>
        Public Function GetVariables(Optional env As Environment = Nothing,
                                     Optional inherits As Boolean = False) As VariableInfo()

            env = If(env, debugger.CurrentEnvironment)

            If env Is Nothing Then
                Return {}
            End If

            Dim list As New List(Of VariableInfo)
            Dim names As New Index(Of String)
            Dim current As Environment = env

            Do While Not current Is Nothing
                For Each name As String In current.GetSymbolsNames
                    ' 内层环境之中的同名变量会遮蔽掉外层环境之中的变量, 
                    ' 在这里只保留最内层的那一个
                    If name Like names Then
                        Continue For
                    End If

                    Dim symbol As Symbol = current.FindSymbol(name)

                    If symbol Is Nothing Then
                        Continue For
                    End If

                    Call names.Add(name)
                    Call list.Add(New VariableInfo With {
                        .name = name,
                        .type = symbol.typeof?.ToString,
                        .value = FormatValue(symbol.value)
                    })
                Next

                If Not inherits Then
                    Exit Do
                Else
                    current = current.parent
                End If
            Loop

            Return list.OrderBy(Function(v) v.name).ToArray
        End Function

        ''' <summary>
        ''' 获取得到从当前的位置一直到最顶层的调用堆栈信息
        ''' </summary>
        ''' <param name="env">
        ''' 目标运行时环境, 默认为当前所暂停的位置上的环境对象
        ''' </param>
        Public Function GetCallStack(Optional env As Environment = Nothing) As String()
            env = If(env, debugger.CurrentEnvironment)

            If env Is Nothing Then
                Return {}
            End If

            Dim stacks As StackFrame() = env.stackTrace

            If stacks.IsNullOrEmpty Then
                Return {}
            End If

            Return stacks _
                .Select(Function(frame, i)
                            Return $"[{i}] {frame}"
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' 在指定的运行时环境之中对一个R#表达式进行求值
        ''' </summary>
        ''' <param name="expression">所需要进行求值的R#表达式的源代码</param>
        ''' <param name="env">
        ''' 目标运行时环境, 默认为当前所暂停的位置上的环境对象
        ''' </param>
        ''' <returns>
        ''' 表达式的求值结果. 当语法解析或者是求值的过程之中发生了错误的时候, 
        ''' 函数会返回一个 <see cref="Message"/> 类型的错误对象
        ''' </returns>
        ''' <remarks>
        ''' 请注意, 表达式的求值是在真实的运行时环境之上进行的, 
        ''' 所以类似于``x = 1``这样的赋值表达式是会对程序的运行状态
        ''' 产生实际的副作用的
        ''' </remarks>
        Public Function Evaluate(expression As String, Optional env As Environment = Nothing) As Object
            env = If(env, debugger.CurrentEnvironment)

            If env Is Nothing Then
                Return Internal.debug.stop("no available runtime environment for evaluate the expression!", env)
            ElseIf expression.StringEmpty(, True) Then
                Return Nothing
            End If

            Dim syntaxError As String = Nothing
            Dim program As Program = Program.BuildProgram(expression, [error]:=syntaxError)

            If program Is Nothing Then
                Return Internal.debug.stop({"syntax error while parsing the debug expression!", syntaxError}, env)
            End If

            Try
                ' 这里不可以直接使用 program.Execute, 因为
                ' ExecutableLoop.Execute 会再一次的触发调试器的暂停逻辑, 
                ' 从而造成重入的死锁问题. 在这里通过临时的挂起调试状态来规避
                Return debugger.EvaluateWithoutDebug(program, env)
            Catch ex As Exception
                Return Internal.debug.stop(ex, env)
            End Try
        End Function

        ''' <summary>
        ''' 将一个运行时的对象值格式化为便于阅读的字符串形式
        ''' </summary>
        Public Shared Function FormatValue(value As Object, Optional maxLength As Integer = 120) As String
            If value Is Nothing Then
                Return "NULL"
            End If

            Dim text As String

            Try
                If TypeOf value Is String Then
                    text = $"""{value}"""
                ElseIf TypeOf value Is Array Then
                    Dim vec = DirectCast(value, Array).AsObjectEnumerator.Take(10).ToArray
                    Dim body As String = vec _
                        .Select(Function(o) If(o Is Nothing, "NA", o.ToString)) _
                        .JoinBy(", ")

                    If DirectCast(value, Array).Length > 10 Then
                        body = $"{body}, ..."
                    End If

                    text = $"[{body}]"
                Else
                    text = value.ToString
                End If
            Catch ex As Exception
                text = $"<error: {ex.Message}>"
            End Try

            If text.Length > maxLength Then
                text = text.Substring(0, maxLength) & "..."
            End If

            Return text
        End Function
    End Class
End Namespace
