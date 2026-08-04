#Region "Microsoft.VisualBasic::00000000000000000000000000000000, R#\Interpreter\Debugger\Breakpoint.vb"

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


    '     Class Breakpoint
    ' 
    '         Properties: condition, enabled, file, hitCount, line
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter

    ''' <summary>
    ''' 一个断点的描述模型
    ''' </summary>
    ''' <remarks>
    ''' 断点是通过``文件名+行号``的方式来进行定位的, 在
    ''' <see cref="BreakpointStore"/> 的内部则会将其转换为
    ''' 与表达式的 <see cref="Expression.GetBreakPointHashCode"/>
    ''' 相一致的哈希码来做 O(1) 的快速匹配
    ''' </remarks>
    Public Class Breakpoint

        ''' <summary>
        ''' 断点所处的脚本文件的文件路径
        ''' </summary>
        Public Property file As String
        ''' <summary>
        ''' 断点所处的源代码行号
        ''' </summary>
        Public Property line As Integer

        ''' <summary>
        ''' 条件断点的条件表达式的R#源代码, 
        ''' 当这个属性值为空的时候则表示这是一个无条件断点
        ''' </summary>
        ''' <remarks>
        ''' 只有当这个条件表达式的求值结果为逻辑真值的时候, 
        ''' 程序才会在这个断点位置上暂停下来
        ''' </remarks>
        Public Property condition As String

        ''' <summary>
        ''' 这个断点当前是否处于启用状态? 
        ''' 被禁用掉的断点不会被命中, 但是仍然会保留在断点列表之中
        ''' </summary>
        Public Property enabled As Boolean = True

        ''' <summary>
        ''' 这个断点已经被命中的次数
        ''' </summary>
        Public Property hitCount As Integer

        ''' <summary>
        ''' 这个断点所对应的表达式哈希码, 
        ''' 由 <see cref="BreakpointStore"/> 在注册的时候负责填充
        ''' </summary>
        Friend Property hashCode As Integer

        Public Overrides Function ToString() As String
            Dim location As String = $"{file}:{line}"

            If Not condition.StringEmpty(, True) Then
                location = $"{location} when ({condition})"
            End If
            If Not enabled Then
                location = $"{location} [disabled]"
            End If
            If hitCount > 0 Then
                location = $"{location} (hits: {hitCount})"
            End If

            Return location
        End Function
    End Class
End Namespace
