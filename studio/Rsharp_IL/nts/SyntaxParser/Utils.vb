﻿Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Public Module Utils

    <Extension>
    Public Sub RemoveRange(stat As StackStates, buffer As List(Of SyntaxToken))
        Call buffer.RemoveRange(stat.Range.Min, stat.Range.Length)
    End Sub

    Friend Sub Reindex(ByRef buffer As List(Of SyntaxToken))
        For i As Integer = 0 To buffer.Count - 1
            buffer(i).index = i
        Next
    End Sub

    ''' <summary>
    ''' traceback the index to the last comma or open token
    ''' </summary>
    ''' <returns></returns>
    Friend Function Traceback(buffer As List(Of SyntaxToken), match As TokenType()) As Integer
        For i As Integer = buffer.Count - 2 To 0 Step -1
            If buffer(i) Like GetType(Token) Then
                Dim type As TokenType = buffer(i).TryCast(Of Token).name
                Dim assert As Boolean = match.Any(Function(f) f = type)

                If assert Then
                    Return i + 1
                End If
            End If
        Next

        Return 0
    End Function

    Friend Function isNotDelimiter(ByRef t As Token) As Boolean
        If t.name <> TokenType.delimiter Then
            Return True
        Else
            If t.text = vbCr OrElse t.text = vbLf Then
                t = New Token(TokenType.newLine, vbCr)
                Return True
            End If

            Return False
        End If
    End Function

    <Extension>
    Public Function isSequenceSymbol(t As SyntaxToken) As Boolean
        If t Like GetType(Token) Then
            Return t.TryCast(Of Token).name = TokenType.sequence
        Else
            Return False
        End If
    End Function

    <Extension>
    Public Function isComma(t As SyntaxToken) As Boolean
        If t Like GetType(Token) Then
            Return t.TryCast(Of Token).name = TokenType.comma
        Else
            Return False
        End If
    End Function

    <Extension>
    Friend Function isTerminator(t As SyntaxToken) As Boolean
        If t Like GetType(Token) Then
            Return t.TryCast(Of Token).isTerminator
        Else
            Return False
        End If
    End Function

    <Extension>
    Friend Function isTerminator(t As Token) As Boolean
        If t.name = TokenType.terminator Then
            Return True
        ElseIf t.name = TokenType.newLine Then
            Return True
        ElseIf t.name = TokenType.delimiter Then
            If t.text = vbCr OrElse t.text = vbLf Then
                Return True
            Else
                Return False
            End If
        Else
            Return False
        End If
    End Function
End Module