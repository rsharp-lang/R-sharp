#Region "Microsoft.VisualBasic::3b756c515b07b4f37003940f4f321c6d, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//dataMining/TracebackMatrix.vb"

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

    '   Total Lines: 94
    '    Code Lines: 60
    ' Comment Lines: 17
    '   Blank Lines: 17
    '     File Size: 3.31 KB


    ' Class TracebackMatrix
    ' 
    '     Properties: length
    ' 
    '     Function: (+3 Overloads) getByIndex, setByindex, setByIndex, toList
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.CType
Imports SMRUCC.Rsharp.Runtime.Vectorization

Public Class TracebackMatrix : Inherits RDefaultFunction
    Implements RIndex, ICTypeList

    Friend data As NamedCollection(Of String)()

    Public ReadOnly Property length As Integer Implements RIndex.length
        Get
            If data.IsNullOrEmpty Then
                Return 0
            End If

            Return data(0).Length
        End Get
    End Property

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="i">1-based index value</param>
    ''' <param name="sort"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' this function returns a tuple list of mapping the cluster entity 
    ''' unique id to the class id by default, otherwise a character vector 
    ''' of the class id will be returns if the parameter sort has been 
    ''' specificed.
    ''' </returns>
    <RDefaultFunction>
    Public Function getByIndex(i As Integer,
                               <RRawVectorArgument>
                               Optional sort As Object = Nothing,
                               Optional env As Environment = Nothing) As Object

        If sort Is Nothing Then
            Return getByIndex(i)
        End If

        Dim sortId As String() = CLRVector.asCharacter(sort)
        Dim list As Dictionary(Of String, String) = DirectCast(getByIndex(i), list).AsGeneric(Of String)(env)
        Dim labels As String() = sortId _
            .Select(Function(id) list.TryGetValue(id, [default]:="no_class")) _
            .ToArray

        Return labels
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="i">1-based offset index</param>
    ''' <returns></returns>
    Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
        Dim list As New Dictionary(Of String, Object)

        i = i - 1

        For Each item As NamedCollection(Of String) In data
            Call list.Add(item.name, item(i))
        Next

        Return New list With {.slots = list}
    End Function

    Public Function getByIndex(i() As Integer) As Array Implements RIndex.getByIndex
        Throw New NotSupportedException
    End Function

    Public Function setByIndex(i As Integer, value As Object, envir As Environment) As Object Implements RIndex.setByIndex
        Return Internal.debug.stop("data is readonly!", envir)
    End Function

    Public Function setByindex(i() As Integer, value As Array, envir As Environment) As Object Implements RIndex.setByindex
        Return Internal.debug.stop("data is readonly!", envir)
    End Function

    Public Function toList() As list Implements ICTypeList.toList
        Return New list With {
            .slots = data _
                .ToDictionary(Function(a) a.name,
                              Function(a)
                                  Return CObj(a.ToArray)
                              End Function)
        }
    End Function

End Class

