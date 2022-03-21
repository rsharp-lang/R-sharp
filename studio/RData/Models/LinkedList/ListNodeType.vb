#Region "Microsoft.VisualBasic::e32d62a49dfbc9a3bdebb0a86551d02c, R-sharp\studio\RData\Models\LinkedList\ListNodeType.vb"

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

    '   Total Lines: 18
    '    Code Lines: 7
    ' Comment Lines: 10
    '   Blank Lines: 1
    '     File Size: 498.00 B


    '     Enum ListNodeType
    ' 
    '         LinkedList, NA, Vector
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
