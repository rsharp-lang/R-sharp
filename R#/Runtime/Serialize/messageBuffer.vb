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



                buffer.Flush()

                Return buffer.ToArray
            End Using
        End Function
    End Class
End Namespace