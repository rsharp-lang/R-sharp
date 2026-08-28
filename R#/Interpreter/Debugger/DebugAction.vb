#Region "Microsoft.VisualBasic::290f84394471fcf75dbe160a5c74910c, R#\Interpreter\Debugger\DebugAction.vb"

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

    '   Total Lines: 13
    '    Code Lines: 8 (61.54%)
    ' Comment Lines: 3 (23.08%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (15.38%)
    '     File Size: 371 B


    '     Enum DebugAction
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter

    ''' <summary>
    ''' 调试动作类型
    ''' </summary>
    ''' <remarks>
    ''' 单步语义均以 <see cref="DebuggerContext.stackDepth"/> 所记录的
    ''' 块嵌套深度为判定基准, 具体的判定规则请参见
    ''' <see cref="DebuggerContext.ShouldPause"/> 函数.
    ''' </remarks>
    Public Enum DebugAction
        ''' <summary>
        ''' 继续运行, 直到命中下一个断点为止
        ''' </summary>
        [Continue]
        ''' <summary>
        ''' 单步跳过: 在与当前语句相同或者更外层的位置暂停,
        ''' 被调用的函数体/循环体将会被完整执行完毕而不会中途暂停
        ''' </summary>
        [StepOver]
        ''' <summary>
        ''' 单步进入: 在下一条被执行的语句处暂停, 无论其位于哪一个嵌套层级,
        ''' 因此可以进入用户所自定义的函数体内部
        ''' </summary>
        [StepInto]
        ''' <summary>
        ''' 单步跳出: 一直执行到当前的代码块返回至其外层代码块之后再暂停
        ''' </summary>
        [StepOut]
        ''' <summary>
        ''' 停止执行整个脚本程序
        ''' </summary>
        [Stop]
    End Enum

End Namespace
