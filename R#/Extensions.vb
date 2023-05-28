#Region "Microsoft.VisualBasic::3b8114e455bd2911bd73cea732e6bafb, F:/GCModeller/src/R-sharp/R#//Extensions.vb"

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

    '   Total Lines: 245
    '    Code Lines: 175
    ' Comment Lines: 42
    '   Blank Lines: 28
    '     File Size: 8.77 KB


    ' Module Extensions
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: AsRReturn, Buffer, EvaluateFramework, GetEncoding, GetObject
    '               GetString, ParseDebugLevel, SafeCreateColumns, toList
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
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
    ''' 一个R对象通用的求值框架函数
    ''' </summary>
    ''' <typeparam name="T">输入的元素类型</typeparam>
    ''' <typeparam name="TOut">输出的元素类型</typeparam>
    ''' <param name="env"></param>
    ''' <param name="x">the input object source, supports vector, list, and scalar data object.
    ''' (输入的数据源)
    ''' </param>
    ''' <param name="eval">求值函数</param>
    ''' <returns></returns>
    <Extension>
    Public Function EvaluateFramework(Of T, TOut)(env As Environment, x As Object, eval As Func(Of T, TOut)) As Object
        If x Is Nothing Then
            Return Nothing
        ElseIf TypeOf x Is list Then
            Return DirectCast(x, list) _
                .AsGeneric(Of T)(env) _
                .ToDictionary(Function(a) a.Key,
                              Function(a)
                                  Return CObj(eval(a.Value))
                              End Function) _
                .DoCall(Function(list)
                            Return New list With {
                                .slots = list
                            }
                        End Function)
        ElseIf TypeOf x Is vector Then
            With DirectCast(x, vector)
                Dim list As New List(Of TOut)

                For Each item As Object In .data.AsObjectEnumerator
                    item = RCType.CTypeDynamic(item, GetType(T), env)

                    If Program.isException(item) Then
                        Return item
                    Else
                        list.Add(eval(item))
                    End If
                Next

                Return New vector(
                    names:= .getNames,
                    input:=list.ToArray,
                    type:=RType.GetRSharpType(GetType(TOut)),
                    env:=env
                )
            End With
        ElseIf x.GetType.IsArray Then
            Dim list As New List(Of TOut)

            For Each item As Object In DirectCast(x, Array).AsObjectEnumerator
                item = RCType.CTypeDynamic(item, GetType(T), env)

                If Program.isException(item) Then
                    Return item
                Else
                    list.Add(eval(item))
                End If
            Next

            Return New vector(
                input:=list.ToArray,
                type:=RType.GetRSharpType(GetType(TOut))
            )
        ElseIf TypeOf x Is T Then
            Return eval(DirectCast(x, T))
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(T), x.GetType, env), env)
        End If
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
