#Region "Microsoft.VisualBasic::94bc2d7bb257f377e4d08a8ebfa19592, ..\R-sharp\R#\runtime\CodeDOM\FunctionExpression.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Runtime.CodeDOM

    ''' <summary>
    ''' Declare a function
    ''' 
    ''' ```
    ''' func &lt;- function(a,b, c as integer = a - 99 + b / 2) {
    '''    return [a * 2, b ^ 3, mean({a, b, c})];
    ''' }
    ''' ```
    ''' </summary>
    Public Class FunctionExpression : Inherits Closure

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns></returns>
        Public Property Name As String
        ''' <summary>
        ''' 当存在表达式初始化的时候为可选参数，反之为必须参数
        ''' </summary>
        ''' <returns></returns>
        Public Property Parameters As VariableDeclareExpression()

    End Class

    Public Class Closure : Inherits PrimitiveExpression


    End Class

    Public Class ForLoop : Inherits Closure

    End Class

    Public Class IfBranch : Inherits Closure

    End Class

    Public Class DoLoop : Inherits Closure

    End Class
End Namespace
