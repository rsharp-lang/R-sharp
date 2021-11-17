#Region "Microsoft.VisualBasic::c755ee64da38413add77ddfda665c99a, Library\R.graph\Models\E.vb"

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

' Class E
' 
'     Function: (+2 Overloads) getByName, getNames, hasName, (+2 Overloads) setByName, setNames
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface

''' <summary>
''' edge attribute data visistor
''' </summary>
Public Class E : Implements RNames, RNameIndex

    Friend ReadOnly edges As Edge()

    Sub New(edges As IEnumerable(Of Edge))
        Me.edges = edges.ToArray
    End Sub

#Region "edge attribute data visistors"

    Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
        Throw New NotImplementedException()
    End Function

    Public Function hasName(name As String) As Boolean Implements RNames.hasName
        Throw New NotImplementedException()
    End Function

    Public Function getNames() As String() Implements IReflector.getNames
        Throw New NotImplementedException()
    End Function

    Public Function getByName(name As String) As Object Implements RNameIndex.getByName
        Throw New NotImplementedException()
    End Function

    Public Function getByName(names() As String) As Object Implements RNameIndex.getByName
        Throw New NotImplementedException()
    End Function

    Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
        Throw New NotImplementedException()
    End Function

    Public Function setByName(names() As String, value As Array, envir As Environment) As Object Implements RNameIndex.setByName
        Throw New NotImplementedException()
    End Function
#End Region

End Class
