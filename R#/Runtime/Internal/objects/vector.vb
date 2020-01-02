#Region "Microsoft.VisualBasic::e7b2be643713d382ab3d4c684be30b9a, R#\Runtime\Internal\objects\vector.vb"

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

    '     Class vector
    ' 
    '         Properties: data, length
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: (+2 Overloads) getByIndex, getNames, setByindex, setByIndex, setNames
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal.Object

    Public Class vector : Implements RNames, RIndex

        Public Property data As Array

        Public ReadOnly Property length As Integer Implements RIndex.length
            Get
                Return data.Length
            End Get
        End Property

        Dim names As String()
        Dim nameIndex As Index(Of String)

        Sub New()
        End Sub

        Sub New(names As String(), data As Array, envir As Environment)
            Me.data = data
            Me.setNames(names, envir)
        End Sub

        Public Function getNames() As String() Implements RNames.getNames
            Return names
        End Function

        ''' <summary>
        ''' 出错的时候会返回<see cref="Message"/>
        ''' </summary>
        ''' <param name="names"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
            If Not names.IsNullOrEmpty Then
                If names.Length <> data.Length Then
                    Return Internal.stop($"vector names is not equals in length with the vector element data!", envir)
                Else
                    Me.names = names
                    Me.nameIndex = names.Indexing
                End If
            Else
                Me.names = names
                Me.nameIndex = Nothing
            End If

            Return names
        End Function

        Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
            If i < 0 Then
                i = data.Length + i
            End If

            If i >= data.Length OrElse i < 0 Then
                Return Nothing
            Else
                Return data.GetValue(i)
            End If
        End Function

        Public Function getByIndex(i() As Integer) As Object() Implements RIndex.getByIndex
            Return i.Select(AddressOf getByIndex).ToArray
        End Function

        Public Function setByIndex(i As Integer, value As Object, envir As Environment) As Object Implements RIndex.setByIndex
            If i < 0 Then
                i = data.Length + i
            End If

            If i < 0 Then
                Return Internal.stop($"Invalid element index value '{i}'!", envir)
            End If

            Dim delta = i - data.Length

            If delta <= 0 Then
                data.SetValue(value, i)
            Else
                Dim resize As Array = New Object(i - 1) {}
                Array.ConstrainedCopy(data, Scan0, resize, Scan0, data.Length)
                data = resize
                data.SetValue(value, i - 1)
            End If

            Return value
        End Function

        Public Function setByindex(i() As Integer, value As Array, envir As Environment) As Object Implements RIndex.setByindex
            Dim getValue As Func(Of Integer, Object)

            If value.Length = 1 Then
                Dim val As Object = value.GetValue(Scan0)

                getValue = Function(j%) As Object
                               Return val
                           End Function
            Else
                If i.Length <> value.Length Then
                    Return Internal.stop({
                        $"Number of items to replace is not equals to replacement length!",
                        $"length(index): {i.Length}",
                        $"length(value): {value.Length}"
                    }, envir)
                End If

                getValue = Function(j%) As Object
                               Return value.GetValue(j)
                           End Function
            End If

            Dim result As New List(Of Object)
            Dim message As Object

            For index As Integer = 0 To i.Length - 1
                message = setByIndex(i(index), getValue(index), envir)

                If Not message Is Nothing AndAlso message.GetType Is GetType(Message) Then
                    Return message
                Else
                    result += message
                End If
            Next

            Return result.ToArray
        End Function
    End Class
End Namespace
