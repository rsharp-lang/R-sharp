Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Public Class V : Implements RNames, RNameIndex

    ReadOnly vertex As Node()
    ReadOnly dataNames As Index(Of String)

    Sub New(g As NetworkGraph)
        vertex = g.vertex.ToArray
        dataNames = vertex.Select(Function(v) v.data.Properties.Keys).IteratesALL.Distinct.ToArray
    End Sub

    Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
        Throw New NotImplementedException()
    End Function

    Public Function hasName(name As String) As Boolean Implements RNames.hasName
        Return name Like dataNames
    End Function

    Public Function getNames() As String() Implements IReflector.getNames
        Return dataNames.Objects
    End Function

    Public Function getByName(name As String) As Object Implements RNameIndex.getByName
        Return vertex.Select(Function(v) v.data(name)).ToArray
    End Function

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
        Throw New NotImplementedException()
    End Function

    Public Function setByName(names() As String, value As Array, envir As Environment) As Object Implements RNameIndex.setByName
        Throw New NotImplementedException()
    End Function
End Class
