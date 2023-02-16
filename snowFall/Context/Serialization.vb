#Region "Microsoft.VisualBasic::e4a52cc480caa47a3bf553092b0805fa, E:/GCModeller/src/R-sharp/snowFall//Context/Serialization.vb"

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

    '   Total Lines: 100
    '    Code Lines: 60
    ' Comment Lines: 27
    '   Blank Lines: 13
    '     File Size: 3.67 KB


    ' Module Serialization
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: GetBuffer, GetBytes, GetValue, ParseBuffer
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Data.IO
Imports Microsoft.VisualBasic.Data.IO.MessagePack
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Serialize

''' <summary>
''' R# data object serializer
''' </summary>
Public Module Serialization

    Sub New()
        Call Global.Parallel.RegisterDiagnoseBuffer()
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
        If env.globalEnvironment.Rscript.debug Then
            Call Console.WriteLine($"[found symbol::{symbol.name}] {symbol.ToString}")
        End If

        Using buffer As New MemoryStream, writer As New BinaryDataWriter(buffer)
            Call writer.Write(symbol.name, BinaryStringFormat.ZeroTerminated)
            Call writer.Write(symbol.readonly)
            Call writer.Write(symbol.constraint)

            Dim stackTrace As Byte() = MsgPackSerializer.SerializeObject(symbol.stacktrace)
            Dim value As Byte() = GetBuffer(symbol.value, env)

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
        Using reader As New BinaryDataReader(buffer)
            Dim name As String = reader.ReadString(BinaryStringFormat.ZeroTerminated)
            Dim is_readonly As Boolean = reader.ReadBoolean
            Dim type As TypeCodes = reader.ReadByte
            Dim n As Integer = reader.ReadInt32
            Dim stackBuf As Byte() = reader.ReadBytes(n)
            Dim n2 As Integer = reader.ReadInt32
            Dim valueBuf As Byte() = reader.ReadBytes(n2)
            Dim stackframes As StackFrame() = MsgPackSerializer.Deserialize(Of StackFrame())(stackBuf)
            Dim value As Object = ParseBuffer(valueBuf)

            Return New Symbol(name, value, type, is_readonly) With {
                .stacktrace = stackframes
            }
        End Using
    End Function
End Module
