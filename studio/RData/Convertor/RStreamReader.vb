Imports REnv = SMRUCC.Rsharp.Runtime

Public Class RStreamReader

    Public Shared Function ReadString(robj As RObject) As String
        Return robj.DecodeCharacters
    End Function

    Public Shared Function ReadNumbers(robj As RObject) As Double()
        Return REnv.asVector(Of Double)(robj.value)
    End Function

    Public Shared Function ReadIntegers(robj As RObject) As Long()
        Return REnv.asVector(Of Long)(robj.value)
    End Function

    Public Shared Function ReadLogicals(robj As RObject) As Boolean()
        Return REnv.asVector(Of Boolean)(robj.value)
    End Function

    ''' <summary>
    ''' read R vector in any element data type
    ''' </summary>
    ''' <param name="robj"></param>
    ''' <returns></returns>
    Public Shared Function ReadVector(robj As RObject) As Array
        Select Case robj.info.type
            Case RObjectType.CHAR : Return ReadString(robj).ToArray
            Case RObjectType.INT : Return ReadIntegers(robj)
            Case RObjectType.REAL : Return ReadNumbers(robj)
            Case RObjectType.LGL : Return ReadLogicals(robj)
            Case Else
                Throw New NotImplementedException(robj.info.ToString)
        End Select
    End Function

End Class
