Imports Microsoft.VisualBasic.Text.Parser
Imports Microsoft.VisualBasic.Language
Imports System.Runtime.CompilerServices

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
            Dim tokenicer As Scanner
            Dim stack As New Stack(Of Token)

            code = [string]
            buffer *= 0
            escape = New Scanner.Escapes With {.[string] = True}

            Do While Not code
                If Not (token = walkChar(++code)) Is Nothing Then
                    Yield token

                    ' 当前的字符为 { 栈起始符号
                    ' 继续解析token直到遇到最顶层的 栈结束符号 }
                    tokenicer = New Scanner(code)

                    For Each t As Token In tokenicer.GetTokens
                        If t.name = TokenType.open AndAlso t.text = "{" Then
                            Call stack.Push(t)
                        ElseIf t.name = TokenType.close AndAlso t.text = "}" Then
                            Call stack.Pop()
                        End If

                        Yield t

                        If stack.Count = 0 Then
                            Exit For
                        End If
                    Next

                    escape.string = True
                End If
            Loop

            If buffer > 0 Then
                Yield New Token With {
                    .text = buffer.CharString,
                    .name = TokenType.stringLiteral
                }
            End If
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
            End If

            Return Nothing
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function ParseTokens(expression As String) As Token()
            Return New StringInterpolation().GetTokens(expression).ToArray
        End Function
    End Class
End Namespace