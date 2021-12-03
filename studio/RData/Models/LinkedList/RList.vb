Namespace Struct.LinkedList

    ''' <summary>
    ''' 在R数据文件之中用于存储数据的链表结构
    ''' </summary>
    Public Class RList

        ''' <summary>
        ''' 当前节点的数据
        ''' </summary>
        ''' <returns></returns>
        Public Property CAR As RObject

        ''' <summary>
        ''' 链表中的下一个节点
        ''' </summary>
        ''' <returns></returns>
        Public Property CDR As RObject

        ''' <summary>
        ''' 当前节点所存储的向量数据
        ''' </summary>
        ''' <returns></returns>
        Public Property data As Array

        Public ReadOnly Property nodeType As ListNodeType
            Get
                If Not data Is Nothing Then
                    Return ListNodeType.Vector
                Else
                    Return ListNodeType.LinkedList
                End If
            End Get
        End Property

        Friend Shared Function CreateNode(value As Object) As RList
            If value Is Nothing Then
                Return New RList
            ElseIf value.GetType.IsArray Then
                Return New RList With {
                    .data = DirectCast(value, Array)
                }
            ElseIf TypeOf value Is ValueTuple Then
                With DirectCast(value, (RObject, RObject))
                    Dim CAR = .Item1
                    Dim CDR = .Item2

                    Return New RList With {
                        .CAR = CAR,
                        .CDR = CDR
                    }
                End With
            ElseIf TypeOf value Is RObject Then
                Return New RList With {
                    .CAR = value
                }
            ElseIf TypeOf value Is RList Then
                Return value
            End If

            Throw New NotImplementedException(value.GetType.FullName)
        End Function
    End Class
End Namespace