Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language

    Module Code

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ParseScript(script As String) As Token()
            Return New Scanner(script).GetTokens().ToArray
        End Function

        <Extension>
        Friend Function SplitByTopLevelDelimiter(tokens As Token(), delimiter As TokenType) As List(Of Token())
            Dim blocks As New List(Of Token())
            Dim buf As New List(Of Token)
            Dim stack As New Stack(Of Token)

            ' 使用最顶层的comma进行分割
            For Each t As Token In tokens
                Dim add As Boolean = True

                If t.name = TokenType.open Then
                    stack.Push(t)
                ElseIf t.name = TokenType.close Then
                    stack.Pop()
                ElseIf t.name = delimiter Then
                    If stack.Count = 0 Then
                        ' 这个是最顶层的分割
                        blocks += buf.PopAll
                        blocks += {t}

                        add = False
                    End If
                End If

                If add Then
                    buf += t
                End If
            Next

            Return blocks + buf.ToArray
        End Function
    End Module
End Namespace