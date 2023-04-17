Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports System.Data
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Public Class SyntaxTree

    ReadOnly script As Rscript
    ReadOnly debug As Boolean = False
    ReadOnly scanner As TsScanner
    ReadOnly opts As SyntaxBuilderOptions

    Sub New(script As Rscript, Optional debug As Boolean = False)
        Me.debug = debug
        Me.script = script
        Me.scanner = New TsScanner(script.script)
        Me.opts = New SyntaxBuilderOptions(AddressOf ParseTypeScriptLine, Function(c, s) New TsScanner(c, s)) With {
            .source = script,
            .debug = debug,
            .pipelineSymbols = {"."}
        }
    End Sub

    Friend Function ParseTsScript() As Program
        Dim tokens As Token() = scanner.GetTokens.ToArray
        Dim syntax = GetExpressions(tokens).ToArray

        For Each exp In syntax
            If exp.isException Then
                Throw New Exception(exp.error.ToString)
            End If
        Next

        Dim prog As New Program(syntax.Select(Function(s) s.expression))

        Return prog
    End Function

    Private Iterator Function GetExpressions(lines As Token()) As IEnumerable(Of SyntaxResult)
        ' {} () []
        Dim stack As New TokenStack(Of TokenType)
        Dim buffer As New List(Of Token)

        ' find the max stack closed scope
        For Each t As Token In lines
            If isNotDelimiter(t) Then
                buffer.Add(t)
            End If

            If t.name = TokenType.open Then
                stack.Push(t)
            ElseIf t.name = TokenType.close Then
                If stack.Pop(t) = StackStates.MisMatched Then
                    Throw New SyntaxErrorException
                End If
            ElseIf isTerminator(t) Then
                If stack.isEmpty Then
                    ' get an expression scope with max stack close range
                    Dim exp = ParseTypeScriptLine(buffer.ToArray, opts)

                    If exp.isException Then
                        ' needs add more token into the buffer list
                        ' do no action
                    Else
                        Yield exp.expression
                    End If
                End If
            End If
        Next
    End Function

    Private Shared Function isNotDelimiter(ByRef t As Token) As Boolean
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

    Private Shared Function isTerminator(t As Token) As Boolean
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
End Class
