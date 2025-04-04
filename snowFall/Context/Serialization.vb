﻿#Region "Microsoft.VisualBasic::18925dd27e40be8cd55cb0f182db1723, snowFall\Context\Serialization.vb"

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

    '   Total Lines: 129
    '    Code Lines: 83 (64.34%)
    ' Comment Lines: 28 (21.71%)
    '    - Xml Docs: 96.43%
    ' 
    '   Blank Lines: 18 (13.95%)
    '     File Size: 4.78 KB


    ' Module Serialization
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetBuffer, GetBytes, GetValue, ParseBuffer
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Text
Imports Darwinism.HPC.Parallel
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Serialize

''' <summary>
''' R# data object serializer
''' </summary>
Public Module Serialization

    Sub New()
        Call Darwinism.HPC.Parallel.RegisterDiagnoseBuffer()
    End Sub

    ''' <summary>
    ''' serialize R# object to byte buffer
    ''' </summary>
    ''' <param name="R"></param>
    ''' <returns></returns>
    Public Function GetBuffer(R As Object, env As Environment) As Byte()
        Dim buffer As Buffer = BufferHandler.getBuffer(R, env)
        Dim payload As Byte() = buffer.Serialize

        Return payload
    End Function

    ''' <summary>
    ''' parse R# object from byte buffer
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <returns></returns>
    Public Function ParseBuffer(buffer As Byte()) As Object
        Using file As New MemoryStream(buffer)
            Dim buf As Buffer = Serialize.Buffer.ParseBuffer(file)
            Dim payload = buf.data
            Dim result As Object = payload.getValue

            Return result
        End Using
    End Function

    ''' <summary>
    ''' serialize R# symbol to byte buffer
    ''' </summary>
    ''' <param name="symbol"></param>
    ''' <returns></returns>
    Public Function GetBytes(symbol As Symbol, env As Environment) As Byte()
        Dim value As Byte()
        Dim stackTrace As Byte() = MsgPackSerializer.SerializeObject(symbol.stacktrace)

        If env.globalEnvironment.Rscript.debug Then
            Call Console.WriteLine($"[found symbol::{symbol.name}] {symbol.ToString}")
        End If

        Using buffer As New MemoryStream, writer As New BinaryDataWriter(buffer)
            Call writer.Write(symbol.name, BinaryStringFormat.ZeroTerminated)
            Call writer.Write(symbol.readonly)

            If TypeOf symbol.value Is RMethodInfo Then
                Dim del As New IDelegate(DirectCast(symbol.value, RMethodInfo).GetNetCoreCLRDeclaration)
                Dim json As String = del.GetJson

                writer.Write(TypeCodes.clr_delegate)
                value = Encoding.UTF8.GetBytes(json)
            Else
                writer.Write(symbol.constraint)
                value = GetBuffer(symbol.value, env)
            End If

            Call writer.Write(stackTrace.Length)
            Call writer.Write(stackTrace)
            Call writer.Write(value.Length)
            Call writer.Write(value)
            Call writer.Flush()
            Call writer.Seek(Scan0, SeekOrigin.Begin)

            Return buffer.ToArray
        End Using
    End Function

    ''' <summary>
    ''' parse R# symbol from a byte buffer
    ''' </summary>
    ''' <param name="buffer"></param>
    ''' <returns>
    ''' a symbol object that contains the stack trace 
    ''' information of the parent environment from the 
    ''' master node. decode at slave node.
    ''' </returns>
    Public Function GetValue(buffer As Stream) As Symbol
        Dim value As Object

        Using reader As New BinaryDataReader(buffer)
            Dim name As String = reader.ReadString(BinaryStringFormat.ZeroTerminated)
            Dim is_readonly As Boolean = reader.ReadBoolean
            Dim type As TypeCodes = reader.ReadByte
            Dim n As Integer = reader.ReadInt32
            Dim stackBuf As Byte() = reader.ReadBytes(n)
            Dim n2 As Integer = reader.ReadInt32
            Dim valueBuf As Byte() = reader.ReadBytes(n2)
            Dim stackframes As StackFrame() = MsgPackSerializer.Deserialize(Of StackFrame())(stackBuf)

            ' A little hack about the .NET clr function serialization
            If type = TypeCodes.clr_delegate Then
                Dim json As String = Encoding.UTF8.GetString(valueBuf)
                Dim del As IDelegate = json.LoadJSON(Of IDelegate)
                Dim clr_func As MethodInfo = del.GetMethod
                Dim r_func As New RMethodInfo(ImportsPackage.TryParse(clr_func, strict:=False))

                value = r_func
            Else
                value = ParseBuffer(valueBuf)
            End If

            Return New Symbol(name, value, type, is_readonly) With {
                .stacktrace = stackframes
            }
        End Using
    End Function
End Module
