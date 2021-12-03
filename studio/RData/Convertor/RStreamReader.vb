Imports REnv = SMRUCC.Rsharp.Runtime

Public Class RStreamReader

    Public Shared Function ReadString(robj As RObject) As String
        Return robj.DecodeCharacters
    End Function

    Public Shared Function ReadNumbers(robj As RObject) As Double()
        Return REnv.asVector(Of Double)(robj.value)
    End Function

End Class
