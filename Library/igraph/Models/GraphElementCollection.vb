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