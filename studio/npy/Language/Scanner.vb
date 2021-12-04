Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Language.TokenIcer.Scanner

Namespace Language

    Public Class Scanner

        Dim code As CharPtr
        Dim buffer As New CharBuffer
        Dim escape As New Escapes
        ''' <summary>
        ''' 当前的代码行号
        ''' </summary>
        Dim lineNumber As Integer = 1
        Dim lastPopoutToken As Token

        Private ReadOnly Property lastCharIsEscapeSplash As Boolean
            Get
                Return buffer.GetLastOrDefault = "\"c
            End Get
        End Property

        <DebuggerStepThrough>
        Sub New(source As [Variant](Of String, CharPtr))
            If source Like GetType(String) Then
                Me.code = source.TryCast(Of String).SolveStream
            Else
                Me.code = source.TryCast(Of CharPtr)
            End If
        End Sub

        Public Iterator Function GetTokens() As IEnumerable(Of Token)
            Dim token As New Value(Of Token)
            Dim start As Integer = 0

            Do While Not code
                If Not (token = walkChar(++code)) Is Nothing Then

                End If
            Loop

        End Function

        Private Function walkChar(c As Char) As Token

        End Function
    End Class
End Namespace