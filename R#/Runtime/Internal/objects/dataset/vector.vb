#Region "Microsoft.VisualBasic::bc0f696dbfc7924f0aab21081c214e4f, R-sharp\R#\Runtime\Internal\objects\dataset\vector.vb"

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


     Code Statistics:

        Total Lines:   309
        Code Lines:    212
        Comment Lines: 45
        Blank Lines:   52
        File Size:     11.37 KB


    '     Class vector
    ' 
    '         Properties: data, factor, length, unit
    ' 
    '         Constructor: (+7 Overloads) Sub New
    '         Function: asVector, (+2 Overloads) getByIndex, getByName, getNames, hasName
    '                   isVectorOf, setByindex, setByIndex, setNames, ToString
    '         Operators: <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

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
        ''' <param name="type"></param>
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
        Sub New(model As Type, input As IEnumerable, env As Environment)
            Dim i As i32 = Scan0

            If model Is GetType(Void) Then
                model = GetType(Object)
            End If

            ' create an empty vector with 
            ' allocable data buffer
            Dim buffer As Array = Array.CreateInstance(model, BufferSize)
            Dim objType As Type

            For Each obj As Object In input
                If Not obj Is Nothing Then
                    objType = obj.GetType

                    If objType Is model Then
                        ' do nothing
                    ElseIf obj.GetType.IsInheritsFrom(model) Then
                        obj = RCType.CTypeDynamic(obj, model, env)
                    End If
                End If

                If Not model Is GetType(vbObject) AndAlso TypeOf obj Is vbObject Then
                    obj = DirectCast(obj, vbObject).target
                End If

                Call buffer.SetValue(obj, CInt(i))

                If ++i = buffer.Length Then
                    ' resize vector buffer
                    data = buffer
                    buffer = Array.CreateInstance(model, BufferSize + buffer.Length)
                    Array.ConstrainedCopy(data, Scan0, buffer, Scan0, data.Length)
                End If
            Next

            elementType = RType.GetRSharpType(model)
            ' trim the vector to its acutal size
            data = Array.CreateInstance(model, length:=i)
            ' do buffer memory copy
            Array.ConstrainedCopy(buffer, Scan0, data, Scan0, data.Length)
        End Sub

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

        Public Function getByName(name As String) As Object
            If Not name Like nameIndex Then
                Return Nothing
            Else
                Return getByIndex(nameIndex.IndexOf(name) + 1)
            End If
        End Function

        Public Function hasName(name As String) As Boolean Implements RNames.hasName
            Return name Like nameIndex
        End Function

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
                    Return Internal.debug.stop($"vector names is not equals in length with the vector element data!", envir)
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
        Public Function getByIndex(i As Integer) As Object Implements RIndex.getByIndex
            If i < 0 Then
                i = data.Length + i
            End If

            If i > data.Length OrElse i <= 0 Then
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

        Public Overrides Function ToString() As String
            Return $"[{length}] vec<{m_type.ToString}>"
        End Function

        Public Shared Function asVector(Of T)(x As IEnumerable(Of T), Optional unit As unit = Nothing) As vector
            Return New vector With {
                .data = x.ToArray,
                .elementType = RType.GetRSharpType(GetType(T)),
                .unit = unit
            }
        End Function

        Public Shared Function isVectorOf(x As Object, type As TypeCodes) As Boolean
            Return TypeOf x Is vector AndAlso RType.TypeOf(x) Like RType.GetType(type)
        End Function

        Public Overloads Shared Operator =(v As vector, value As String) As vector
            Dim str As String() = REnv.asVector(Of String)(v.data)
            Dim bools As Boolean() = str.Select(Function(s) s = value).ToArray

            Return vector.asVector(Of Boolean)(bools)
        End Operator

        Public Overloads Shared Operator <>(v As vector, value As String) As vector
            Return vector.asVector(Of Boolean)(UnaryNot.Not(v = value))
        End Operator
    End Class
End Namespace
