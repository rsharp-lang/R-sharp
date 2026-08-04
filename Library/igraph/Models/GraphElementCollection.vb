#Region "Microsoft.VisualBasic::fee85ce1d05546e34d4964f8c99d946b, Library\igraph\Models\GraphElementCollection.vb"

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

    '   Total Lines: 61
    '    Code Lines: 43 (70.49%)
    ' Comment Lines: 7 (11.48%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (18.03%)
    '     File Size: 2.19 KB


    ' Class GraphElementCollection
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: getNames, hasName, loadDataNames, setNames
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Expressions
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]

Public Class GraphElementCollection : Implements RNames

    ''' <summary>
    ''' all data attribute names in each vertex node object
    ''' </summary>
    Protected ReadOnly dataNames As Index(Of String)
    Protected ReadOnly list As Array

    Sub New(collection As IEnumerable(Of Node))
        list = collection.ToArray
        dataNames = loadDataNames(DirectCast(list, Node()).Select(Function(a) DirectCast(a.data, GraphData)))
    End Sub

    Sub New(collection As IEnumerable(Of Edge))
        list = collection.ToArray
        dataNames = loadDataNames(DirectCast(list, Edge()).Select(Function(a) DirectCast(a.data, GraphData)))
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Private Shared Function loadDataNames(data As IEnumerable(Of GraphData)) As Index(Of String)
        Return data _
            .Select(Function(v)
                        Return v.Properties.Keys
                    End Function) _
            .IteratesALL _
            .Distinct _
            .ToArray
    End Function


#Region "Metadata Attribute Data Accessor"

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
        Throw New NotImplementedException()
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function hasName(name As String) As Boolean Implements RNames.hasName
        Return name Like dataNames
    End Function

    ''' <summary>
    ''' get all node vertex attribute names
    ''' </summary>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function getNames() As String() Implements IReflector.getNames
        Return dataNames.Objects
    End Function
#End Region

End Class
