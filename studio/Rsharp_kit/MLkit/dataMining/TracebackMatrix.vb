Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]

Public Class TracebackMatrix : Implements RIndex

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
End Class
