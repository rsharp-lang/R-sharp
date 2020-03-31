#Region "Microsoft.VisualBasic::f2f40fecf10b4f67e9bf9c1d99187037, R#\Runtime\System\Interface\RNames.vb"

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

    '     Interface IReflector
    ' 
    '         Function: getNames
    ' 
    '     Interface RNames
    ' 
    '         Function: setNames
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
    Public Interface IReflector

        Function getNames() As String()
    End Interface

    Public Interface RNames : Inherits IReflector

        Function setNames(names As String(), envir As Environment) As Object
    End Interface

    Public Interface RIndex

        ReadOnly Property length As Integer

        Function getByIndex(i As Integer) As Object
        Function getByIndex(i As Integer()) As Array
        Function setByIndex(i As Integer, value As Object, envir As Environment) As Object
        Function setByindex(i As Integer(), value As Array, envir As Environment) As Object

    End Interface

    Public Interface RNameIndex : Inherits IReflector

        Function getByName(name As String) As Object
        Function getByName(names As String()) As Object
        Function setByName(name As String, value As Object, envir As Environment) As Object
        Function setByName(names As String(), value As Array, envir As Environment) As Object

    End Interface
End Namespace
