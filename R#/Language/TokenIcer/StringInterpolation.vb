Imports Microsoft.VisualBasic.Text.Parser
Imports Microsoft.VisualBasic.Language

Namespace Language.TokenIcer

    ' let s = `This is a ${"string"}.`

    ''' <summary>
    ''' The string interpolation expression token parser
    ''' </summary>
    Public Class StringInterpolation

        Dim code As CharPtr
        Dim buffer As New List(Of Char)
        Dim escape As Scanner.Escapes

        Public ReadOnly Property isEscapeSplash As Boolean
            Get
                Return buffer > 0 AndAlso buffer.Last = "\"c
            End Get
        End Property

        Public Iterator Function GetTokens([string] As String) As IEnumerable(Of Token)
            Dim token As New Value(Of Token)

            code = [string]
            buffer *= 0
            escape = New Scanner.Escapes With {.[string] = True}

            Do While Not code
                If Not (token = walkChar(++code)) Is Nothing Then
                    Yield token
                End If
            Loop
        End Function

        Private Function walkChar(c As Char) As Token
            If c = "$"c AndAlso code.Current = "{"c AndAlso Not isEscapeSplash Then
                escape.string = False

                Return New Token With {
                    .text = buffer.PopAll.CharString,
                    .name = TokenType.stringLiteral
                }
            End If

            If escape.string Then
                buffer += c
            Else
                ' expression parts

            End If
        End Function

    End Class
End Namespace