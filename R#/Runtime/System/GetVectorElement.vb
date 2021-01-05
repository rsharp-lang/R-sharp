#Region "Microsoft.VisualBasic::2ca152ebcd1a9185bd2679b2bdce0b00, R#\Runtime\System\GetVectorElement.vb"

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

'     Class GetVectorElement
' 
'         Properties: isNullOrEmpty
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Getter
' 
' 
' /********************************************************************************/

#End Region


Imports System.Runtime.CompilerServices

Namespace Runtime.Components

    Public Class GetVectorElement

        ReadOnly [single] As Object
        ReadOnly vector As Array
        ReadOnly m_get As Func(Of Integer, Object)

        Public ReadOnly Property isNullOrEmpty As Boolean
            Get
                Return vector Is Nothing OrElse vector.Length = 0
            End Get
        End Property

        Default Public ReadOnly Property item(i As Integer) As Object
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_get(i)
            End Get
        End Property

        Sub New(vec As Array)
            Me.vector = vec

            If vec Is Nothing Then
                [single] = Nothing
            ElseIf vec.Length = 0 Then
                [single] = Nothing
            Else
                [single] = vec.GetValue(Scan0)
            End If

            m_get = Getter()
        End Sub

        Public Function Getter() As Func(Of Integer, Object)
            If isNullOrEmpty OrElse vector.Length = 1 Then
                Return Function() [single]
            Else
                ' R对向量的访问是可以下标越界的
                Return Function(i)
                           If i >= vector.Length Then
                               Return Nothing
                           Else
                               Return vector.GetValue(i)
                           End If
                       End Function
            End If
        End Function
    End Class
End Namespace
