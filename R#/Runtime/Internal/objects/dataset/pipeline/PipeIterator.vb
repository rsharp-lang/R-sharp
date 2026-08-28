#Region "Microsoft.VisualBasic::a6edcdfb71faae675db9802a15615c0b, R#\Runtime\Internal\objects\dataset\pipeline\PipeIterator.vb"

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

    '   Total Lines: 82
    '    Code Lines: 65 (79.27%)
    ' Comment Lines: 4 (4.88%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (15.85%)
    '     File Size: 2.51 KB


    '     Class PipeIterator
    ' 
    '         Properties: isError, length, scalar
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: getData, GetEnumerator, getError, IEnumerable_GetEnumerator, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' <see cref="IEnumerable(Of T)"/> collection of <typeparamref name="T"/>
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class PipeIterator(Of T) : Implements IEnumerable(Of T)

        Dim data As T()
        Dim err As Message

        Public ReadOnly Property isError As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Not err Is Nothing
            End Get
        End Property

        Public ReadOnly Property length As Integer
            Get
                If isError Then
                    Return 0
                Else
                    Return data.Length
                End If
            End Get
        End Property

        Public ReadOnly Property scalar As T
            Get
                If length > 0 Then
                    Return data(0)
                Else
                    Return Nothing
                End If
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getError() As Message
            Return err
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getData() As T()
            Return data
        End Function

        Sub New(data As IEnumerable(Of T))
            Me.data = data.ToArray
        End Sub

        Sub New(err As Message)
            Me.err = err
        End Sub

        Public Overrides Function ToString() As String
            If isError Then
                Return err.message.JoinBy(". ")
            ElseIf length = 0 Then
                Return "[]"
            ElseIf length = 1 Then
                Return $"scalar({GetType(T).Name} {scalar.ToString})"
            Else
                Return $"[{length}x{GetType(T).Name}] {data.Take(6).JoinBy(", ")}..."
            End If
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of T) Implements IEnumerable(Of T).GetEnumerator
            For Each item As T In data
                Yield item
            Next
        End Function

        Private Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Return GetEnumerator()
        End Function
    End Class
End Namespace
