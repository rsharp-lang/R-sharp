#Region "Microsoft.VisualBasic::00000000000000000000000000000000, R#\Interpreter\Debugger\SourceLocation.vb"

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


    '     Structure SourceLocation
    ' 
    '         Properties: file, line
    ' 
    '         Function: (+2 Overloads) FromExpression, NormalizeFilePath, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Interpreter

    ''' <summary>
    ''' 表达式的源代码位置信息
    ''' </summary>
    ''' <remarks>
    ''' 因为 <see cref="StackFrame.Line"/> 属性的数据类型是字符串, 
    ''' 并且在部分的语法构建器之中会被填充为``n/a``这样的无效值, 
    ''' 所以在这里统一封装源代码位置的解析逻辑, 避免在调试器的
    ''' 各个模块之中重复处理这些边界情况
    ''' </remarks>
    Public Structure SourceLocation

        ''' <summary>
        ''' 脚本文件的路径, 无效的时候为空值
        ''' </summary>
        Public ReadOnly Property file As String
        ''' <summary>
        ''' 源代码行号, 无效的时候为 -1
        ''' </summary>
        Public ReadOnly Property line As Integer

        ''' <summary>
        ''' 当前的这个源代码位置信息是否是有效的?
        ''' </summary>
        Public ReadOnly Property isValid As Boolean
            Get
                Return Not file.StringEmpty(, True) AndAlso line >= 0
            End Get
        End Property

        Sub New(file As String, line As Integer)
            Me._file = file
            Me._line = line
        End Sub

        Public Overrides Function ToString() As String
            Return If(isValid, $"{file}:{line}", "<unknown location>")
        End Function

        ''' <summary>
        ''' 从一个表达式对象之上提取出其源代码位置信息
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns>
        ''' 当目标表达式并没有实现 <see cref="IRuntimeTrace"/> 接口的时候, 
        ''' 函数会返回一个无效的位置信息对象
        ''' </returns>
        Public Shared Function FromExpression(expression As Expression) As SourceLocation
            Return FromStackFrame(TryCast(expression, IRuntimeTrace)?.stackFrame)
        End Function

        ''' <summary>
        ''' 从一个堆栈帧对象之上提取出其源代码位置信息
        ''' </summary>
        ''' <param name="frame"></param>
        ''' <returns></returns>
        Public Shared Function FromStackFrame(frame As StackFrame) As SourceLocation
            If frame Is Nothing Then
                Return New SourceLocation(Nothing, -1)
            End If

            Dim lineNum As Integer

            ' 注意: StackFrame.Line 的数据类型是字符串, 在部分的语法
            ' 构建器之中会被填充为 "n/a" 这样的无效值, 在这里解析失败
            ' 的时候统一使用 -1 来表示行号未知
            If Not Integer.TryParse(frame.Line, lineNum) Then
                lineNum = -1
            End If

            Return New SourceLocation(frame.File, lineNum)
        End Function

        ''' <summary>
        ''' 对脚本文件的路径做规范化处理
        ''' </summary>
        ''' <param name="file"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 统一使用``/``作为路径分隔符并且转换为小写形式, 
        ''' 使得用户在注册断点的时候所输入的路径能够与
        ''' 语法构建器之中所记录的 <see cref="StackFrame.File"/> 相匹配
        ''' </remarks>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function NormalizeFilePath(file As String) As String
            If file.StringEmpty(, True) Then
                Return ""
            Else
                Return file.Replace("\", "/").ToLower.Trim(" "c, ControlChars.Quot)
            End If
        End Function
    End Structure
End Namespace
