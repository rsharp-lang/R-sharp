Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Development.Components

    Public Class ProgressBarFunction : Inherits RDefaultFunction

        ''' <summary>
        ''' total n task number
        ''' </summary>
        ReadOnly n As Integer
        ''' <summary>
        ''' interval for print the progress
        ''' </summary>
        ReadOnly d As Integer
        ReadOnly width As Integer
        ReadOnly t0 As Date = Now

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="n">total task size</param>
        ''' <param name="interval">
        ''' [1,100]
        ''' </param>
        Sub New(n As Integer, interval As Integer, Optional width As Integer = 45)
            Me.width = width
            Me.n = n
            Me.d = n * (interval / 100)
        End Sub

        Dim i As Integer = 0

        <RDefaultFunction>
        Public Sub Tick(setLabel As String)
            If i Mod d = 0 Then
                Dim walk_chars As Integer = (i / n) * width
                ' print progress
                Dim walk As New String(Tqdm.ProgressBar.FullChar, walk_chars)
                Dim blank As New String("_"c, width - walk_chars)
                Dim bar As New StringBuilder("|")
                Dim span As TimeSpan = Now - t0
                Dim speed As Double = i / span.TotalSeconds
                Dim elapsed As String = StringFormats.ReadableElapsedTime(span)

                Call bar.Append(walk)
                Call bar.Append(blank)
                Call bar.Append("|")
                Call bar.Append($" [{(i / n * 100).ToString("F1")}% ~ {speed.ToString("F2")}itr/s, Elapsed:{elapsed}] ")
                Call bar.Append(setLabel)

                Call VBDebugger.EchoLine(bar.ToString)
            End If

            i += 1
        End Sub
    End Class
End Namespace