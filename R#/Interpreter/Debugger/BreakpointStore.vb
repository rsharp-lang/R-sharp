#Region "Microsoft.VisualBasic::00000000000000000000000000000000, R#\Interpreter\Debugger\BreakpointStore.vb"

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


    '     Class BreakpointStore
    ' 
    '         Properties: count, isEmpty
    ' 
    '         Function: Add, GetBreakpoint, ListAll, Remove, TryHit
    ' 
    '         Sub: Clear, SetEnabled
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Interpreter

    ''' <summary>
    ''' 断点的集合管理器
    ''' </summary>
    ''' <remarks>
    ''' 对外是以``文件名+行号``的方式来进行断点的增删查改操作的, 
    ''' 而在内部则是使用经过规范化处理之后的``文件名:行号``字符串
    ''' 作为键名来建立索引字典, 使得在程序执行的热点循环之中能够以
    ''' O(1) 的时间复杂度来完成断点的命中判断.
    ''' 
    ''' 这里之所以不直接使用 <see cref="Expression.GetBreakPointHashCode"/> 
    ''' 所返回的哈希码来做索引, 是因为该哈希码的计算逻辑位于外部的
    ''' sciBASIC 程序集之中, 其具体的哈希算法对于本项目而言是不透明的, 
    ''' 无法保证由用户所输入的``文件名+行号``能够反推出完全一致的哈希码
    ''' </remarks>
    Public Class BreakpointStore

        ''' <summary>
        ''' [规范化的 file:line =&gt; 断点对象]
        ''' </summary>
        ReadOnly index As New Dictionary(Of String, Breakpoint)

        ''' <summary>
        ''' 当前所注册的断点的总数量
        ''' </summary>
        Public ReadOnly Property count As Integer
            Get
                Return index.Count
            End Get
        End Property

        ''' <summary>
        ''' 当前是否没有注册任何的断点?
        ''' </summary>
        ''' <remarks>
        ''' 这个属性主要是被用于在程序执行的热点循环之中做快速的短路判断
        ''' </remarks>
        Public ReadOnly Property isEmpty As Boolean
            Get
                Return index.Count = 0
            End Get
        End Property

        Private Shared Function key(file As String, line As Integer) As String
            Return $"{SourceLocation.NormalizeFilePath(file)}:{line}"
        End Function

        ''' <summary>
        ''' 添加一个新的断点, 或者更新一个已经存在的断点
        ''' </summary>
        ''' <param name="file">脚本文件的路径</param>
        ''' <param name="line">源代码的行号</param>
        ''' <param name="condition">
        ''' 可选的条件表达式R#源代码, 只有当这个条件表达式的求值结果
        ''' 为逻辑真值的时候程序才会在此处暂停
        ''' </param>
        ''' <returns>所添加的断点对象</returns>
        Public Function Add(file As String, line As Integer, Optional condition As String = Nothing) As Breakpoint
            Dim keyId As String = key(file, line)
            Dim nameId As String = key(System.IO.Path.GetFileName(file), line)
            Dim bp As Breakpoint = Nothing

            If index.TryGetValue(keyId, bp) Then
                ' 断点已经存在了, 在这里只更新其条件表达式
                bp.condition = condition
                bp.enabled = True
            Else
                bp = New Breakpoint With {
                    .file = file,
                    .line = line,
                    .condition = condition,
                    .enabled = True
                }
                ' 同时以完整的文件路径与纯文件名两种键来进行索引, 
                ' 因为在程序执行期间, 表达式的 stackFrame.File 往往只带有
                ' 纯文件名(取决于词法解析器在构建 source 对象时所使用的文件名),
                ' 而用户在注册断点的时候通常是使用完整的文件路径. 用两份索引
                ' 即可保证无论以何种形式注册或者命中的断点都能够正确的匹配上
                index(keyId) = bp
                index(nameId) = bp
            End If

            Return bp
        End Function

        ''' <summary>
        ''' 移除掉指定位置上的断点
        ''' </summary>
        ''' <returns>目标断点是否存在并且已经被成功的移除?</returns>
        Public Function Remove(file As String, line As Integer) As Boolean
            Return index.Remove(key(file, line))
        End Function

        ''' <summary>
        ''' 设置指定位置上的断点的启用状态
        ''' </summary>
        Public Sub SetEnabled(file As String, line As Integer, enabled As Boolean)
            Dim bp As Breakpoint = Nothing

            If index.TryGetValue(key(file, line), bp) Then
                bp.enabled = enabled
            End If
        End Sub

        ''' <summary>
        ''' 清除掉所有的断点
        ''' </summary>
        Public Sub Clear()
            Call index.Clear()
        End Sub

        ''' <summary>
        ''' 获取得到指定位置上所设置的断点对象
        ''' </summary>
        ''' <returns>没有找到目标断点的话会返回空值</returns>
        Public Function GetBreakpoint(file As String, line As Integer) As Breakpoint
            Dim bp As Breakpoint = Nothing
            Return If(index.TryGetValue(key(file, line), bp), bp, Nothing)
        End Function

        ''' <summary>
        ''' 列出当前所注册的全部的断点
        ''' </summary>
        Public Function ListAll() As Breakpoint()
            Return index.Values _
                .OrderBy(Function(bp) bp.file) _
                .ThenBy(Function(bp) bp.line) _
                .ToArray
        End Function

        ''' <summary>
        ''' 判断给定的表达式所处的源代码位置上是否设置有处于启用状态的断点
        ''' </summary>
        ''' <param name="expression">当前即将被执行的表达式</param>
        ''' <returns>
        ''' 命中了断点的话会返回对应的断点对象, 否则返回空值.
        ''' 请注意, 这个函数并不会对条件断点的条件表达式进行求值, 
        ''' 条件的求值是由 <see cref="DebuggerContext"/> 来负责完成的
        ''' </returns>
        Public Function TryHit(expression As Expression) As Breakpoint
            If index.Count = 0 Then
                ' 没有注册任何的断点, 快速短路返回
                Return Nothing
            End If

            Dim location As SourceLocation = SourceLocation.FromExpression(expression)

            If Not location.isValid Then
                ' 当前的表达式没有可用的源代码位置信息, 无法匹配断点
                Return Nothing
            End If

            Dim bp As Breakpoint = Nothing
            Dim fullKey As String = key(location.file, location.line)
            ' 在程序执行期间, 表达式的 stackFrame.File 往往只带有纯文件名
            ' (取决于词法解析器在构建 source 对象时所使用的文件名), 而用户
            ' 在注册断点的时候通常是使用完整的文件路径. 为了能够让这两者有
            ' 效的匹配而不会由于目录前缀的差异而丢失命中, 这里在按照完整
            ' 路径匹配失败之后, 再以纯文件名做一次回退匹配
            Dim nameKey As String = key(System.IO.Path.GetFileName(location.file), location.line)

            System.IO.File.AppendAllText("tryhit_diag.log", $"full={fullKey}|name={nameKey}|valid={location.isValid}|keys={String.Join(",", index.Keys)}" & vbCrLf)

            If (index.TryGetValue(fullKey, bp) OrElse index.TryGetValue(nameKey, bp)) AndAlso bp.enabled Then
                Return bp
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace
