#Region "Microsoft.VisualBasic::00000000000000000000000000000000, R#\Interpreter\Debugger\DebugFrame.vb"

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


    '     Class DebugFrame
    ' 
    '         Properties: breakpoint, depth, environment, expression, file
    '                     line
    ' 
    '         Function: GetSourceLocation, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    ''' <summary>
    ''' 程序在某一个暂停点上的运行时快照信息
    ''' </summary>
    ''' <remarks>
    ''' 这个对象会作为 <see cref="DebuggerContext.OnBreakpointHit"/> 
    ''' 事件的参数被传递给宿主程序, 使得宿主程序不需要直接去接触
    ''' 调试器的内部状态就可以获取到当前的暂停位置信息
    ''' </remarks>
    Public Class DebugFrame

        ''' <summary>
        ''' 当前即将被执行的表达式
        ''' </summary>
        Public ReadOnly Property expression As Expression
        ''' <summary>
        ''' 当前的运行时环境, 可以通过这个环境对象来查看局部变量
        ''' </summary>
        Public ReadOnly Property environment As Environment
        ''' <summary>
        ''' 当前的表达式所处的脚本文件路径, 
        ''' 当源代码位置信息不可用的时候这个属性值可能会为空
        ''' </summary>
        Public ReadOnly Property file As String
        ''' <summary>
        ''' 当前的表达式所处的源代码行号, 
        ''' 当源代码位置信息不可用的时候这个属性值为 -1
        ''' </summary>
        Public ReadOnly Property line As Integer
        ''' <summary>
        ''' 当前所处的代码块的嵌套深度
        ''' </summary>
        Public ReadOnly Property depth As Integer
        ''' <summary>
        ''' 触发本次暂停的断点对象, 
        ''' 如果本次暂停是由单步执行所触发的话则这个属性值为空
        ''' </summary>
        Public ReadOnly Property breakpoint As Breakpoint

        Sub New(expression As Expression, env As Environment, depth As Integer, Optional breakpoint As Breakpoint = Nothing)
            Me._expression = expression
            Me._environment = env
            Me._depth = depth
            Me._breakpoint = breakpoint

            Dim location As SourceLocation = SourceLocation.FromExpression(expression)

            Me._file = location.file
            Me._line = location.line
        End Sub

        ''' <summary>
        ''' 获取得到一个便于阅读的源代码位置字符串
        ''' </summary>
        Public Function GetSourceLocation() As String
            If file.StringEmpty(, True) Then
                Return "<unknown location>"
            ElseIf line < 0 Then
                Return file
            Else
                Return $"{file}:{line}"
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"[{GetSourceLocation()}] {expression}"
        End Function
    End Class
End Namespace
