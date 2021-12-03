Namespace Struct.LinkedList

    Public Enum ListNodeType
        ''' <summary>
        ''' contains no data, end of the linked list
        ''' </summary>
        NA
        ''' <summary>
        ''' contains vector data, end of the linked list.
        ''' (当前节点为最末尾的叶节点)
        ''' </summary>
        Vector
        ''' <summary>
        ''' 当前节点为链表的中间连接节点
        ''' </summary>
        LinkedList
    End Enum
End Namespace