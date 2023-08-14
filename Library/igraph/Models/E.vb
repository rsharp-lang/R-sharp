#Region "Microsoft.VisualBasic::244afe9bf1d3615eafd5b3c8dc2b0a70, D:/GCModeller/src/R-sharp/Library/igraph//Models/E.vb"

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

'   Total Lines: 122
'    Code Lines: 93
' Comment Lines: 8
'   Blank Lines: 21
'     File Size: 4.37 KB


' Class E
' 
'     Properties: size, weight
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: EvaluateIndexer, (+2 Overloads) getByIndex, (+2 Overloads) getByName, getNames, hasName
'               setByindex, setByIndex, (+2 Overloads) setByName, setNames
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' edge attribute data visistor
''' </summary>
Public Class E : Inherits GraphElementCollection
    Implements RNames, RNameIndex, RIndex, RIndexer

    Friend ReadOnly edges As Edge()
    Friend ReadOnly edgeIndex As Dictionary(Of String, Edge)

    Public ReadOnly Property size As Integer Implements RIndex.length
        Get
            Return edges.Length
        End Get
    End Property

    Default Public ReadOnly Property GetLink(refId As String) As Edge
        Get
            Return edgeIndex.TryGetValue(refId)
        End Get
    End Property

    Public ReadOnly Property weight As vector
        Get
            Return vector.asVector(edges.Select(Function(l) l.weight))
        End Get
    End Property

    Sub New(edges As IEnumerable(Of Edge))
        Call MyBase.New(edges.ToArray)

        Me.edges = DirectCast(list, Edge())
        Me.edgeIndex = Me.edges.ToDictionary(Function(e) e.ID)
    End Sub

#Region "edge attribute data visistors"

    ''' <summary>
    ''' get edge data by name
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Function getByName(name As String) As Object Implements RNameIndex.getByName
        Select Case LCase(name)
            Case "type", NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE, "interaction"
                Return edges _
                    .Select(Function(l) l.data(NamesOf.REFLECTION_ID_MAPPING_INTERACTION_TYPE)) _
                    .ToArray
            Case "weight"
                Return edges _
                    .Select(Function(l) l.weight) _
                    .ToArray
            Case Else
                Return edges _
                    .Select(Function(l) l.data(name)) _
                    .ToArray
        End Select
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

#Region "edge indexer"
    Public Function EvaluateIndexer(expr As Expression, env As Environment) As Object Implements RIndexer.EvaluateIndexer
        Dim i As Object = expr.Evaluate(env)

        If Program.isException(i) Then
            Return i
        Else
            Return SymbolIndexer.getByIndex(edges, i, env)
        End If
    End Function

    Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
        If i > edges.Length Then
            Return Nothing
        Else
            Return New vbObject(edges(i - 1))
        End If
    End Function

    Public Function getByIndex(i() As Integer) As Array Implements RIndex.getByIndex
        Return i.Select(AddressOf getByIndex).ToArray
    End Function

    Public Function setByIndex(i As Integer, value As Object, envir As Environment) As Object Implements RIndex.setByIndex
        Throw New NotImplementedException()
    End Function

    Public Function setByindex(i() As Integer, value As Array, envir As Environment) As Object Implements RIndex.setByindex
        Throw New NotImplementedException()
    End Function
#End Region

End Class
