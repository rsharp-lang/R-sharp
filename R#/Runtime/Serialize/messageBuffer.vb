Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Serialize

    Public Class messageBuffer : Inherits RawStream

        Public Property message As String()
        Public Property level As MSG_TYPES
        Public Property environmentStack As StackFrame()
        Public Property trace As StackFrame()
        Public Property source As String

        Sub New(message As Message)
            Me.message = message.message
            Me.level = message.level
            Me.source = message.source.ToString
            Me.environmentStack = message.environmentStack
            Me.trace = message.trace
        End Sub

        Sub New()
        End Sub

        Public Shared Function CreateBuffer(buffer As Stream) As messageBuffer
            Dim level As MSG_TYPES
            Dim int_buf As Byte() = New Byte(3) {}
            Dim text As Encoding = Encodings.UTF8.CodePage
            Dim bytes As Byte()

            buffer.Read(int_buf, Scan0, int_buf.Length)
            level = CType(BitConverter.ToInt32(int_buf, Scan0), MSG_TYPES)

            buffer.Read(int_buf, Scan0, int_buf.Length)
            bytes = New Byte(BitConverter.ToInt32(int_buf, Scan0) - 1) {}
            buffer.Read(bytes, Scan0, bytes.Length)

            Dim source As String = text.GetString(bytes)

            buffer.Read(int_buf, Scan0, int_buf.Length)
            bytes = New Byte(BitConverter.ToInt32(int_buf, Scan0) - 1) {}
            buffer.Read(bytes, Scan0, bytes.Length)

            Dim message As String() = RawStream.GetData(bytes, TypeCode.String)

            buffer.Read(int_buf, Scan0, int_buf.Length)
            bytes = New Byte(BitConverter.ToInt32(int_buf, Scan0) - 1) {}
            buffer.Read(bytes, Scan0, bytes.Length)

            Dim env As StackFrame() = New TraceBuffer(bytes).StackTrace

            buffer.Read(int_buf, Scan0, int_buf.Length)
            bytes = New Byte(BitConverter.ToInt32(int_buf, Scan0) - 1) {}
            buffer.Read(bytes, Scan0, bytes.Length)

            Dim trace As StackFrame() = New TraceBuffer(bytes).StackTrace

            Return New messageBuffer With {
                .environmentStack = env,
                .level = level,
                .message = message,
                .source = source,
                .trace = trace
            }
        End Function

        Public Overrides Function Serialize() As Byte()
            Dim text As Encoding = Encodings.UTF8.CodePage
            Dim bytes As Byte()

            Using buffer As New MemoryStream()
                Call buffer.Write(BitConverter.GetBytes(CInt(level)), Scan0, 4)

                bytes = text.GetBytes(source)
                buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
                buffer.Write(bytes, Scan0, bytes.Length)

                bytes = RawStream.GetBytes(message)
                buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
                buffer.Write(bytes, Scan0, bytes.Length)

                Dim trace As New TraceBuffer With {.StackTrace = environmentStack}

                bytes = trace.Serialize
                buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
                buffer.Write(bytes, Scan0, bytes.Length)

                trace = New TraceBuffer With {.StackTrace = Me.trace}

                bytes = trace.Serialize
                buffer.Write(BitConverter.GetBytes(bytes.Length), Scan0, 4)
                buffer.Write(bytes, Scan0, bytes.Length)

                buffer.Flush()

                Return buffer.ToArray
            End Using
        End Function
    End Class
End Namespace