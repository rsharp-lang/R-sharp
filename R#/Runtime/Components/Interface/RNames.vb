Namespace Runtime.Components.Interface

    Public Interface RNames

        Function getNames() As String()
        Function setNames(names As String(), envir As Environment) As Object
    End Interface

    Public Interface RIndex

        Function getByIndex(i As Integer) As Object
        Function getByIndex(i As Integer()) As Object()
        Function setByIndex(i As Integer, value As Object, envir As Environment) As Object
        Function setByindex(i As Integer(), value As Array, envir As Environment) As Object

    End Interface

    Public Interface RNameIndex

        Function getByName(name As String) As Object
        Function getByName(names As String()) As Object
        Function setByName(name As String, value As Object, envir As Environment) As Object
        Function setByName(names As String(), value As Array, envir As Environment) As Object

    End Interface
End Namespace