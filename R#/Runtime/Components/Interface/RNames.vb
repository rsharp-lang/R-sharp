Namespace Runtime.Components

    Public Interface RNames

        Function getNames() As String()
        Function setNames(names As String(), envir As Environment) As Object
    End Interface
End Namespace