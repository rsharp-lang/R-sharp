Imports System.Data
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

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

    Private Iterator Function GetExpressions(lines As Pointer(Of Token)) As IEnumerable(Of SyntaxResult)
        ' {} () []
        Dim stack As New TokenStack(Of TokenType)
        Dim buffer As New List(Of SyntaxToken)
        Dim i As New Value(Of Token)
        Dim t As Token
        Dim state As New Value(Of StackStates)

        ' find the max stack closed scope
        Do While (i = ++lines) IsNot Nothing
            t = i

            If isNotDelimiter(t) Then
                buffer.Add(New SyntaxToken(buffer.Count, t))
            End If

            If t.name = TokenType.open Then
                stack.Push(t, buffer.Count - 1)
            ElseIf t.name = TokenType.close Then
                If (state = stack.Pop(t, buffer.Count - 1)).MisMatched Then
                    Throw New SyntaxErrorException
                Else
                    Dim range = state.Value.GetRange(buffer).ToArray
                    Dim exp = range.GetExpression(fromComma:=False, opts)

                    If range.First Like GetType(Token) Then
                        Dim left = range.First.TryCast(Of Token)
                        Dim leftToken As SyntaxToken = state.Value.Left(buffer)

                        If left.name = TokenType.open Then
                            If left.text = "(" Then
                                If leftToken Is Nothing Then
                                    ' (...)
                                    state.Value.RemoveRange(buffer)
                                    buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp))
                                ElseIf leftToken Like GetType(Token) Then
                                    Dim lt = leftToken.TryCast(Of Token)

                                    If lt.name = TokenType.keyword AndAlso lt.text = "function" Then
                                        ' is function declare
                                        ' do nothing
                                        Continue Do
                                    End If

                                    Dim target = Expression.CreateExpression({lt}, opts)

                                    If target Like GetType(SymbolReference) Then
                                        ' invoke function
                                        ' func(...)
                                        buffer.RemoveRange(state.Value.Range.Min - 1, state.Value.Range.Length + 2)
                                        exp = New FunctionInvoke(target.expression, opts.GetStackTrace(t), ExpressionCollection.GetExpressions(exp))
                                        buffer.Insert(state.Value.Range.Min - 1, New SyntaxToken(-1, exp))
                                        Reindex(buffer)
                                    ElseIf target.isException Then
                                        Yield target
                                    Else
                                        Throw New NotImplementedException
                                    End If
                                Else
                                    Dim target = leftToken.TryCast(Of Expression)

                                    If TypeOf target Is SymbolReference Then
                                        ' invoke function
                                        ' func(...)
                                        state.Value.RemoveRange(buffer)
                                        exp = New FunctionInvoke(target, Nothing, ExpressionCollection.GetExpressions(exp))
                                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp))
                                        Reindex(buffer)
                                    Else
                                        Throw New NotImplementedException
                                    End If
                                End If
                            ElseIf left.text = "{" Then
                                If leftToken Is Nothing Then
                                    ' is a multiple line closure expression
                                    ' {...}
                                    state.Value.RemoveRange(buffer)
                                    buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp))
                                ElseIf leftToken Like GetType(Token) Then
                                    Dim lt = leftToken.TryCast(Of Token)

                                Else

                                End If
                            End If
                        End If
                    End If

                    Yield exp
                End If
            ElseIf t.name = TokenType.comma Then
                Dim index = Traceback(buffer)
                Dim range = buffer.Skip(index).Take(buffer.Count - index).ToArray
                Dim exp = range.GetExpression(fromComma:=True, opts)

                Yield exp
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
                Else
                    If t.name = TokenType.newLine Then
                        ' remove current newline token
                        buffer.Pop()
                    End If
                End If
            End If
        Loop

        If buffer > 0 Then
            Yield ParseTypeScriptLine(buffer.PopAll, opts)
        End If
    End Function

End Class
