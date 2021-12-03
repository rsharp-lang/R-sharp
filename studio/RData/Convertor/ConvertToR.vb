Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' Convert to R# object
''' </summary>
Public Module ConvertToR

    ReadOnly elementVectorFlags As Index(Of RObjectType) = {
        RObjectType.BCODE,
        RObjectType.CPLX,
        RObjectType.EXPR,
        RObjectType.INT,
        RObjectType.LGL,
        RObjectType.RAW,
        RObjectType.REAL,
        RObjectType.VEC
    }

    ''' <summary>
    ''' Convert to R# object
    ''' </summary>
    ''' <param name="rdata"></param>
    ''' <returns></returns>
    Public Function ToRObject(rdata As RObject) As Object
        Dim value As Object = rdata.value

        If TypeOf value Is RList Then
            rdata = DirectCast(value, RList).CAR

            If rdata.info.type Like elementVectorFlags Then
                Return rdata.CreateRVector
            Else
                Throw New NotImplementedException(rdata.info.ToString)
            End If
        Else
            Throw New NotImplementedException(rdata.info.ToString)
        End If
    End Function

    <Extension>
    Private Function CreatePairList(robj As RObject) As list

    End Function

    <Extension>
    Private Function CreateRVector(robj As RObject) As vector
        Dim type As RType = robj.info.GetRType
        Dim data As Array = RStreamReader.ReadVector(robj)
        Dim vec As New vector(data, type)

        Return vec
    End Function

    <Extension>
    Private Function CreateRTable(robj As RObject) As dataframe

    End Function
End Module
