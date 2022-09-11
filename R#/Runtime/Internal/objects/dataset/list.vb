#Region "Microsoft.VisualBasic::dccdab7a5d111fa7fe05fdaad68d003f, R-sharp\R#\Runtime\Internal\objects\dataset\list.vb"

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

    '   Total Lines: 376
    '    Code Lines: 257
    ' Comment Lines: 59
    '   Blank Lines: 60
    '     File Size: 13.54 KB


    '     Class list
    ' 
    '         Properties: data, length, slots
    ' 
    '         Constructor: (+6 Overloads) Sub New
    ' 
    '         Function: AsGeneric, checkTuple, ctypeInternal, (+2 Overloads) getByIndex, (+2 Overloads) getByName
    '                   getNames, GetSlots, (+2 Overloads) getValue, hasName, namedValues
    '                   setByindex, setByIndex, (+2 Overloads) setByName, setNames, ToString
    ' 
    '         Sub: add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.CType

Namespace Runtime.Internal.Object

    Public Class list : Inherits RsharpDataObject
        Implements RNames, RIndex, RNameIndex, ITupleConstructor

        Public Property slots As Dictionary(Of String, Object)

        Public ReadOnly Property length As Integer Implements RIndex.length
            Get
                Return slots.Count
            End Get
        End Property

        ''' <summary>
        ''' get all values collection from the ``slots`` symbol.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property data As IEnumerable(Of Object)
            Get
                If slots.IsNullOrEmpty Then
                    Return {}
                Else
                    Return slots.Values
                End If
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

        Sub New()
        End Sub

        ''' <summary>
        ''' data clone
        ''' </summary>
        ''' <param name="list"></param>
        Sub New(list As list)
            slots = New Dictionary(Of String, Object)(list.slots)
            elementType = list.elementType
        End Sub

        Sub New(list As Dictionary(Of String, Object))
            _slots = list
        End Sub

        Sub New(table As IDictionary)
            _slots = New Dictionary(Of String, Object)

            For Each itemKey In table.Keys
                _slots(itemKey.ToString) = table(itemKey)
            Next
        End Sub

        Sub New(type As Type)
            Call Me.New(RType.GetRSharpType(type))
        End Sub

        Sub New(type As RType)
            elementType = type
            slots = New Dictionary(Of String, Object)
        End Sub

        Public Sub add(name As String, value As Object)
            If slots Is Nothing Then
                slots = New Dictionary(Of String, Object)
            End If

            Call slots.Add(name, value)
        End Sub

        Public Function hasName(name As String) As Boolean Implements RNames.hasName
            Return slots.ContainsKey(name)
        End Function

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

        'Public Sub Add(name As String, value As Object)
        '    _slots(name) = value
        'End Sub

        ''' <summary>
        ''' 出错会返回错误消息
        ''' </summary>
        ''' <param name="names"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
            Dim oldNames = slots.Keys.ToArray
            Dim newSlots As Dictionary(Of String, Object)

            If names.IsNullOrEmpty Then
                ' delete the source names
                names = oldNames _
                    .Select(Function(null, i) $"[[{i + 1}]]") _
                    .ToArray
            ElseIf oldNames.Length <> names.Length Then
                Return Internal.debug.stop({
                    $"Inconsist name list length!",
                    $"list size: {oldNames.Length}",
                    $"set names: {names.Length}"
                }, envir)
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

        ''' <summary>
        ''' get value or default
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="synonym"></param>
        ''' <param name="env"></param>
        ''' <param name="default">the default value.</param>
        ''' <returns></returns>
        Public Function getValue(Of T)(synonym As String(), env As Environment, Optional [default] As T = Nothing, Optional ByRef err As Message = Nothing) As T
            Dim value As Object = Nothing
            Dim hasHit As Boolean = False

            For Each name As String In synonym
                If slots.ContainsKey(name) Then
                    value = slots(name)
                    hasHit = True
                    Exit For
                End If
            Next

            If Not hasHit Then
                Return [default]
            ElseIf TypeOf value Is Message Then
                err = value
                Return Nothing
            Else
                Return ctypeInternal(Of T)(value, env, err)
            End If
        End Function

        Private Shared Function ctypeInternal(Of T)(value As Object, env As Environment, ByRef err As Message) As T
            Dim type As Type = GetType(T)

            If Not value Is Nothing AndAlso value.GetType Is GetType(T) Then
                Return value
            ElseIf type.IsArray Then
                value = CObj(asVector(value, type.GetElementType, env))
            Else
                value = RCType.CTypeDynamic([single](value), GetType(T), env)
            End If

            If TypeOf value Is Message Then
                err = value
                Return Nothing
            Else
                Return value
            End If
        End Function

        ''' <summary>
        ''' get value or default
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="name"></param>
        ''' <param name="env"></param>
        ''' <param name="default">the default value.</param>
        ''' <returns></returns>
        Public Function getValue(Of T)(name As String, env As Environment, Optional [default] As T = Nothing, Optional ByRef err As Message = Nothing) As T
            If Not slots.ContainsKey(name) Then
                Return [default]
            ElseIf TypeOf slots(name) Is Message Then
                err = slots(name)
                Return Nothing
            Else
                Return ctypeInternal(Of T)(slots(name), env, err)
            End If
        End Function

        Public Function AsGeneric(Of T)(env As Environment,
                                        Optional [default] As T = Nothing,
                                        Optional ByRef err As Message = Nothing) As Dictionary(Of String, T)

            Try
                Dim generic As New Dictionary(Of String, T)

                For Each key As String In slots.Keys
                    Dim value As T = getValue(Of T)(key, env, [default], err)

                    If value Is Nothing AndAlso Not err Is Nothing Then
                        Return Nothing
                    Else
                        Call generic.Add(key, value)
                    End If
                Next

                Return generic
            Catch ex As Exception
                err = Internal.debug.stop(ex, env)
                Return Nothing
            End Try
        End Function

        Public Overrides Function ToString() As String
            Return getNames.GetJson
        End Function

        Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex, ITupleConstructor.getByIndex
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

        Public Function getByIndex(i() As Integer) As Array Implements RIndex.getByIndex
            ' 20200330 list对象只能够返回object数组
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
        Public Function getByName(name As String) As Object Implements RNameIndex.getByName, ITupleConstructor.getByName
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

        ''' <summary>
        ''' null <paramref name="value"/> will remove the target slot by <paramref name="name"/>
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            If value Is Nothing Then
                slots.Remove(name)
            Else
                slots(name) = value
            End If

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
                    Return Internal.debug.stop({
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
            ElseIf type Is GetType(IDictionary(Of String, Object)) Then
                Return any
            Else
                Return Nothing
            End If
        End Function

        Public Shared Widening Operator CType(data As Dictionary(Of String, Object)) As list
            Return New list With {.slots = data}
        End Operator

        Public Function checkTuple(names() As String) As Boolean Implements ITupleConstructor.checkTuple
            Return names.All(AddressOf hasName)
        End Function
    End Class
End Namespace
