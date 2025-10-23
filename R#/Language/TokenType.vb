#Region "Microsoft.VisualBasic::0ee3324dfc9b9397f0ac72e47094ecfd, R#\Language\TokenType.vb"

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

    '   Total Lines: 78
    '    Code Lines: 27 (34.62%)
    ' Comment Lines: 42 (53.85%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (11.54%)
    '     File Size: 1.81 KB


    '     Enum TokenType
    ' 
    '         [operator], annotation, booleanLiteral, cliShellInvoke, close
    '         comma, comment, delimiter, identifier, iif
    '         integerLiteral, invalid, keyword, lineContinue, missingLiteral
    '         newLine, numberLiteral, open, regexp, sequence
    '         stringInterpolation, stringLiteral, terminator
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Language

    ''' <summary>
    ''' Language syntax token types
    ''' </summary>
    Public Enum TokenType As Byte

        ''' <summary>
        ''' Syntax error
        ''' </summary>
        invalid

        ''' <summary>
        ''' cr/lf
        ''' </summary>
        newLine
        ''' <summary>
        ''' &lt;whitespace> and &lt;TAB> as delimiter
        ''' </summary>
        delimiter

        comment
        ''' <summary>
        ''' 类似于在VisualBasic中的自定义属性的注解语法
        ''' </summary>
        annotation

        identifier
        ''' <summary>
        ''' 必须要使用``;``作为表达式的结束分隔符
        ''' </summary>
        terminator
        comma
        keyword
        [operator]
        ''' <summary>
        ''' :
        ''' </summary>
        sequence
        ''' <summary>
        ''' ?
        ''' </summary>
        iif

        ''' <summary>
        ''' 字符串插值语法，与javascript脚本之中的字符串插值保持一致
        ''' </summary>
        stringInterpolation
        ''' <summary>
        ''' 命令行调用语法
        ''' </summary>
        cliShellInvoke
        regexp

        ''' <summary>
        ''' NULL, NA, Inf
        ''' </summary>
        missingLiteral
        stringLiteral
        numberLiteral
        integerLiteral
        booleanLiteral

        ''' <summary>
        ''' 左边的括号与大括号
        ''' </summary>
        open
        ''' <summary>
        ''' 右边的括号与大括号
        ''' </summary>
        close

        ''' <summary>
        ''' ... line continue token for matlab/octave language
        ''' </summary>
        lineContinue
    End Enum
End Namespace
