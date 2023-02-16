#Region "Microsoft.VisualBasic::b83af7194320f9a8eea3c214472b7098, E:/GCModeller/src/R-sharp/studio/RData//Convertor/ConvertToR.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 271
    '    Code Lines: 194
    ' Comment Lines: 34
    '   Blank Lines: 43
    '     File Size: 10.18 KB


    '     Module ConvertToR
    ' 
    '         Function: CreateFactor, CreatePairList, CreateRTable, CreateRVector, PullRawData
    '                   (+2 Overloads) PullRObject, readColumnNames, readRowNames, ToRObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.RDataSet.Flags
Imports SMRUCC.Rsharp.RDataSet.Struct
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

        <Extension>
        Public Function PullRawData(rdata As RData) As Dictionary(Of String, RObject)
            Dim symbols As New Dictionary(Of String, RObject)
            Dim obj As RObject = rdata.object

            Do While obj.value.nodeType = ListNodeType.LinkedList
                Dim value As RList = obj.value
                Dim car As RObject = value.CAR
                Dim nodeType = value.nodeType

                If nodeType = ListNodeType.NA Then
                    Exit Do
                ElseIf nodeType = ListNodeType.Vector Then
                    Call symbols.Add(obj.symbolName, obj)
                    Exit Do
                ElseIf nodeType = ListNodeType.Environment Then
                    Throw New NotImplementedException
                Else
                    ' CAR为当前节点的数据
                    ' 获取节点数据，然后继续通过CDR进行链表的递归访问
                    Dim currentName As String = obj.symbolName
                    Dim CDR As RObject = value.CDR

                    ' pull an object
                    Call symbols.Add(currentName, value.CAR)

                    If CDR Is Nothing Then
                        Exit Do
                    Else
                        ' 向后访问整个链表
                        obj = CDR
                    End If
                End If
            Loop

            Return symbols
        End Function

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

        Public Function PullRObject(rdata As RObject) As Object
            Return PullRObject(rdata, New Dictionary(Of String, Object))
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
            Dim nodeType = value.nodeType

            If nodeType = ListNodeType.NA Then
                Return Nothing
            ElseIf nodeType = ListNodeType.Vector Then
                ' 已经没有数据了，结束递归
                If rdata.value.isPrimitive Then
                    Return rdata.CreateRVector
                End If

                If RObjectSignature.IsPairList(rdata) Then
                    Return rdata.CreatePairList
                ElseIf RObjectSignature.IsDataFrame(rdata) Then
                    Return rdata.CreateRTable
                Else
                    Return rdata.CreateRVector
                End If
            ElseIf nodeType = ListNodeType.Environment Then
                Dim current As New list With {
                    .slots = New Dictionary(Of String, Object)
                }
                Dim env As EnvironmentValue = value.env

                Call current.add(NameOf(EnvironmentValue.enclosure), ToRObject(env.enclosure))
                Call current.add(NameOf(EnvironmentValue.frame), ToRObject(env.frame))
                Call current.add(NameOf(EnvironmentValue.hash_table), ToRObject(env.hash_table))
                Call current.add(NameOf(EnvironmentValue.locked), env.locked)

                Return current
            Else
                ' CAR为当前节点的数据
                ' 获取节点数据，然后继续通过CDR进行链表的递归访问
                Dim current As Object = PullRObject(car, list)
                Dim currentName As String = If(rdata.tag?.characters, rdata.characters)
                Dim CDR As RObject = value.CDR

                ' 20220920 duplicated symbol names?
                If list.ContainsKey(currentName) Then
                    currentName = list.Keys _
                        .JoinIterates({currentName}) _
                        .uniqueNames _
                        .Last
                End If

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
                Call list.add(names(i), ConvertToR.PullRObject(elements(i), New Dictionary(Of String, Object)))
            Next

            Return list
        End Function

        ''' <summary>
        ''' create primitive vector
        ''' </summary>
        ''' <param name="robj"></param>
        ''' <returns></returns>
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
            robj = robj.attributes.LinkVisitor("row.names")

            If Not robj Is Nothing Then
                Dim Rlist As RList = robj.value

                If Rlist.nodeType = ListNodeType.LinkedList Then
                    If Rlist.CAR.info.type = RObjectType.INT Then
                        Return Nothing
                    End If
                End If

                Return RStreamReader.ReadStrings(robj.value)
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
