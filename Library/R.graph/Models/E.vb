Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Public Class E : Implements RNames, RNameIndex

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
End Class
