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
        Dim pullAll As New Dictionary(Of String, Object)
        Dim ends As Object = rdata.PullRObject(pullAll)

        Return pullAll
    End Function

    ''' <summary>
    ''' Pull all R# object from the RData linked list
    ''' </summary>
    ''' <param name="rdata"></param>
    ''' <returns></returns>
    ''' 
    <Extension>
    Private Function PullRObject(rdata As RObject, list As Dictionary(Of String, Object)) As Object
        Dim value As RList = rdata.value
        Dim car As RObject = value.CAR

        If value.nodeType = DataType.Vector Then
            ' 已经没有数据了，结束递归
            Return RStreamReader.ReadVector(rdata)
        Else
            ' CAR为当前节点的数据
            ' 获取节点数据，然后继续通过CDR进行链表的递归访问
            Dim current As Object = PullRObject(car, list)
            Dim CDR As RObject = value.CDR

            If CDR Is Nothing Then
                Return current
            Else
                ' 产生一个列表

            End If
        End If

        If Not car.info.type Like elementVectorFlags Then
            ' is r pair list or dataframe
            If Not car.attributes Is Nothing AndAlso car.attributes.tag.characters = "row.names" Then
                Return car.CreateRTable
            ElseIf Not car.attributes Is Nothing AndAlso TypeOf car.attributes.value Is RList Then
                Dim cdr As RObject = car.attributes.value.CDR

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
    End Function

    <Extension>
    Private Function CreatePairList(robj As RObject) As list
        Dim elements As RObject() = robj.value.data
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
            Call list.add(names(i), ConvertToR.PullRObject(elements(i), New Dictionary(Of String, Object)))
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
            Return RStreamReader.ReadStrings(robj.attributes.value.CDR)
        End If
    End Function

    <Extension>
    Private Function readRowNames(robj As RObject) As String()
        Dim attrVals = robj.attributes.value
        Dim cdr As RObject = attrVals.CDR

        If cdr.tag.characters = "row.names" Then
            If TypeOf cdr.value Is RList AndAlso cdr.value.CAR.info.type <> RObjectType.STR Then
                Return Nothing
            End If

            Return RStreamReader.ReadStrings(cdr.value)
        Else
            Return Nothing
        End If
    End Function

    <Extension>
    Private Function CreateRTable(robj As RObject) As dataframe
        Dim columns As RObject() = robj.value.data
        Dim colnames As String() = robj.readColumnNames
        Dim table As New dataframe With {
            .columns = New Dictionary(Of String, Array),
            .rownames = robj.readRowNames
        }

        For i As Integer = 0 To colnames.Length - 1
            table.columns(colnames(i)) = RStreamReader.ReadVector(columns(i))
        Next

        Return table
    End Function
End Module
