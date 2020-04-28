Imports System.Drawing
Imports System.Drawing.Imaging
Imports System.IO

Namespace Runtime

    ''' <summary>
    ''' R# I/O redirect and interface for Rserve http server
    ''' </summary>
    Public Class RContentOutput

        Public ReadOnly Property recommendType As String

        Dim stdout As StreamWriter

        Sub New(stdout As StreamWriter)
            Me.stdout = stdout
        End Sub

        Public Sub Flush()
            Call stdout.Flush()
        End Sub

        ''' <summary>
        ''' Writes a string followed by a line terminator to the text string or stream.
        ''' </summary>
        ''' <param name="message">
        ''' The string to write. If value is null, only the line terminator is written.
        ''' </param>
        Public Sub WriteLine(Optional message As String = Nothing)
            If message Is Nothing Then
                Call stdout.WriteLine()
            Else
                Call stdout.WriteLine(message)
            End If

            Call stdout.Flush()

            If _recommendType Is Nothing Then
                _recommendType = "text/html;charset=UTF-8"
            End If
        End Sub

        Public Sub Write(message As String)
            Call stdout.Write(message)
            Call stdout.Flush()

            If _recommendType Is Nothing Then
                _recommendType = "text/html;charset=UTF-8"
            End If
        End Sub

        Public Sub Write(data As IEnumerable(Of Byte))
            Call stdout.Write(data.ToArray)

            If _recommendType Is Nothing Then
                _recommendType = "application/octet-stream"
            End If
        End Sub

        Public Sub Write(image As Image)
            _recommendType = "image/png"

            Using buffer As New MemoryStream
                Call image.Save(buffer, ImageFormat.Png)
                Call stdout.Write(buffer.ToArray)
            End Using
        End Sub
    End Class
End Namespace