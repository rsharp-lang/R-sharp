Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' Convert to R# object
''' </summary>
Public Module ConvertToR

    ''' <summary>
    ''' Convert to R# object
    ''' </summary>
    ''' <param name="rdata"></param>
    ''' <returns></returns>
    Public Function ToRObject(rdata As RData) As Object

    End Function

    <Extension>
    Private Function CreatePairList(robj As RObject) As list

    End Function

    <Extension>
    Private Function CreateRVector(robj As RObject) As vector

    End Function

    <Extension>
    Private Function CreateRTable(robj As RObject) As dataframe

    End Function
End Module
