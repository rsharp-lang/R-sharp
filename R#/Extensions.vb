#Region "Microsoft.VisualBasic::b9648e84b684ea055eb45527fd749b78, R#\Extensions.vb"

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

    '   Total Lines: 356
    '    Code Lines: 236 (66.29%)
    ' Comment Lines: 78 (21.91%)
    '    - Xml Docs: 87.18%
    ' 
    '   Blank Lines: 42 (11.80%)
    '     File Size: 13.22 KB


    ' Module Extensions
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: AsRReturn, Buffer, CastSequence, EvaluateFramework, evaluateList
    '               GetEncoding, GetObject, GetString, ParseDebugLevel, SafeCreateColumns
    '               toList
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

<HideModuleName>
Public Module Extensions

    ReadOnly debugLevels As Dictionary(Of String, DebugLevels)

    Sub New()
        debugLevels = Enums(Of DebugLevels).ToDictionary(Function(flag) flag.Description.ToLower)
    End Sub

    <Extension>
    Public Function toList(data As dataframe, env As Environment, Optional byrow As Boolean = False) As list
        If byrow Then
            Return data.listByRows(Nothing, env)
        Else
            Return data.listByColumns
        End If
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function ParseDebugLevel(argVal As String) As DebugLevels
        Return debugLevels.TryGetValue(Strings.LCase(argVal), [default]:=Interpreter.DebugLevels.All)
    End Function

    <DebuggerStepThrough>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function AsRReturn(Of T)(x As T) As RReturn
        Return New RReturn(x)
    End Function

    ''' <summary>
    ''' Create a specific .NET object from given data
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="list">the object property data value collection.</param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function GetObject(Of T As {New, Class})(list As list) As T
        Return RListObjectArgumentAttribute.CreateArgumentModel(Of T)(list.slots)
    End Function

    ''' <summary>
    ''' Get text encoding value, returns <see cref="Encoding.Default"/> by default if no matched.
    ''' </summary>
    ''' <param name="val">
    ''' 
    ''' </param>
    ''' <returns></returns>
    ''' <remarks>
    ''' utf8 encoding will be returned if the parameter 
    ''' value is nothing orelse is unknown string value.
    ''' </remarks>
    Public Function GetEncoding(val As Object) As Encoding
        If val Is Nothing OrElse (TypeOf val Is String AndAlso val = "unknown") Then
            Return Encodings.UTF8WithoutBOM.CodePage
        ElseIf TypeOf val Is Encoding Then
            Return val
        ElseIf TypeOf val Is Encodings Then
            Return DirectCast(val, Encodings).CodePage
        ElseIf val.GetType Like RType.characters Then
            Dim encodingName$ = any.ToString(CLRVector.asCharacter(val).SafeQuery.FirstOrDefault, null:="utf8")
            Dim encodingVal As Encoding = TextEncodings _
                .ParseEncodingsName(
                    encoding:=encodingName,
                    onFailure:=Encodings.UTF8
                ) _
                .CodePage

            Return encodingVal
        Else
            Return Encoding.Default
        End If
    End Function

    ''' <summary>
    ''' get target value as string
    ''' </summary>
    ''' <param name="list"></param>
    ''' <param name="key$"></param>
    ''' <param name="default$"></param>
    ''' <returns></returns>
    <Extension>
    Public Function GetString(list As list, key$, Optional default$ = Nothing) As String
        If Not list.hasName(key) Then
            Return [default]
        Else
            Return any.ToString(Runtime.getFirst(list(key)))
        End If
    End Function

    <Extension>
    Public Function SafeCreateColumns(Of T)(data As IEnumerable(Of T),
                                            getKey As Func(Of T, String),
                                            getArray As Func(Of T, String())) As Dictionary(Of String, Array)

        Dim cols As New Dictionary(Of String, Array)
        Dim key As String
        Dim index As Integer = Scan0

        For Each col As T In data
            key = getKey(col)
            index += 1

            If key Is Nothing Then
                key = $"[, {index}]"
            End If

            If cols.ContainsKey(key) Then
                For i As Integer = 0 To 10000
                    Dim newKey = key & "." & i

                    If Not cols.ContainsKey(newKey) Then
                        key = newKey
                        Exit For
                    End If
                Next
            End If

            cols.Add(key, getArray(col))
        Next

        Return cols
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="TOut"></typeparam>
    ''' <param name="x"></param>
    ''' <param name="eval"></param>
    ''' <param name="parallel"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' variant type of <typeparamref name="TOut"/> or a tuple list result
    ''' </returns>
    <Extension>
    Private Function evaluateList(Of T, TOut)(x As list,
                                              eval As Func(Of T, TOut),
                                              parallel As Boolean,
                                              tqdm As Boolean,
                                              env As Environment) As Object

        Dim seq = x.AsGeneric(Of T)(env).AsList
        Dim eval2 = Function(xi As KeyValuePair(Of String, T)) As TOut
                        Return eval(xi.Value)
                    End Function
        ' cast to tuples for create list later
        Dim result = seq.CastSequence(eval2, parallel, tqdm)
        Dim list As New list With {.slots = New Dictionary(Of String, Object)}

        If result Like GetType(TOut) Then
            Return result.TryCast(Of TOut)
        End If

        Dim outs As TOut() = result.TryCast(Of vector).data

        For i As Integer = 0 To outs.Length - 1
            Call list.slots.Add(seq(i).Key, outs(i))
        Next

        Return list
    End Function

    ''' <summary>
    ''' 一个R对象通用的求值框架函数
    ''' </summary>
    ''' <typeparam name="T">输入的元素类型</typeparam>
    ''' <typeparam name="TOut">输出的元素类型</typeparam>
    ''' <param name="env"></param>
    ''' <param name="x">the input object source, supports vector, list, and scalar data object.
    ''' (输入的数据源)
    ''' </param>
    ''' <param name="eval">求值函数</param>
    ''' <returns>this function returns value of:
    ''' 
    ''' + nothing if the parameter input <paramref name="x"/> has no value, 
    ''' + vector for input is a collection or array
    ''' + list for input is a list
    ''' + Tout value object if the given x is a <typeparamref name="T"/> scalar object
    ''' + and also may returns an error <see cref="Message"/> if error happends!
    ''' </returns>
    <Extension>
    Public Function EvaluateFramework(Of T, TOut)(env As Environment,
                                                  x As Object,
                                                  eval As Func(Of T, TOut),
                                                  Optional parallel As Boolean = False,
                                                  Optional tqdm As Boolean = False) As Object
        If x Is Nothing Then
            Return Nothing
        ElseIf TypeOf x Is list Then
            ' returns variant of TOut or list
            Return DirectCast(x, list).evaluateList(eval, parallel, tqdm, env)
        ElseIf TypeOf x Is vector Then
            ' returns variant of Tout or vector
            With DirectCast(x, vector)
                Dim list As New List(Of TOut)
                Dim populator As IEnumerable(Of Object)

                If tqdm Then
                    populator = TqdmWrapper.Wrap(.data.ToArray(Of Object), wrap_console:=App.EnableTqdm)
                Else
                    populator = .data.AsObjectEnumerator
                End If

                For Each item As Object In populator
                    item = RCType.CTypeDynamic(item, GetType(T), env)

                    If Program.isException(item) Then
                        Return item
                    Else
                        list.Add(eval(item))
                    End If
                Next

                If list.Count = 1 AndAlso Not DataFramework.IsPrimitive(GetType(TOut)) Then
                    Return list(0)
                End If

                Return New vector(
                    names:= .getNames,
                    input:=list.ToArray,
                    type:=RType.GetRSharpType(GetType(TOut)),
                    env:=env
                )
            End With
        ElseIf x.GetType.IsArray Then
            ' returns variant of Tout or vector
            Dim cast As New List(Of T)

            For Each item As Object In DirectCast(x, Array).AsObjectEnumerator
                item = RCType.CTypeDynamic(item, GetType(T), env)

                If Program.isException(item) Then
                    Return item
                Else
                    Call cast.Add(item)
                End If
            Next

            Return cast.CastSequence(eval, parallel, tqdm).Value
        ElseIf TypeOf x Is T Then
            ' returns Tout
            Return eval(DirectCast(x, T))
        ElseIf x.GetType.ImplementInterface(Of IEnumerable(Of T)) Then
            Return DirectCast(x, IEnumerable(Of T)) _
                .AsList _
                .CastSequence(eval, parallel, tqdm) _
                .Value
        Else
            Return RInternal.debug.stop(Message.InCompatibleType(GetType(T), x.GetType, env), env)
        End If
    End Function

    ''' <summary>
    ''' the parallel will not break the input sequence order
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <typeparam name="TOut"></typeparam>
    ''' <param name="cast"></param>
    ''' <param name="eval"></param>
    ''' <param name="parallel"></param>
    ''' <returns>
    ''' variant of vector or <typeparamref name="TOut"/> value
    ''' </returns>
    <Extension>
    Private Function CastSequence(Of T, TOut)(cast As List(Of T),
                                              eval As Func(Of T, TOut),
                                              parallel As Boolean,
                                              tqdm As Boolean) As [Variant](Of vector, TOut)
        Dim list As New List(Of TOut)

        If cast.Count = 1 AndAlso Not DataFramework.IsPrimitive(GetType(TOut)) Then
            Return eval(cast(0))
        End If

        If parallel Then
            ' keeps the input order
            Call cast.SeqIterator _
                .AsParallel _
                .Select(Function(item) (item.i, eval(item.value))) _
                .OrderBy(Function(item) item.i) _
                .Select(Function(item) item.Item2) _
                .DoCall(AddressOf list.AddRange)
        Else
            Dim populator As IEnumerable(Of T)

            If tqdm Then
                populator = TqdmWrapper.Wrap(cast, wrap_console:=App.EnableTqdm)
            Else
                populator = cast
            End If

            For Each item As T In populator
                Call list.Add(eval(item))
            Next
        End If

        Return New vector(
            input:=list.ToArray,
            type:=RType.GetRSharpType(GetType(TOut))
        )
    End Function

    ''' <summary>
    ''' try to convert any object as bytes byffer data
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Public Function Buffer(<RRawVectorArgument> stream As Object, env As Environment) As [Variant](Of Byte(), Message)
        Dim bytes As pipeline = pipeline.TryCreatePipeline(Of Byte)(stream, env)

        If stream Is Nothing Then
            Return Nothing
        ElseIf TypeOf stream Is MemoryStream Then
            Return DirectCast(stream, MemoryStream).ToArray
        ElseIf stream.GetType.IsInheritsFrom(GetType(RawStream)) Then
            Return DirectCast(stream, RawStream).Serialize
        End If

        If bytes.isError Then
            If TypeOf stream Is Stream Then
                Return DirectCast(stream, Stream) _
                    .PopulateBlocks _
                    .IteratesALL _
                    .ToArray
            Else
                Return bytes.getError
            End If
        Else
            Return bytes.populates(Of Byte)(env).ToArray
        End If
    End Function
End Module
