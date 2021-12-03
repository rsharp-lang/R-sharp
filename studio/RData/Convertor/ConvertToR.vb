Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct.LinkedList
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Convertor

    ''' <summary>
    ''' Convert to R# object
    ''' </summary>
    Public Module ConvertToR

        ' R storage units (nodes)
        ' Two types: SEXPREC(non-vectors) And VECTOR_SEXPREC(vectors)

        ' Node: VECTOR_SEXPREC
        ' The vector types are RAWSXP, CHARSXP, LGLSXP, INTSXP, REALSXP, CPLXSXP, STRSXP, VECSXP, EXPRSXP And WEAKREFSXP.
        Friend ReadOnly elementVectorFlags As Index(Of RObjectType) = {
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
            Dim pullAll As New list With {
                .slots = New Dictionary(Of String, Object)
            }

            ' Pull all R# object from the RData linked list
            Call rdata.PullRObject(pullAll.slots)

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

            If value.nodeType = ListNodeType.NA Then
                Return Nothing
            ElseIf value.nodeType = ListNodeType.Vector Then
                ' 已经没有数据了，结束递归
                If RObjectSignature.IsPairList(rdata) Then
                    Return rdata.CreatePairList
                ElseIf RObjectSignature.IsDataFrame(rdata) Then
                    Return rdata.CreateRTable
                Else
                    Return rdata.CreateRVector
                End If
            Else
                ' CAR为当前节点的数据
                ' 获取节点数据，然后继续通过CDR进行链表的递归访问
                Dim current As Object = PullRObject(car, list)
                Dim currentName As String = rdata.tag.characters
                Dim CDR As RObject = value.CDR

                ' pull an object
                Call list.Add(currentName, current)

                If CDR Is Nothing Then
                    Return current
                Else
                    ' 产生一个列表
                    Return PullRObject(CDR, list)
                End If
            End If
        End Function

        <Extension>
        Private Function CreatePairList(robj As RObject) As list
            Dim elements As RObject() = robj.value.data
            Dim attrTags As RObject = robj.attributes
            Dim names As String() = Nothing

            If attrTags IsNot Nothing AndAlso attrTags.tag IsNot Nothing Then
                Dim tag As RObject = attrTags.tag

                If tag.characters = "names" Then
                    names = RStreamReader.ReadStrings(attrTags.value)
                ElseIf tag.referenced_object IsNot Nothing AndAlso tag.referenced_object.characters = "names" Then
                    names = RStreamReader.ReadStrings(attrTags.value)
                End If
            End If

            If names Is Nothing Then
                names = elements _
                    .Sequence(offSet:=1) _
                    .Select(Function(i) i.ToString) _
                    .ToArray
            End If

            Dim list As New list

            For i As Integer = 0 To names.Length - 1
                Call list.add(names(i), ConvertToR.PullRObject(elements(i), Nothing))
            Next

            Return list
        End Function

        <Extension>
        Private Function CreateRVector(robj As RObject) As vector
            Dim type As RType = robj.info.GetRType
            Dim data As Array = RStreamReader.ReadVector(robj)
            Dim factor As factor = If(RObjectSignature.HasFactor(robj), robj.attributes.value.CreateFactor, Nothing)
            Dim vec As New vector(data, type) With {
                .factor = factor
            }

            Return vec
        End Function

        <Extension>
        Private Function CreateFactor(robj As RList) As factor
            Dim data = robj.CAR
            Dim levels As String() = RStreamReader.ReadStrings(data)
            Dim factor As factor = factor.CreateFactor(levels, ordered:=True)

            Return factor
        End Function

        <Extension>
        Private Function readColumnNames(robj As RObject) As String()
            Dim attrs As RObject = robj.attributes

            If attrs.tag.characters = "names" Then
                Return RStreamReader.ReadStrings(attrs.value)
            ElseIf attrs.tag.referenced_object IsNot Nothing Then
                Return RStreamReader.ReadStrings(attrs.value)
            Else
                Return RStreamReader.ReadStrings(attrs.value.CDR)
            End If
        End Function

        <Extension>
        Private Function readRowNames(robj As RObject) As String()
            Dim attrVals As RList = robj.attributes.value
            Dim cdr As RObject = attrVals.CDR

            If cdr.tag.characters = "row.names" Then
                If cdr.value.CAR.info.type <> RObjectType.STR Then
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
            Dim vector As vector
            Dim table As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = robj.readRowNames
            }

            For i As Integer = 0 To colnames.Length - 1
                vector = columns(i).CreateRVector

                If vector.factor Is Nothing Then
                    table.columns(colnames(i)) = vector.data
                Else
                    table.columns(colnames(i)) = factor.asCharacter(vector)
                End If
            Next

            Return table
        End Function
    End Module
End Namespace