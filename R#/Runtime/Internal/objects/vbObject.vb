Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal

    ''' <summary>
    ''' Proxy for VB.NET class <see cref="Object"/>
    ''' </summary>
    Public Class vbObject : Implements RNameIndex

        Public ReadOnly Property target As Object
        Public ReadOnly Property type As RType

        Sub New(obj As Object)
            target = obj
            type = New RType(obj.GetType)
        End Sub

        ''' <summary>
        ''' Get property value by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function getByName(name As String) As Object Implements RNameIndex.getByName
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' Get properties value collection by a given name list
        ''' </summary>
        ''' <param name="names"></param>
        ''' <returns></returns>
        Public Function getByName(names() As String) As Object Implements RNameIndex.getByName
            Throw New NotImplementedException()
        End Function

        Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            Throw New NotImplementedException()
        End Function

        Public Function setByName(names() As String, value As Array, envir As Environment) As Object Implements RNameIndex.setByName
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace