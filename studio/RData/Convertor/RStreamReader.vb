Public Class RStreamReader

    Public Shared Function ReadString(robj As RObject) As String
        Return robj.DecodeCharacters
    End Function

End Class
