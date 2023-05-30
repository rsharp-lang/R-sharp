#Region "Microsoft.VisualBasic::0e3d7d0b2937ced003f9b2c1e5e84001, F:/GCModeller/src/R-sharp/R#//Runtime/Internal/objects/dataset/vector.vb"

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

'   Total Lines: 332
'    Code Lines: 228
' Comment Lines: 50
'   Blank Lines: 54
'     File Size: 12.31 KB


'     Class vector
' 
'         Properties: data, factor, length, unit
' 
'         Constructor: (+7 Overloads) Sub New
'         Function: asVector, fromScalar, (+2 Overloads) getByIndex, getByName, getNames
'                   hasName, isVectorOf, setByindex, setByIndex, setNames
'                   ToString
'         Operators: <>, =
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Object

    Public Class vector : Inherits RsharpDataObject
        Implements RNames, RIndex

        Public Property data As Array
        ''' <summary>
        ''' do conversion from current vector to another scale.
        ''' </summary>
        ''' <returns></returns>
        Public Property unit As unit
        Public Property factor As factor

        Public ReadOnly Property length As Integer Implements RIndex.length
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            <DebuggerStepThrough>
            Get
                Return data.Length
            End Get
        End Property

        Dim names As String()
        Dim nameIndex As Index(Of String)

        Sub New()
        End Sub

        ''' <summary>
        ''' value copy
        ''' </summary>
        ''' <param name="vec"></param>
        Sub New(vec As vector)
            data = Array.CreateInstance(vec.data.GetType.GetElementType, vec.length)
            Array.ConstrainedCopy(vec.data, Scan0, data, Scan0, length)
            factor = vec.factor
            unit = vec.unit
            elementType = vec.elementType
            names = vec.names
            nameIndex = vec.nameIndex
        End Sub

        ''' <summary>
        ''' 这个构造函数主要是应用于内部编程的快速创建
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="type">
        ''' should be the array element type
        ''' </param>
        Sub New(input As Array, type As RType)
            data = input
            elementType = type
        End Sub

        Friend Sub New(names As String(), input As Array, type As RType, unit As unit)
            Call Me.New(input, type)

            Me.unit = unit
            Me.names = names
            Me.nameIndex = names.Indexing
        End Sub

        Sub New(names As String(), input As Array, type As RType, env As Environment)
            Call Me.New(input, type)
            Call Me.setNames(names, env)
        End Sub

        ''' <summary>
        ''' Create a vector from a pipeline model and given array element <paramref name="model"/> type
        ''' </summary>
        ''' <param name="model">element type of the array</param>
        ''' <param name="input"></param>
        ''' <remarks>
        ''' try to make the collection data generic in this constructor function
        ''' </remarks>
        Sub New(model As Type, input As IEnumerable, env As Environment)
            If model Is GetType(Void) Then
                model = GetType(Object)
            End If

            elementType = RType.GetRSharpType(model)

            If input.GetType.IsArray Then
                If input.GetType.GetElementType Is model Then
                    data = CObj(input)
                Else
                    data = loadGenericCollection(input, model, env)
                End If
            Else
                data = loadGenericCollection(input, model, env)
            End If
        End Sub

        Private Shared Function loadGenericCollection(input As IEnumerable, model As Type, env As Environment) As Array
            Dim list As IList = Activator.CreateInstance(GetType(List(Of )).MakeGenericType(model))
            Dim isObjWrapper As Boolean = model Is GetType(vbObject)
            Dim isNumeric As Boolean = DataFramework.IsNumericType(model)
            Dim is_interface As Boolean = model.IsInterface

            For Each obj As Object In input
                If Not isObjWrapper AndAlso TypeOf obj Is vbObject Then
                    obj = DirectCast(obj, vbObject).target
                End If

                If Not obj Is Nothing Then
                    Dim objType As Type = obj.GetType

                    If objType Is model Then
                        ' do nothing
                    ElseIf (is_interface AndAlso objType.ImplementInterface(model)) OrElse objType.IsInheritsFrom(model) Then
                        obj = Conversion.CTypeDynamic(obj, model)
                    ElseIf isNumeric AndAlso TypeOf obj Is String Then
                        obj = RCType.CTypeDynamic(CStr(obj).ParseNumeric, model, env)
                    Else
                        obj = RCType.CTypeDynamic(obj, model, env)
                    End If
                End If

                Call list.Add(obj)
            Next

            Dim buffer As Array = Array.CreateInstance(model, length:=list.Count)

            For i As Integer = 0 To buffer.Length - 1
                Call buffer.SetValue(list.Item(i), i)
            Next

            Return buffer
        End Function

        Sub New(names As String(), data As Array, envir As Environment)
            If data.AsObjectEnumerator _
                   .All(Function(a)
                            Return Not a Is Nothing AndAlso a.GetType.IsArray AndAlso DirectCast(a, Array).Length = 1
                        End Function) Then

                data = data _
                    .AsObjectEnumerator _
                    .Select(Function(a) DirectCast(a, Array).GetValue(Scan0)) _
                    .ToArray
            End If

            Me.data = data
            Me.setNames(names, envir)
            Me.elementType = Runtime _
                .MeasureArrayElementType(data) _
                .DoCall(AddressOf RType.GetRSharpType)
        End Sub

        Sub New(names As String(), data As Double())
            Me.data = data
            Me.setNames(names)
            Me.elementType = RType.GetRSharpType(GetType(Double))
        End Sub

        Sub New(names As String(), data As Single())
            Me.data = data
            Me.setNames(names)
            Me.elementType = RType.GetRSharpType(GetType(Single))
        End Sub

        Sub New(names As String(), data As Integer())
            Me.data = data
            Me.setNames(names)
            Me.elementType = RType.GetRSharpType(GetType(Integer))
        End Sub

        Sub New(names As String(), data As Long())
            Me.data = data
            Me.setNames(names)
            Me.elementType = RType.GetRSharpType(GetType(Long))
        End Sub

        Sub New(names As String(), data As Boolean())
            Me.data = data
            Me.setNames(names)
            Me.elementType = RType.GetRSharpType(GetType(Boolean))
        End Sub

        Sub New(names As String(), data As String())
            Me.data = data
            Me.setNames(names)
            Me.elementType = RType.GetRSharpType(GetType(String))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getByName(name As String) As Object
            If Not name Like nameIndex Then
                Return Nothing
            Else
                Return getByIndex(nameIndex.IndexOf(name) + 1)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasName(name As String) As Boolean Implements RNames.hasName
            Return name Like nameIndex
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function getNames() As String() Implements RNames.getNames
            Return names
        End Function

        Public Sub setNames(names As String())
            If Not names.IsNullOrEmpty Then
                If names.Length <> data.Length Then
                    Throw New InvalidProgramException(sizeMisMatched)
                Else
                    Me.names = names
                    Me.nameIndex = names.Indexing
                End If
            Else
                Me.names = names
                Me.nameIndex = Nothing
            End If
        End Sub

        Const sizeMisMatched As String = "vector names is not equals in length with the vector element data!"

        ''' <summary>
        ''' 出错的时候会返回<see cref="Message"/>
        ''' </summary>
        ''' <param name="names"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function setNames(names() As String, envir As Environment) As Object Implements RNames.setNames
            If Not names.IsNullOrEmpty Then
                If names.Length <> data.Length Then
                    Return Internal.debug.stop(sizeMisMatched, envir)
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

        ''' <summary>
        ''' 以R语言中的下表的方式访问数组元素，所以这个函数的i参数应该是从1开始的
        ''' </summary>
        ''' <param name="i">
        ''' 这个下标值应该是从1开始的
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个函数是一个安全的函数，下表越界的时候按照R语言的处理规则，不会抛出错误，而是返回空值
        ''' </remarks>
        Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
            If i < 0 Then
                i = data.Length + i
            End If

            If data.Length = 0 OrElse i > data.Length OrElse i <= 0 Then
                Return Nothing
            Else
                ' 下标是从1开始的
                Return data.GetValue(i - 1)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="i">
        ''' 这个下标值应该是从1开始的
        ''' </param>
        ''' <returns></returns>
        Public Function getByIndex(i() As Integer) As Array Implements RIndex.getByIndex
            If elementType Is Nothing OrElse elementType Like GetType(Object) Then
                Return i.Select(AddressOf getByIndex).ToArray
            Else
                Dim vec As Array = Array.CreateInstance(elementType.raw, i.Length)

                For j As Integer = 0 To i.Length - 1
                    vec.SetValue(getByIndex(i(j)), j)
                Next

                Return vec
            End If
        End Function

        Public Function setByIndex(i As Integer, value As Object, envir As Environment) As Object Implements RIndex.setByIndex
            If i < 0 Then
                i = data.Length + i
            End If

            If i < 0 Then
                Return Internal.debug.stop($"Invalid element index value '{i}'!", envir)
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
                Dim readonlyVal As Object = value.GetValue(Scan0)

                getValue = Function(j%) As Object
                               Return readonlyVal
                           End Function
            Else
                If i.Length <> value.Length Then
                    Return Internal.debug.stop({
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{length}] vec<{m_type.ToString}>"
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function asVector(Of T)(x As IEnumerable(Of T), Optional unit As unit = Nothing) As vector
            Return New vector With {
                .data = x.ToArray,
                .elementType = RType.GetRSharpType(GetType(T)),
                .unit = unit
            }
        End Function

        Public Shared Function fromScalar(x As Object) As vector
            Dim array As Array = Array.CreateInstance(x.GetType, 1)
            Call array.SetValue(x, Scan0)
            Return New vector With {
                .data = array,
                .elementType = RType.GetRSharpType(x.GetType)
            }
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function isVectorOf(x As Object, type As TypeCodes) As Boolean
            Return TypeOf x Is vector AndAlso RType.TypeOf(x) Like RType.GetType(type)
        End Function

        Public Overloads Shared Operator =(v As vector, value As String) As vector
            Dim str As String() = CLRVector.asCharacter(v.data)
            Dim bools As Boolean() = New Boolean(str.Length - 1) {}

            For i As Integer = 0 To bools.Length - 1
                bools(i) = str(i) = value
            Next

            Return New vector With {
                .data = bools,
                .elementType = RType.GetRSharpType(GetType(Boolean))
            }
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Operator <>(v As vector, value As String) As vector
            Return vector.asVector(Of Boolean)(Operators.UnaryNot.Not(v = value))
        End Operator
    End Class
End Namespace
