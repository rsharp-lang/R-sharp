#Region "Microsoft.VisualBasic::483908329b27018bb06ce982b9b87cb2, R#\Runtime\System\Interface\RNames.vb"

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

    '   Total Lines: 96
    '    Code Lines: 22 (22.92%)
    ' Comment Lines: 63 (65.62%)
    '    - Xml Docs: 92.06%
    ' 
    '   Blank Lines: 11 (11.46%)
    '     File Size: 3.36 KB


    '     Interface IReflector
    ' 
    '         Function: getNames
    ' 
    '     Interface RNames
    ' 
    '         Function: hasName, setNames
    ' 
    '     Interface RIndex
    ' 
    '         Properties: length
    ' 
    '         Function: (+2 Overloads) getByIndex, setByindex, setByIndex
    ' 
    '     Interface RNameIndex
    ' 
    '         Function: (+2 Overloads) getByName, (+2 Overloads) setByName
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Components.Interface

    ''' <summary>
    ''' The reflection operation helper in R# language runtime
    ''' </summary>
    ''' <remarks>
    ''' used this model for implements the get names:
    ''' 
    ''' 1. for vector and list, implements ``names`` function
    ''' 2. for dataframe object, implements the ``colnames`` function
    ''' </remarks>
    Public Interface IReflector

        Function getNames() As String()
    End Interface

    Public Interface RNames : Inherits IReflector

        Function setNames(names As String(), envir As Environment) As Object
        Function hasName(name As String) As Boolean
    End Interface

    ''' <summary>
    ''' index start from base 1
    ''' </summary>
    Public Interface RIndex

        ReadOnly Property length As Integer

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="i">index i should be start from base 1</param>
        ''' <returns></returns>
        Function getByIndex(i As Integer) As Object
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="i">index i should be start from base 1</param>
        ''' <returns></returns>
        Function getByIndex(i As Integer()) As Array
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="i">index i should be start from base 1</param>
        ''' <param name="value"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Function setByIndex(i As Integer, value As Object, envir As Environment) As Object
        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="i">index i should be start from base 1</param>
        ''' <param name="value"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Function setByindex(i As Integer(), value As Array, envir As Environment) As Object

    End Interface

    ''' <summary>
    ''' a collection data model in R# language that indicates each element has names
    ''' </summary>
    Public Interface RNameIndex : Inherits IReflector

        ''' <summary>
        ''' get an element by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Function getByName(name As String) As Object
        ''' <summary>
        ''' get a set of data elements by name set
        ''' </summary>
        ''' <param name="names"></param>
        ''' <returns></returns>
        Function getByName(names As String()) As Object
        ''' <summary>
        ''' set data element value by a given name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Function setByName(name As String, value As Object, envir As Environment) As Object
        ''' <summary>
        ''' set a data collection of values by a given name list
        ''' </summary>
        ''' <param name="names"></param>
        ''' <param name="value"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Function setByName(names As String(), value As Array, envir As Environment) As Object

    End Interface
End Namespace
