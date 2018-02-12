#Region "Microsoft.VisualBasic::a9b15d9d2974681086d25c48816dc05f, R#\Interpreter\Language\Tokens.vb"

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

    '     Enum Tokens
    ' 
    '         [Boolean], [Function], [Operator], [String], AbsVector
    '         Comment, DotNetMethodCall, DotNetSharedAPI, LeftAssign, Numeric
    '         ParamDelimiter, ParameterAssign, ParenClose, ParenOpen, Pipeline
    '         Priority, StringContact, Tuple, Variable, VectorDeclare
    '         VectorNorm
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.Language

    ''' <summary>
    ''' The R# language tokens
    ''' </summary>
    Public Enum Tokens As Byte

        undefine = 0
        ''' <summary>
        ''' identifier, value expression, etc.(允许使用小数点作为变量名称的一部分)
        ''' </summary>
        [Object] = 10
        ''' <summary>
        ''' &lt;-
        ''' </summary>
        LeftAssign
        ''' <summary>
        ''' =
        ''' </summary>
        ParameterAssign
        [Operator]
        ''' <summary>
        ''' 使用``:``操作符来调用``.NET``方法
        ''' 使用``$``操作符调用自身的R#方法
        ''' </summary>
        DotNetMethodCall
        ''' <summary>
        ''' 使用``::``来调用.NET的静态API，例如定义在VB.NET的标准模块之中的函数方法
        ''' </summary>
        DotNetSharedAPI
        ''' <summary>
        ''' ,
        ''' </summary>
        ParamDelimiter
        ''' <summary>
        ''' |
        ''' </summary>
        Pipeline
        ''' <summary>
        ''' 目标括号对象是一个优先级的改变运算，而非函数调用 ``(...)``
        ''' </summary>
        Priority
        ''' <summary>
        ''' [...] tuple类型
        ''' </summary>
        Tuple
        ''' <summary>
        ''' |...| 向量的申明语句
        ''' </summary>
        VectorDeclare
        ''' <summary>
        ''' |x| 求取数值向量的绝对值
        ''' </summary>
        AbsVector
        ''' <summary>
        ''' ||x|| 求取向量的模
        ''' </summary>
        VectorNorm
        ''' <summary>
        ''' ([{
        ''' </summary>
        ParenOpen
        ''' <summary>
        ''' )}]
        ''' </summary>
        ParenClose

        ''' <summary>
        ''' ``&amp;``
        ''' </summary>
        StringContact
        ''' <summary>
        ''' Variable declare init
        ''' </summary>
        Variable
        ''' <summary>
        ''' 字符串值
        ''' </summary>
        [String]
        ''' <summary>
        ''' 数值类型
        ''' </summary>
        Numeric
        ''' <summary>
        ''' 逻辑值类型
        ''' </summary>
        [Boolean]
        Comment
        [Function]
    End Enum
End Namespace
