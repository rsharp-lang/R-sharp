#Region "Microsoft.VisualBasic::193bc16348b11d86bdb81cab0688a120, F:/GCModeller/src/R-sharp/studio/RData//Models/LinkedList/RList.vb"

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

    '   Total Lines: 105
    '    Code Lines: 74
    ' Comment Lines: 19
    '   Blank Lines: 12
    '     File Size: 3.53 KB


    '     Class RList
    ' 
    '         Properties: CAR, CDR, data, env, isPrimitive
    '                     nodeType
    ' 
    '         Function: CreateNode, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Serialization.JSON

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
        Public Property env As EnvironmentValue

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

        ''' <summary>
        ''' data vector is not a collection of <see cref="RObject"/>?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isPrimitive As Boolean
            Get
                If data Is Nothing Then
                    Return False
                ElseIf data.GetType.GetElementType Is Nothing Then
                    Return False
                Else
                    Return Not data.GetType.GetElementType Is GetType(RObject)
                End If
            End Get
        End Property

        Public ReadOnly Property nodeType As ListNodeType
            Get
                If Not env Is Nothing Then
                    Return ListNodeType.Environment
                ElseIf data Is Nothing AndAlso CAR Is Nothing AndAlso CDR Is Nothing Then
                    Return ListNodeType.NA
                ElseIf Not data Is Nothing Then
                    Return ListNodeType.Vector
                Else
                    Return ListNodeType.LinkedList
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            If Not CAR Is Nothing Then
                Return CAR.ToString
            Else
                Return data.GetJson
            End If
        End Function

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
            ElseIf TypeOf value Is String Then
                Return New RList With {
                    .data = DirectCast(value, String).ToArray
                }
            ElseIf TypeOf value Is EnvironmentValue Then
                Dim env As EnvironmentValue = DirectCast(value, EnvironmentValue)

                Return New RList With {
                    .env = env
                }
            End If

            Throw New NotImplementedException(value.GetType.FullName)
        End Function
    End Class
End Namespace
