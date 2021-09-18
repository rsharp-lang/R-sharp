Namespace Runtime.Interop.CType

    Public Interface ITupleConstructor

        Function getByName(name As String) As Object
        Function getByIndex(i As Integer) As Object
        Function checkTuple(names As String()) As Boolean

    End Interface
End Namespace