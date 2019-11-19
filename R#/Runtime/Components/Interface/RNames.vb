#Region "Microsoft.VisualBasic::83fc2ff81b9be15e9f65e284d1e8d506, R#\Runtime\Components\Interface\RNames.vb"

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

    '     Interface RNames
    ' 
    '         Function: getNames, setNames
    ' 
    '     Interface RIndex
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

    Public Interface RNames

        Function getNames() As String()
        Function setNames(names As String(), envir As Environment) As Object
    End Interface

    Public Interface RIndex

        Function getByIndex(i As Integer) As Object
        Function getByIndex(i As Integer()) As Object()
        Function setByIndex(i As Integer, value As Object, envir As Environment) As Object
        Function setByindex(i As Integer(), value As Array, envir As Environment) As Object

    End Interface

    Public Interface RNameIndex

        Function getByName(name As String) As Object
        Function getByName(names As String()) As Object
        Function setByName(name As String, value As Object, envir As Environment) As Object
        Function setByName(names As String(), value As Array, envir As Environment) As Object

    End Interface
End Namespace
