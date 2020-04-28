Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

Namespace Runtime

    Public Class RContentOutput

        Public ReadOnly Property recommendType As String

        Dim stdout As StreamWriter

        Sub New(stdout As StreamWriter)
            Me.stdout = stdout
        End Sub

        ''' <summary>
        ''' Writes a string followed by a line terminator to the text string or stream.
        ''' </summary>
        ''' <param name="message">
        ''' The string to write. If value is null, only the line terminator is written.
        ''' </param>
        Public Sub WriteLine(Optional message As String = Nothing)
            If stdout Is Nothing Then
                If message Is Nothing Then
                    Call Console.WriteLine()
                Else
                    Call Console.WriteLine(message)
                End If
            Else
                If message Is Nothing Then
                    Call stdout.WriteLine()
                Else
                    Call stdout.WriteLine(message)
                End If
            End If
        End Sub

        Public Sub Write(message As String)
            If stdout Is Nothing Then
                Call Console.Write(message)
            Else
                Call stdout.Write(message)
            End If
        End Sub

        Public Sub Write(data As IEnumerable(Of Byte))
            If stdout Is Nothing Then
                Call CType(App.StdOut.DefaultValue, StreamWriter).Write(data.ToArray)
            Else
                Call stdout.Write(data.ToArray)
            End If
        End Sub

        Public Sub Write(image As Image)
            Using buffer As New MemoryStream
                Call image.Save(buffer, ImageFormat.Png)

                If stdout Is Nothing Then
                    Call CType(App.StdOut.DefaultValue, StreamWriter).Write(buffer.ToArray)
                Else
                    Call stdout.Write(buffer.ToArray)
                End If
            End Using
        End Sub
    End Class
End Namespace