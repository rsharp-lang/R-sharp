#Region "Microsoft.VisualBasic::eb1013d41b41b2165c167cc4709c8d9b, D:/GCModeller/src/R-sharp/Library/igraph//Models/V.vb"

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

    '   Total Lines: 215
    '    Code Lines: 158
    ' Comment Lines: 23
    '   Blank Lines: 34
    '     File Size: 7.83 KB


    ' Class V
    ' 
    '     Properties: size
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: ConfigSymbols, eval, EvaluateIndexer, (+2 Overloads) getByIndex, (+2 Overloads) getByName
    '               getNames, hasName, index, setByindex, setByIndex
    '               (+2 Overloads) setByName, setNames
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' node attribute data visitor
''' </summary>
Public Class V : Implements RNames, RNameIndex, RIndex, RIndexer

    Friend ReadOnly vertex As Node()

    ''' <summary>
    ''' all data attribute names in each vertex node object
    ''' </summary>
    ReadOnly dataNames As Index(Of String)
    ReadOnly vertexIndex As Dictionary(Of String, Node)
    ''' <summary>
    ''' ordinal order of <see cref="vertex"/>
    ''' </summary>
    ReadOnly i As Index(Of String)

    ''' <summary>
    ''' the size of the vertex collection in this data visitor model
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property size As Integer Implements RIndex.length
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return vertex.Length
        End Get
    End Property

    ''' <summary>
    ''' get a vertex node via a uniqueId reference. 
    ''' </summary>
    ''' <param name="id"></param>
    ''' <returns></returns>
    Default Public ReadOnly Property GetVertex(id As String) As Node
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            Return vertexIndex.TryGetValue(id)
        End Get
    End Property

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Sub New(g As NetworkGraph, Optional allConnected As Boolean = False)
        Call Me.New(If(allConnected, g.connectedNodes, g.vertex))
    End Sub

    Sub New(list As IEnumerable(Of Node))
        vertex = list.ToArray
        dataNames = vertex _
            .Select(Function(v)
                        Return v.data.Properties.Keys
                    End Function) _
            .IteratesALL _
            .Distinct _
            .ToArray
        vertexIndex = vertex.ToDictionary(Function(v) v.label)
        i = vertex.Select(Function(v) v.label).Indexing
    End Sub

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function index(vlabs As IEnumerable(Of String), Optional base As Integer = 1) As Integer()
        Return vlabs _
            .Select(Function(lab)
                        Return If(i.IndexOf(lab) < 0, -1, i.IndexOf(lab) + base)
                    End Function) _
            .ToArray
    End Function

#Region "Node Attribute Data"

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
        Throw New NotImplementedException()
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function hasName(name As String) As Boolean Implements RNames.hasName
        Return name Like dataNames
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function getNames() As String() Implements IReflector.getNames
        Return dataNames.Objects
    End Function

    ''' <summary>
    ''' get attribute value via attribute name
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Function getByName(name As String) As Object Implements RNameIndex.getByName
        If name = "group" AndAlso Not name Like dataNames Then
            name = NamesOf.REFLECTION_ID_MAPPING_NODETYPE
        ElseIf name = "label" Then
            Return (From v As Node
                    In vertex
                    Select v.data.label).ToArray
        End If

        Return (From v As Node In vertex Select v.data(name)).ToArray
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function getByName(names() As String) As Object Implements RNameIndex.getByName
        Return New list With {
            .slots = names _
                .ToDictionary(Function(name) name,
                              Function(name)
                                  Return getByName(name)
                              End Function)
        }
    End Function

    Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
        Dim data As String() = CLRVector.safeCharacters(value)

        If name = "label" Then
            For i As Integer = 0 To vertex.Length - 1
                vertex(i).data.label = data(i)
            Next
        ElseIf name = "group" Then
            For i As Integer = 0 To vertex.Length - 1
                vertex(i).data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = data(i)
            Next
        Else
            For i As Integer = 0 To vertex.Length - 1
                vertex(i).data(name) = data(i)
            Next
        End If

        Return value
    End Function

    Public Function setByName(names() As String, value As Array, envir As Environment) As Object Implements RNameIndex.setByName
        If names.Length = 1 Then
            Return setByName(names(Scan0), value, envir)
        End If

        Throw New NotImplementedException()
    End Function
#End Region

#Region "vertex indexer"
    Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
        If i > vertex.Length Then
            Return Nothing
        Else
            Return New vbObject(vertex(i - 1))
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

    Private Function ConfigSymbols(expr As Expression, env As Environment) As Environment
        Dim symbols = SymbolAnalysis.GetSymbolReferenceList(expr).ToArray

        env = New Environment(env, "RIndexer.EvaluateIndexer")

        For Each name As NamedValue(Of PropertyAccess) In symbols
            Call env.Push(name.Name, getByName(name.Name), [readonly]:=True)
        Next

        Return env
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function eval(expr As Expression, env As Environment) As Object
        Return expr.Evaluate(ConfigSymbols(expr, env))
    End Function

    Public Function EvaluateIndexer(expr As Expression, env As Environment) As Object Implements RIndexer.EvaluateIndexer
        Dim i As Object = eval(expr, env)

        If Program.isException(i) Then
            Return i
        Else
            Dim subset = SymbolIndexer.getByIndex(vertex, i, env)

            If Program.isException(subset) Then
                Return subset
            End If

            Dim nodes As Node() = REnv.asVector(Of Node)(subset)
            Dim v As New V(nodes)

            Return v
        End If
    End Function
#End Region
End Class
