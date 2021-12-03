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
            Dim car As RObject = DirectCast(value, RList).CAR

            If Not car.info.type Like elementVectorFlags Then
                ' is r pair list or dataframe
                If Not car.attributes Is Nothing AndAlso car.attributes.tag.characters = "row.names" Then
                    Return car.CreateRTable
                ElseIf Not car.attributes Is Nothing AndAlso TypeOf car.attributes.value Is RList Then
                    Dim cdr As RObject = DirectCast(car.attributes.value, RList).CDR

                    If cdr.tag IsNot Nothing AndAlso cdr.tag.characters = "row.names" Then
                        Return car.CreateRTable
                    Else
                        Return car.CreatePairList
                    End If
                Else
                    Return car.CreatePairList
                End If
            End If

            If car.info.type Like elementVectorFlags Then
                Return car.CreateRVector
            Else
                Throw New NotImplementedException(car.info.ToString)
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
    Private Function readColumnNames(robj As RObject) As String()
        If robj.attributes.tag.characters = "names" Then
            Return RStreamReader.ReadStrings(robj.attributes.value)
        Else
            Return RStreamReader.ReadStrings(DirectCast(robj.attributes.value, RList).CDR)
        End If
    End Function

    <Extension>
    Private Function CreateRTable(robj As RObject) As dataframe
        Dim columns As RObject() = robj.value
        Dim colnames As String() = robj.readColumnNames
        Dim table As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        For i As Integer = 0 To colnames.Length - 1
            table.columns(colnames(i)) = RStreamReader.ReadVector(columns(i))
        Next

        Return table
    End Function
End Module
