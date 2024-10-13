﻿#Region "Microsoft.VisualBasic::7ef1fecbbd6d8fba11920790775bab36, R#\ApiArgumentHelpers.vb"

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

    '   Total Lines: 266
    '    Code Lines: 189 (71.05%)
    ' Comment Lines: 49 (18.42%)
    '    - Xml Docs: 91.84%
    ' 
    '   Blank Lines: 28 (10.53%)
    '     File Size: 10.64 KB


    ' Module ApiArgumentHelpers
    ' 
    '     Function: FileStreamWriter, GetDoubleRange, GetFileStream, GetNamedValueTuple, rangeFromVector
    '               slot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Public Module ApiArgumentHelpers

    ''' <summary>
    ''' create a slot value reference inside a tuple <see cref="list"/> for construct a new list data
    ''' </summary>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Function slot(name As String) As ArgumentReference
        Return CType(name, ArgumentReference)
    End Function

    Public Function list(ParamArray slots As ArgumentReference()) As list
        Return New list(slots)
    End Function

    ''' <summary>
    ''' create a name tagged value object from a 
    ''' given data source value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="value">
    ''' any type of the .NET object
    ''' </param>
    ''' <param name="env"></param>
    ''' <param name="api">
    ''' the traceback tag
    ''' </param>
    ''' <returns></returns>
    Public Function GetNamedValueTuple(Of T)(value As Object,
                                             env As Environment,
                                             <CallerMemberName>
                                             Optional api$ = Nothing) As [Variant](Of NamedValue(Of T), Message)
        If value Is Nothing Then
            Return Nothing
        End If

        Select Case value.GetType
            Case GetType(NamedValue(Of T))
                Return DirectCast(value, NamedValue(Of T))
            Case GetType(list)
                Dim list As list = DirectCast(value, list)
                Dim name As String = list.slots.Keys.First

                value = DirectCast(value, list).slots(name)
                value = RCType.CTypeDynamic(value, GetType(T), env)

                Return New NamedValue(Of T) With {
                    .Name = name,
                    .Value = value,
                    .Description = list.getValue(Of String)("description", env)
                }

            Case Else
                Return debug.stop({
                    "invalid data type for cast to a numeric range!",
                    "required: " & GetType(NamedValue(Of T)).FullName,
                    "given: " & value.GetType.FullName,
                    ".NET traceback: " & api
                }, env)
        End Select
    End Function

    Public Function GetDoubleRange(value As Object, env As Environment,
                                   Optional default$ = "0,1",
                                   <CallerMemberName>
                                   Optional api$ = Nothing) As [Variant](Of DoubleRange, Message)
        If value Is Nothing Then
            Return DoubleRange.TryParse([default])
        End If

        Select Case value.GetType
            Case GetType(vector)
                Return DirectCast(value, vector).rangeFromVector(env, api)
            Case GetType(DoubleRange)
                Return DirectCast(value, DoubleRange)
            Case GetType(String)
                Return DoubleRange.TryParse(DirectCast(value, String))
            Case Else
                If value.GetType.IsArray Then
                    Return New vector(DirectCast(value, Array), RType.GetRSharpType(GetType(Double))).rangeFromVector(env, api)
                End If

                Return debug.stop({
                    "invalid data type for cast to a numeric range!",
                    "required: " & GetType(DoubleRange).FullName,
                    "given: " & value.GetType.FullName
                }, env)
        End Select
    End Function

    <Extension>
    Private Function rangeFromVector(v As vector, env As Environment, api$) As [Variant](Of DoubleRange, Message)
        Dim vec As Double()

        Select Case v.elementType.mode
            Case TypeCodes.double
                vec = CLRVector.asNumeric(v.data)
            Case TypeCodes.integer
                vec = CLRVector.asNumeric(v.data)
            Case TypeCodes.string

                If v.length = 1 Then
                    Return DoubleRange.TryParse(DirectCast(v.data.GetValue(Scan0), String))
                Else
                    vec = v.data _
                        .AsObjectEnumerator(Of String) _
                        .Select(AddressOf Val) _
                        .ToArray
                End If

            Case Else
                Return debug.stop({
                    "invalid vector data type!",
                    "mode: " & v.elementType.mode,
                    "raw: " & v.elementType.fullName
                }, env)
        End Select

        If vec.Length < 2 Then
            Return debug.stop({
                "a numeric range required two boundary value at least!",
                "api: " & api
            }, env)
        Else
            Return New DoubleRange(vec)
        End If
    End Function

    ''' <summary>
    ''' open stream for file write
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="write"></param>
    ''' 
    <Extension>
    Public Function FileStreamWriter(env As Environment, file As Object, write As Action(Of Stream)) As Object
        Dim stream As Stream
        Dim is_file As Boolean = False

        If file Is Nothing Then
            stream = Console.OpenStandardOutput
        ElseIf TypeOf file Is String Then
            stream = DirectCast(file, String).Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            is_file = True
        ElseIf TypeOf file Is Stream Then
            stream = file
        Else
            Return Message.InCompatibleType(GetType(Stream), file.GetType, env)
        End If

        Call write(stream)
        Call stream.Flush()

        If is_file Then
            Call stream.Close()
            Call stream.Dispose()
        End If

        Return True
    End Function

    ''' <summary>
    ''' the memory stream object that construct via the given byte array works in readonly mode!
    ''' </summary>
    Const ReadOnlyRawByteStream As String = "the file open mode should be readonly when the input file data is a byte data collection."

    ''' <summary>
    ''' get a file stream object
    ''' </summary>
    ''' <param name="file">
    ''' the file resource could be one of the:
    ''' 
    ''' 1. a valid file path to a filesystem location
    ''' 2. a stream object that can be read/write based on the <paramref name="mode"/> context
    ''' 3. a raw bytes array for construct a memory stream in this function(readonly)
    ''' </param>
    ''' <param name="mode"></param>
    ''' <param name="env"></param>
    ''' <param name="suppress"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' base on the value of <paramref name="mode"/>, this function has different behaviour:
    ''' 
    ''' 1. <see cref="FileAccess.Read"/> for a readonly stream
    ''' 2. <see cref="FileAccess.ReadWrite"/> for create a stream could be used for write data
    ''' 3. <see cref="FileAccess.Write"/> will truncate the stream at first!
    ''' </remarks>
    Public Function GetFileStream(file As Object,
                                  mode As FileAccess,
                                  env As Environment,
                                  Optional suppress As Boolean = False,
                                  <Out>
                                  Optional ByRef is_filepath As Boolean = False,
                                  Optional lazy As Boolean = False) As [Variant](Of Stream, Message)

        If TypeOf file Is vector Then
            file = DirectCast(file, vector).data
        End If
        If TypeOf file Is Array AndAlso DirectCast(file, Array).Length = 1 Then
            file = DirectCast(file, Array).GetValue(Scan0)
        End If
        If TypeOf file Is Array Then
            file = TryCastGenericArray(DirectCast(file, Array), env)

            If TypeOf file Is Message Then
                Return DirectCast(file, Message)
            End If
        End If

        If TypeOf file Is Array AndAlso DirectCast(file, Array).Length = 1 Then
            file = DirectCast(file, Array).GetValue(Scan0)
        End If

        If file Is Nothing Then
            Return Internal.debug.stop({"file output can not be nothing!"}, env, suppress)
        Else
            is_filepath = False
        End If

        If TypeOf file Is String Then
            is_filepath = True

            ' from a local file
            If mode = FileAccess.Read Then
                Return DirectCast(file, String).Open(
                    If(lazy,
                    FileMode.OpenOrCreate,  ' not load into memoty at once(lazy)
                    FileMode.Open           ' read all data into memory at once
                    ), doClear:=False, [readOnly]:=True)
            ElseIf mode = FileAccess.Write Then
                Return DirectCast(file, String).Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
            Else
                Return DirectCast(file, String).Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False)
            End If
        ElseIf TypeOf file Is Stream OrElse TypeOf file Is MemoryStream Then
            Return DirectCast(file, Stream)
        ElseIf TypeOf file Is Byte() Then
            If mode = FileAccess.Read Then
                Return New MemoryStream(DirectCast(file, Byte()))
            Else
                Return Internal.debug.stop(ReadOnlyRawByteStream, env, suppress)
            End If
        ElseIf TypeOf file Is StreamWriter AndAlso mode = FileAccess.Write Then
            Return DirectCast(file, StreamWriter).BaseStream
        ElseIf file.GetType.IsArray Then
            If mode = FileAccess.Read Then
                Return New MemoryStream(CLRVector.asRawByte(file))
            Else
                Return Internal.debug.stop(ReadOnlyRawByteStream, env, suppress)
            End If
        Else
            Return Message.InCompatibleType(GetType(Stream), file.GetType, env,, NameOf(file), suppress)
        End If
    End Function
End Module
