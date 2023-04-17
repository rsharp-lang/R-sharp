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
        Dim lines = tokens.Split(Function(t) t.name = TokenType.newLine) _
            .Where(Function(t) t.Length > 0) _
            .ToArray
        Dim syntax = GetExpressions(lines).ToArray

        For Each exp In syntax
            If exp.isException Then
                Throw New Exception(exp.error.ToString)
            End If
        Next

        Dim prog As New Program(syntax.Select(Function(s) s.expression))

        Return prog
    End Function

    Private Iterator Function GetExpressions(lines As Token()()) As IEnumerable(Of SyntaxResult)
        ' {} () []
        Dim stack As New TokenStack(Of TokenType)
        Dim buffer As New List(Of Token)

        For Each line As Token() In lines
            ' find the max stack closed scope
            For Each t As Token In line
                If t.name <> TokenType.delimiter Then
                    buffer.Add(t)
                End If

                If t.name = TokenType.open Then
                    stack.Push(t)
                ElseIf t.name = TokenType.close Then
                    If stack.Pop(t) = StackStates.MisMatched Then
                        Throw New SyntaxErrorException
                    End If
                ElseIf t.name = TokenType.terminator Then
                    If stack.isEmpty Then
                        ' get an expression scope with max stack close range
                        Yield ParseTypeScriptLine(buffer.PopAll, opts)
                    End If
                End If
            Next
        Next
    End Function

    Private Shared Function isTerminator()

    End Function
End Class
