#Region "Microsoft.VisualBasic::b63d1bdc214fbeb1fddd0f661d5ad282, ..\R-sharp\R#\runtime\TypeCodes.vb"

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
        ''' Class type in R#
        ''' </summary>
        [list] = 10
        ''' <summary>
        ''' 函数类型
        ''' </summary>
        [closure]

        ''' <summary>
        ''' <see cref="Integer"/> vector
        ''' </summary>
        [integer] = 100
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

    End Enum
End Namespace
