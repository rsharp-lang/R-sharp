#Region "Microsoft.VisualBasic::3e604a3ca6a687ef0d4a7c8663afaf91, R#\Runtime\Internal\objects\list.vb"

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

    '     Class list
    ' 
    '         Properties: length, slots
    ' 
    '         Function: (+2 Overloads) getByIndex, (+2 Overloads) getByName, getNames, GetSlots, namedValues
    '                   setByindex, setByIndex, (+2 Overloads) setByName, setNames, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal.Object

    Public Class list : Implements RNames, RIndex, RNameIndex

        Public Property slots As Dictionary(Of String, Object)

        Public ReadOnly Property length As Integer Implements RIndex.length
            Get
                Return slots.Count
            End Get
        End Property

        ''' <summary>
        ''' Get slot value by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property SlotValue(name As String) As Object
            Get
                Return getByName(name)
            End Get
        End Property

        Public Function getNames() As String() Implements RNames.getNames
            Return slots.Keys.ToArray
        End Function

        Public Iterator Function namedValues() As IEnumerable(Of NamedValue(Of Object))
            For Each key As String In slots.Keys
                Yield New NamedValue(Of Object) With {
                    .Name = key,
                    .Value = slots(key)
                }
            Next
        End Function

        Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
            Dim oldNames = slots.Keys.ToArray
            Dim newSlots As Dictionary(Of String, Object)

            If names.IsNullOrEmpty Then
                ' delete the source names
                names = oldNames _
                    .Select(Function(null, i) $"[[{i + 1}]]") _
                    .ToArray
            ElseIf oldNames.Length <> names.Length Then
                Return Internal.stop("Inconsist name list length!", envir)
            End If

            newSlots = oldNames _
                .SeqIterator _
                .ToDictionary(Function(i) names(i),
                              Function(index)
                                  Return slots(oldNames(index))
                              End Function)
            slots = newSlots

            Return names
        End Function

        Public Overrides Function ToString() As String
            Return getNames.GetJson
        End Function

        Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
            If i > length Then
                Return Nothing
            Else
                ' R# vector index start from 1
                i -= 1
            End If

            Dim names = getNames()
            Dim key As String = names(i)

            Return slots(key)
        End Function

        Public Function getByIndex(i() As Integer) As Object() Implements RIndex.getByIndex
            Return i.Select(AddressOf getByIndex).ToArray
        End Function

        Public Function setByIndex(i As Integer, value As Object, envir As Environment) As Object Implements RIndex.setByIndex
            Throw New NotImplementedException()
        End Function

        Public Function setByindex(i() As Integer, value As Array, envir As Environment) As Object Implements RIndex.setByindex
            Throw New NotImplementedException()
        End Function

        ''' <summary>
        ''' If target key name is not found in list, then
        ''' function will returns nothing
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function getByName(name As String) As Object Implements RNameIndex.getByName
            If slots.ContainsKey(name) Then
                Return slots(name)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Create a list subset
        ''' </summary>
        ''' <param name="names"></param>
        ''' <returns></returns>
        Public Function getByName(names() As String) As Object Implements RNameIndex.getByName
            Dim subset As Dictionary(Of String, Object) = names _
                .ToDictionary(Function(key) key,
                              Function(key)
                                  Return getByName(key)
                              End Function)

            Return New list With {.slots = subset}
        End Function

        Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            slots(name) = value
            Return value
        End Function

        Public Function setByName(names() As String, value As Array, envir As Environment) As Object Implements RNameIndex.setByName
            Dim getValue As Func(Of Integer, Object)

            If value.Length = 1 Then
                Dim val As Object = value.GetValue(Scan0)

                getValue = Function(i)
                               Return val
                           End Function
            Else
                If names.Length <> value.Length Then
                    Return Internal.stop({
                        $"Number of items to replace is not equals to replacement length!",
                        $"length(names): {names.Length}",
                        $"length(value): {value.Length}"
                    }, envir)
                End If

                getValue = Function(i)
                               Return value.GetValue(i)
                           End Function
            End If

            Dim result As New List(Of Object)
            Dim message As Object

            For index As Integer = 0 To names.Length - 1
                message = setByIndex(names(index), getValue(index), envir)

                If Not message Is Nothing AndAlso message.GetType Is GetType(Runtime.Components.Message) Then
                    Return message
                Else
                    result += message
                End If
            Next

            Return result.ToArray
        End Function

        Public Shared Function GetSlots(any As Object) As Dictionary(Of String, Object)
            If any Is Nothing Then
                Return Nothing
            End If

            Dim type As Type = any.GetType

            If type Is GetType(list) Then
                Return DirectCast(any, list).slots
            ElseIf type Is GetType(Dictionary(Of String, Object)) Then
                Return any
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace
