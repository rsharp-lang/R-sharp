#Region "Microsoft.VisualBasic::c8e4a3b1c6db14cba96bfa06c20051f4, D:/GCModeller/src/R-sharp/Library/base//utils/buffer.vb"

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

    '   Total Lines: 253
    '    Code Lines: 179
    ' Comment Lines: 42
    '   Blank Lines: 32
    '     File Size: 9.46 KB


    ' Module buffer
    ' 
    '     Function: asBuffer, escapeString, float, getString, gzipDecompression
    '               normChar, numberFramework, toInteger, zlibDecompress, zlibStream
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.BinaryDumping
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rsharp = SMRUCC.Rsharp

''' <summary>
''' binary data buffer modules 
''' </summary>
<Package("buffer", Category:=APICategories.UtilityTools)>
Module buffer

    ''' <summary>
    ''' cast a numeric vector as bytes data in network byte order
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    <ExportAPI("packBuffer")>
    Public Function asBuffer(x As Double()) As Byte()
        Dim encoder As New NetworkByteOrderBuffer
        Dim bytes As Byte() = encoder.encode(x)
        Return bytes
    End Function

    ''' <summary>
    ''' apply bit convert of the bytes stream data as floats numbers
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <param name="networkOrder"></param>
    ''' <param name="sizeOf"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <example>
    ''' # load numeric vector from a base64 encoded string
    ''' let raw = base64_decode("the base64 text", wrap = FALSE);
    ''' let vec = buffer::float(raw, networkOrder = TRUE, sizeOf = 64);
    ''' 
    ''' print(vec);
    ''' </example>
    <ExportAPI("float")>
    <RApiReturn(GetType(Double))>
    Public Function float(<RRawVectorArgument> stream As Object,
                          Optional networkOrder As Boolean = False,
                          Optional sizeOf As Integer = 32,
                          Optional env As Environment = Nothing) As Object

        If sizeOf = 32 Then
            Return env.numberFramework(stream, networkOrder, 4, AddressOf BitConverter.ToSingle)
        ElseIf sizeOf = 64 Then
            Return env.numberFramework(stream, networkOrder, 8, AddressOf BitConverter.ToDouble)
        Else
            Return Internal.debug.stop($"the given size value '{sizeOf}' is invalid!", env)
        End If
    End Function

    <Extension>
    Private Function numberFramework(Of T)(env As Environment,
                                           <RRawVectorArgument>
                                           stream As Object,
                                           networkOrder As Boolean,
                                           width As Integer,
                                           fromBlock As Func(Of Byte(), Integer, T)) As Object

        Dim buffer As [Variant](Of Byte(), Message) = Rsharp.Buffer(stream, env)

        If buffer Is Nothing Then
            Return Nothing
        ElseIf buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Dim bytes As Byte() = buffer.TryCast(Of Byte())

        If networkOrder AndAlso BitConverter.IsLittleEndian Then
            Return bytes _
                .Split(width) _
                .Select(Function(block)
                            Array.Reverse(block)
                            Return fromBlock(block, Scan0)
                        End Function) _
                .ToArray
        Else
            Return bytes _
               .Split(width) _
               .Select(Function(block)
                           Return fromBlock(block, Scan0)
                       End Function) _
               .ToArray
        End If
    End Function

    ''' <summary>
    ''' apply bit convert of the bytes stream data as integer numbers
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <param name="networkOrder"></param>
    ''' <param name="sizeOf"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("integer")>
    <RApiReturn(GetType(Integer))>
    Public Function toInteger(<RRawVectorArgument> stream As Object,
                              Optional networkOrder As Boolean = False,
                              Optional sizeOf As Integer = 32,
                              Optional env As Environment = Nothing) As Object
        If sizeOf = 32 Then
            Return env.numberFramework(stream, networkOrder, 4, AddressOf BitConverter.ToInt32)
        ElseIf sizeOf = 64 Then
            Return env.numberFramework(stream, networkOrder, 8, AddressOf BitConverter.ToInt64)
        Else
            Return Internal.debug.stop($"the given size value '{sizeOf}' is invalid!", env)
        End If
    End Function

    ''' <summary>
    ''' zip compression of stream data
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("zlib_stream")>
    Public Function zlibStream(<RRawVectorArgument> stream As Object, Optional env As Environment = Nothing) As Object
        Dim buffer As [Variant](Of Byte(), Message) = Rsharp.Buffer(stream, env)

        If buffer Is Nothing Then
            Return Nothing
        ElseIf buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Using ms As MemoryStream = New MemoryStream(buffer.TryCast(Of Byte())).Zip
            Return ms.ToArray
        End Using
    End Function

    ''' <summary>
    ''' zlib decompression of the raw data buffer
    ''' </summary>
    ''' <param name="stream"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("zlib.decompress")>
    <RApiReturn(GetType(Byte))>
    Public Function zlibDecompress(<RRawVectorArgument> stream As Object, Optional env As Environment = Nothing) As Object
        Dim buffer As [Variant](Of Byte(), Message) = Rsharp.Buffer(stream, env)

        If buffer Is Nothing Then
            Return Nothing
        ElseIf buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Using ms As MemoryStream = buffer.TryCast(Of Byte()).UnZipStream
            Return ms.ToArray
        End Using
    End Function

    <ExportAPI("gzip.decompress")>
    Public Function gzipDecompression(<RRawVectorArgument> stream As Object, Optional env As Environment = Nothing) As Object
        Dim buffer As [Variant](Of Byte(), Message) = Rsharp.Buffer(stream, env)

        If buffer Is Nothing Then
            Return Nothing
        ElseIf buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Using ms As MemoryStream = buffer.TryCast(Of Byte()).UnGzipStream
            Return ms.ToArray
        End Using
    End Function

    ''' <summary>
    ''' char to string or raw bytes to string
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("string")>
    Public Function getString(<RRawVectorArgument>
                              raw As Object,
                              Optional encoding As Encodings = Encodings.UTF8,
                              Optional env As Environment = Nothing) As Object

        Dim bytes As pipeline = pipeline.TryCreatePipeline(Of Byte)(raw, env, suppress:=True)

        If bytes.isError Then
            Dim ints As pipeline = pipeline.TryCreatePipeline(Of Integer)(raw, env, suppress:=True)

            If ints.isError Then
                Dim chars As pipeline = pipeline.TryCreatePipeline(Of Char)(raw, env)

                If chars.isError Then
                    Return chars.getError
                Else
                    Return chars.populates(Of Char)(env).CharString
                End If
            Else
                Return ints _
                    .populates(Of Integer)(env) _
                    .Select(AddressOf ChrW) _
                    .CharString
            End If
        Else
            Return bytes _
                .populates(Of Byte)(env) _
                .ToArray _
                .DoCall(AddressOf encoding.CodePage.GetString)
        End If
    End Function

    ReadOnly byteOrder As ByteOrder = ByteOrderHelper.SystemByteOrder
    ReadOnly utf8 As Encoding = Encodings.ANSI.CodePage

    Private Function normChar(x As Integer) As String
        If ASCII.IsAsciiChar(x) Then
            Return Strings.Chr(x)
        Else
            Return $"<U+{x.ToHexString.ToLower}>"
        End If
    End Function

    <ExportAPI("escapeString")>
    Public Function escapeString(str As String) As String
        Dim output As New StringBuilder

        For Each line As String In str.LineTokens
            Dim chars As Integer() = line _
                .Select(Function(c)
                            Dim bits = utf8.GetBytes(c).PadLeft(0, 4)

                            If byteOrder = ByteOrder.LittleEndian Then
                                Call Array.Reverse(bits)
                            End If

                            Return BitConverter.ToInt32(bits, Scan0)
                        End Function) _
                .ToArray
            Dim newLine As String = chars _
                .Select(AddressOf normChar) _
                .JoinBy("") _
                .Replace("?", "")

            Call output.AppendLine(newLine)
        Next

        Return output.ToString
    End Function
End Module
