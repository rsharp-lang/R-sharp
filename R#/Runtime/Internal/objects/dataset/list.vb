﻿#Region "Microsoft.VisualBasic::337be90c9ace8aa036f3545378b1476a, R#\Runtime\Internal\objects\dataset\list.vb"

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

'   Total Lines: 692
'    Code Lines: 374 (54.05%)
' Comment Lines: 229 (33.09%)
'    - Xml Docs: 91.27%
' 
'   Blank Lines: 89 (12.86%)
'     File Size: 25.67 KB


'     Class list
' 
'         Properties: data, is_empty, length, slots
' 
'         Constructor: (+10 Overloads) Sub New
' 
'         Function: AsGeneric, checkTuple, ctypeInternal, (+2 Overloads) empty, (+2 Overloads) getByIndex
'                   (+2 Overloads) getByName, getBySynonyms, getNames, GetSlots, (+2 Overloads) getValue
'                   GetVector, hasName, hasNames, listOf, namedValues
'                   set_empty, setByindex, setByIndex, (+2 Overloads) setByName, setNames
'                   slotKeys, subset, ToString
' 
'         Sub: (+3 Overloads) add, unique_add
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.CType

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' A tuple paired list object model
    ''' </summary>
    Public Class list : Inherits RsharpDataObject
        Implements RNames, RIndex, RNameIndex, ITupleConstructor

        Public Property slots As Dictionary(Of String, Object)

        ''' <summary>
        ''' gets the <see cref="slots"/> collection size
        ''' </summary>
        ''' <returns></returns>
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

        Public ReadOnly Property is_empty As Boolean
            Get
                Return slots.IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' Get slot value by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' returns nothing if there is no given 
        ''' <paramref name="name"/> exists in the
        ''' slots
        ''' </returns>
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
            If list Is Nothing OrElse list.slots Is Nothing Then
                Call EmptyListWarningMessage.Warning

                ' create empty list
                slots = New Dictionary(Of String, Object)
            Else
                slots = New Dictionary(Of String, Object)(list.slots)
            End If

            elementType = list.elementType
        End Sub

        Sub New(list As Dictionary(Of String, Object))
            _slots = If(list, New Dictionary(Of String, Object))

            If list Is Nothing Then
                Call EmptyListWarningMessage.Warning
            End If
        End Sub

        Public Const EmptyListWarningMessage As String = "the source tuple list value for make value copy is nothing!"

        Sub New(list As IDictionary(Of String, Object))
            If list Is Nothing Then
                Call EmptyListWarningMessage.Warning

                ' create empty list
                _slots = New Dictionary(Of String, Object)
            Else
                _slots = New Dictionary(Of String, Object)(list)
            End If
        End Sub

        Sub New(table As IDictionary)
            _slots = New Dictionary(Of String, Object)

            If table IsNot Nothing Then
                For Each itemKey In table.Keys
                    _slots(itemKey.ToString) = table(itemKey)
                Next
            Else
                Call EmptyListWarningMessage.Warning
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="type">
        ''' <see cref="elementType"/>
        ''' </param>
        Sub New(type As Type)
            Call Me.New(RType.GetRSharpType(type))
        End Sub

        Sub New(type As TypeCodes)
            Call Me.New(RType.GetType(type))
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="type">
        ''' <see cref="elementType"/>
        ''' </param>
        Sub New(type As RType)
            elementType = type
            slots = New Dictionary(Of String, Object)
        End Sub

        ''' <summary>
        ''' construct a new tuple list object from a named slot value collection.
        ''' </summary>
        ''' <param name="data">a named slot value collection.</param>
        Sub New(ParamArray data As ArgumentReference())
            elementType = RType.any
            slots = New Dictionary(Of String, Object)

            For Each slot As ArgumentReference In data
                slots(slot.Name) = slot.Value
            Next
        End Sub

        Sub New(ParamArray data As (key As String, val As Object)())
            elementType = RType.any
            slots = New Dictionary(Of String, Object)

            For Each item As (key As String, val As Object) In data
                slots(item.key) = item.val
            Next
        End Sub

        ''' <summary>
        ''' add key-value directly 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <remarks>
        ''' this function has different behaviours when compares with
        ''' the <see cref="setByName(String, Object, Environment)"/>:
        ''' 
        ''' 1. this function just add slot value directly even if the given value is nothing
        ''' 2. ``setByName`` function will removes slot if the value is nothing
        ''' </remarks>
        Public Sub add(name As String, value As Object)
            If slots Is Nothing Then
                slots = New Dictionary(Of String, Object)
            End If

            Call slots.Add(name, value)
        End Sub

        ''' <summary>
        ''' add unique key-value safely
        ''' </summary>
        ''' <param name="name">duplicated name will be appends a suffix string for avoid the key conflicts</param>
        ''' <param name="value"></param>
        ''' <remarks>
        ''' the duplicated name will be resolved safely in this function.
        ''' </remarks>
        Public Sub unique_add(name As String, value As Object)
            If slots Is Nothing Then
                slots = New Dictionary(Of String, Object)
            End If
            If slots.ContainsKey(name) Then
                For i As Integer = 1 To Integer.MaxValue
                    Dim key As String = $"{name}_{i}"

                    If Not slots.ContainsKey(key) Then
                        name = key
                    End If
                Next
            End If

            Call slots.Add(name, value)
        End Sub

        ''' <summary>
        ''' add a string value
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="str"></param>
        ''' <remarks>
        ''' due to the reason of string is kind of a char collection
        ''' so that the overlaod of the generic collection add method
        ''' may caused the incorrect method reference.
        ''' 
        ''' create this function for deal with the string value overloads bug.
        ''' </remarks>
        Public Sub add(name As String, str As String)
            Call add(name, value:=CObj(str))
        End Sub

        ''' <summary>
        ''' cast the given collection as array and then add into current tuple list
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="name"></param>
        ''' <param name="collection"></param>
        Public Sub add(Of T)(name As String, collection As IEnumerable(Of T))
            If collection Is Nothing Then
                Call add(name, Nothing)
            ElseIf collection.GetType.IsArray Then
                Call add(name, CObj(collection))
            Else
                Call add(name, CObj(collection.ToArray))
            End If
        End Sub

        ''' <summary>
        ''' does <see cref="slots"/> has the specific <paramref name="name"/>?
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' check for the dictionary key is existsed?
        ''' </remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasName(name As String) As Boolean Implements RNames.hasName
            Return slots.ContainsKey(name)
        End Function

        ''' <summary>
        ''' check that all name inside the given <paramref name="names"/> is
        ''' exists in current data list.
        ''' </summary>
        ''' <param name="names"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasNames(ParamArray names As String()) As Boolean
            Return names.All(AddressOf hasName)
        End Function

        ''' <summary>
        ''' get names with uncheck default index
        ''' </summary>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function slotKeys() As String()
            Return _slots.Keys.ToArray
        End Function

        ''' <summary>
        ''' all value element inside current tuple list is the given element type
        ''' </summary>
        ''' <param name="elementMode">the element type for test</param>
        ''' <returns></returns>
        Public Function listOf(elementMode As TypeCodes) As Boolean
            Return data.All(Function(a) a Is Nothing OrElse RType.TypeOf(a).mode = elementMode)
        End Function

        Const BlankIntegerIndex As String = "\[\[\d+\]\]"

        ''' <summary>
        ''' this function may returns nothing if all index names are default index, example as: [[1]]
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' for get key names safely(avoid null reference) in the clr code, 
        ''' expression like ``slots.Keys.ToArray`` could be used. analso the
        ''' function <see cref="slotKeys()"/> works for this situation.
        ''' </remarks>
        Public Function getNames() As String() Implements RNames.getNames
            Dim names As String() = slots.Keys.ToArray

            If length = 0 Then
                ' an empty string collection
                Return If(names, New String() {})
            End If

            ' [[1]]
            If names.All(Function(s) s.IsPattern(BlankIntegerIndex)) Then
                Return Nothing
            Else
                Return names _
                    .Select(Function(s)
                                Return If(s.IsPattern(BlankIntegerIndex), "", s)
                            End Function) _
                    .ToArray
            End If
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
        ''' <remarks>
        ''' this method will evaluate the value <see cref="Expression"/> 
        ''' to target <typeparamref name="T"/> automatically.
        ''' </remarks>
        Public Function getValue(Of T)(synonym As String(), env As Environment,
                                       Optional [default] As T = Nothing,
                                       Optional ByRef err As Message = Nothing) As T

            Dim value As Object = Nothing
            Dim hasHit As Boolean = False

            For Each name As String In synonym
                If slots.ContainsKey(name) Then
                    value = slots(name)
                    hasHit = True
                    Exit For
                End If
            Next

            If (Not hasHit) OrElse (value Is Nothing) Then
                Return [default]
            ElseIf TypeOf value Is Message Then
                err = value
                Return Nothing
            Else
                Return ctypeInternal(Of T)(value, env, err)
            End If
        End Function

        ''' <summary>
        ''' cast data type
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="value">
        ''' if value is an expression andalso <typeparamref name="T"/> 
        ''' type is not an expression, then this function will try to 
        ''' evaluate the expression as value and then cast data type 
        ''' again.
        ''' </param>
        ''' <param name="env"></param>
        ''' <param name="err"></param>
        ''' <returns></returns>
        Private Shared Function ctypeInternal(Of T)(value As Object, env As Environment, ByRef err As Message) As T
            Dim type As Type = GetType(T)

            If TypeOf value Is Expression AndAlso Not GetType(T).IsInheritsFrom(GetType(Expression)) Then
                value = DirectCast(value, Expression).Evaluate(env)
            End If

            If Not value Is Nothing AndAlso
                (value.GetType Is GetType(T) OrElse
                value.GetType.IsInheritsFrom(GetType(T), strict:=False)) Then

                ' is current type or is the subtype of the base type T
                ' then we can returns the object directly
                Return value
            ElseIf type.IsArray Then
                value = CObj(asVector(value, type.GetElementType, env))
            Else
                ' 20221008
                ' is not a array
                ' so set the force single parameter as TRUE
                value = RCType.CTypeDynamic([single](value, forceSingle:=True), GetType(T), env)
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
        ''' <remarks>
        ''' this method will evaluate the value <see cref="Expression"/> 
        ''' to target <typeparamref name="T"/> automatically.
        ''' </remarks>
        Public Function getValue(Of T)(name As String, env As Environment,
                                       Optional [default] As T = Nothing,
                                       Optional ByRef err As Message = Nothing) As T

            If Not slots.ContainsKey(name) Then
                Return [default]
            ElseIf TypeOf slots(name) Is Message Then
                err = slots(name)
                Return Nothing
            Else
                Return ctypeInternal(Of T)(slots(name), env, err)
            End If
        End Function

        ''' <summary>
        ''' Try cast of the object type value to a generic type value
        ''' </summary>
        ''' <typeparam name="T">the generic type that cast to</typeparam>
        ''' <param name="env"></param>
        ''' <param name="default">the default element generic value if the type cast failure.</param>
        ''' <param name="err">gets the error message for the type cast failure from this parameter.</param>
        ''' <returns></returns>
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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="i">1-based index</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' the given index value <paramref name="i"/> should be a integer value from 1-based.
        ''' </remarks>
        Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex, ITupleConstructor.getByIndex
            If i > length Then
                Return Nothing
            Else
                ' R# vector index start from 1
                i -= 1
            End If

            Dim names As ICollection(Of String) = _slots.Keys
            Dim key As String = names(i)

            Return _slots(key)
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
        ''' get raw value by a set of given synonyms.
        ''' </summary>
        ''' <param name="synonym"></param>
        ''' <returns></returns>
        Public Function getBySynonyms(ParamArray synonym As String()) As Object
            For Each name As String In synonym
                If slots.ContainsKey(name) Then
                    Return slots(name)
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' Create a list subset
        ''' </summary>
        ''' <param name="names">
        ''' A specific list keys that will be used for extract the subset 
        ''' elements from the original list to create a new list object.
        ''' </param>
        ''' <returns></returns>
        Public Function getByName(names() As String) As Object Implements RNameIndex.getByName
            Dim subset As Dictionary(Of String, Object) = names _
                .ToDictionary(Function(key) key,
                              Function(key)
                                  Return getByName(key)
                              End Function)

            Return New list With {.slots = subset}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetVector(names As String()) As Array
            Return names _
                .SafeQuery _
                .Select(Function(key) slots.TryGetValue(key)) _
                .ToArray
        End Function

        ''' <summary>
        ''' add/updates of the key <paramref name="name"/> associated <paramref name="value"/> data.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' NULL <paramref name="value"/> will remove the target slot by <paramref name="name"/>
        ''' </remarks>
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

            If value Is Nothing AndAlso names.Length = 1 Then
                getValue = Function(i) Nothing
            ElseIf value.Length = 1 Then
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
                message = setByName(names(index), getValue(index), envir)

                If Program.isException(message) Then
                    Return message
                Else
                    result.Add(message)
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

        Public Shared Function empty() As list
            Return New list With {.slots = New Dictionary(Of String, Object)}
        End Function

        ''' <summary>
        ''' check of the given tuple list object is null or empty?
        ''' </summary>
        ''' <param name="list"></param>
        ''' <returns></returns>
        Public Shared Function empty(list As list) As Boolean
            Return list Is Nothing OrElse list.length = 0
        End Function

        ''' <summary>
        ''' create a list with given slot names, but the 
        ''' corresponding slot value is nothing
        ''' </summary>
        ''' <param name="slot_names"></param>
        ''' <returns></returns>
        Public Shared Function set_empty(slot_names As IEnumerable(Of String)) As list
            Dim empty As list = list.empty

            For Each name As String In slot_names
                empty.add(name, Nothing)
            Next

            Return empty
        End Function

        ''' <summary>
        ''' A shortcut of function <see cref="getByName(String())"/>
        ''' </summary>
        ''' <param name="keys"></param>
        ''' <returns></returns>
        Public Function subset(keys() As String) As list
            Return getByName(keys)
        End Function
    End Class
End Namespace
