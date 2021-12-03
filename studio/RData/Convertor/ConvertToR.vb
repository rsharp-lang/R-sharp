Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' Convert to R# object
''' </summary>
Public Module ConvertToR

    ' R storage units (nodes)
    ' Two types: SEXPREC(non-vectors) And VECTOR_SEXPREC(vectors)

    ' Node: VECTOR_SEXPREC
    ' The vector types are RAWSXP, CHARSXP, LGLSXP, INTSXP, REALSXP, CPLXSXP, STRSXP, VECSXP, EXPRSXP And WEAKREFSXP.
    ReadOnly elementVectorFlags As Index(Of RObjectType) = {
        RObjectType.CPLX,
        RObjectType.EXPR,
        RObjectType.INT,
        RObjectType.LGL,
        RObjectType.RAW,
        RObjectType.REAL,
        RObjectType.VEC,
        RObjectType.CHAR,
        RObjectType.STR,
        RObjectType.WEAKREF
    }

    ''' <summary>
    ''' Convert to R# object
    ''' </summary>
    ''' <param name="rdata"></param>
    ''' <returns></returns>
    Public Function ToRObject(rdata As RObject) As Object
        Dim value As Object = rdata.value

        If TypeOf value Is RList Then
            Dim rlist As RList = DirectCast(value, RList)

            If rdata.info.type = RObjectType.LIST Then
                ' is r pair list
                Return rlist.CAR.CreatePairList
            End If

            rdata = DirectCast(value, RList).CAR

            If rdata.info.type Like elementVectorFlags Then
                Return rdata.CreateRVector
            Else
                Throw New NotImplementedException(rdata.info.ToString)
            End If
        ElseIf rdata.info.type Like elementVectorFlags Then
            Return CreateRVector(rdata)
        Else
            Throw New NotImplementedException(rdata.info.ToString)
        End If
    End Function

    <Extension>
    Private Function CreatePairList(robj As RObject) As list
        Dim elements As RObject() = robj.value
        Dim attrTags As RObject = robj.attributes
        Dim names As String()

        If Not attrTags Is Nothing AndAlso attrTags.tag.characters = "names" Then
            names = RStreamReader.ReadStrings(attrTags.value)
        Else
            names = elements _
                .Sequence(offSet:=1) _
                .Select(Function(i) i.ToString) _
                .ToArray
        End If

        Dim list As New list

        For i As Integer = 0 To names.Length - 1
            Call list.add(names(i), ConvertToR.ToRObject(elements(i)))
        Next

        Return list
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
