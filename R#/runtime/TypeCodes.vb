#Region "Microsoft.VisualBasic::f47b694235d91a445146f0c87d234261, ..\R-sharp\R#\runtime\TypeCodes.vb"

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

Namespace Runtime

    ''' <summary>
    ''' The R# types
    ''' </summary>
    Public Enum TypeCodes As Byte

        ''' <summary>
        ''' Object type in R#.(使用这个类型来表示没有类型约束)
        ''' </summary>
        [generic] = 0
        ''' <summary>
        ''' 函数类型
        ''' </summary>
        [closure]
        ''' <summary>
        ''' Object reference
        ''' </summary>
        ref

#Region "PrimitiveTypes"

        ''' <summary>
        ''' Class type in R#
        ''' </summary>
        ''' <remarks>
        ''' The R# list is the Dictionary type in VB.NET
        ''' 
        ''' R#之中的list类型就是.NET之中的字典类型，对于所有的list类型的R对象而言，
        ''' 尽管他们的属性的数量和名称不相同，但是都是list字典类型
        ''' </remarks>
        [list] = 100
        ''' <summary>
        ''' <see cref="Integer"/> vector
        ''' </summary>
        [integer]
        ''' <summary>
        ''' <see cref="ULong"/> vector
        ''' </summary>
        [uinteger]
        ''' <summary>
        ''' <see cref="Double"/> numeric vector
        ''' </summary>
        [double]
        ''' <summary>
        ''' <see cref="String"/> vector
        ''' </summary>
        [string]
        ''' <summary>
        ''' <see cref="Char"/> vector
        ''' </summary>
        [char]
        ''' <summary>
        ''' <see cref="Boolean"/> vector
        ''' </summary>
        [boolean]
#End Region
    End Enum
End Namespace
