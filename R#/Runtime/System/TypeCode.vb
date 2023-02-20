#Region "Microsoft.VisualBasic::f4e03aa11d4b266d27bec12d67dfba17, D:/GCModeller/src/R-sharp/R#//Runtime/System/TypeCode.vb"

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

    '   Total Lines: 58
    '    Code Lines: 17
    ' Comment Lines: 36
    '   Blank Lines: 5
    '     File Size: 1.54 KB


    '     Enum TypeCodes
    ' 
    '         [boolean], [closure], [dataframe], [double], [formula]
    '         [integer], [ref], [string]
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Components

    ''' <summary>
    ''' The R# types (byte)
    ''' </summary>
    Public Enum TypeCodes As Byte

        ''' <summary>
        ''' Unknown or invalid
        ''' </summary>
        NA = 0

        ''' <summary>
        ''' Object type in R#.(使用这个类型来表示没有类型约束)
        ''' </summary>
        [generic] = 1
        ''' <summary>
        ''' 函数类型
        ''' </summary>
        [closure]
        ''' <summary>
        ''' Object reference
        ''' </summary>
        [ref]
        [formula]
        environment

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
        ''' <see cref="Double"/> numeric vector
        ''' </summary>
        [double]
        ''' <summary>
        ''' <see cref="String"/> vector
        ''' </summary>
        [string]
        ''' <summary>
        ''' <see cref="Boolean"/> vector
        ''' </summary>
        [boolean]
        [dataframe]
#End Region
    End Enum
End Namespace
