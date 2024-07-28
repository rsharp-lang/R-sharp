#Region "Microsoft.VisualBasic::9f85f2b3cd7e0b176cbe26147fd0e404, Library\base\VB6.0.vb"

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

    '   Total Lines: 32
    '    Code Lines: 15 (46.88%)
    ' Comment Lines: 14 (43.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (9.38%)
    '     File Size: 1.16 KB


    ' Module VB6
    ' 
    '     Function: conversion_val
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

''' <summary>
''' Microsoft VisualBasic 6.0 primitive functions
''' </summary>
<Package("Microsoft.VisualBasic")>
Module VB6

    ''' <summary>
    ''' Returns the numbers contained in a string as a numeric value of appropriate type.
    ''' </summary>
    ''' <param name="x">
    ''' Required. Any valid String expression, Object variable, or Char value. If Expression
    ''' is of type Object, its value must be convertible to String or an System.ArgumentException
    ''' error occurs.
    ''' </param>
    ''' <returns>
    ''' The numbers contained in a string as a numeric value of appropriate type.
    ''' </returns>
    <ExportAPI("Val")>
    Public Function conversion_val(<RRawVectorArgument> x As Object) As Double()
        Return CLRVector.asCharacter(x) _
            .SafeQuery _
            .Select(Function(str) Conversion.Val(str)) _
            .ToArray
    End Function

End Module

