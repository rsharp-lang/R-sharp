Imports System.Data
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
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

    ' {} () []
    Dim stack As New TokenStack(Of TokenType)
    Dim buffer As New List(Of SyntaxToken)
    Dim i As New Value(Of Token)
    Dim t As Token
    Dim state As New Value(Of StackStates)

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
        Dim syntax = GetExpressions(tokens) _
            .Where(Function(exp) Not exp Is Nothing) _
            .ToArray

        For Each exp As SyntaxResult In syntax
            If exp.isException Then
                Throw New Exception(exp.error.ToString)
            End If
        Next

        Dim prog As New Program(syntax.Select(Function(s) s.expression)) With {
            .Rscript = script
        }

        Return prog
    End Function

    ''' <summary>
    ''' func(...)
    ''' </summary>
    ''' <param name="lt">
    ''' probably the function name
    ''' </param>
    ''' <param name="exp">
    ''' probably the function argument list
    ''' </param>
    ''' <returns></returns>
    Private Function ParseFuncInvoke(lt As Token, exp As SyntaxResult) As SyntaxResult
        If lt.isAnyKeyword("function", "if") Then
            ' is function declare
            ' do nothing
            Return Nothing
        ElseIf lt.isAnyKeyword("for") Then
            ' for loop
            buffer.RemoveRange(state.Value.Range.Min + 1, state.Value.Range.Length - 1)
            buffer.Insert(state.Value.Range.Min + 1, New SyntaxToken(-1, exp.expression))
            Reindex(buffer)

            Return Nothing
        ElseIf lt.isAnyKeyword("require") Then
            ' javascript require
            buffer.RemoveRange(state.Value.Range.Min - 1, state.Value.Range.Length + 2)
            exp = New Require(ExpressionCollection.GetExpressions(exp.expression))
            buffer.Insert(state.Value.Range.Min - 1, New SyntaxToken(-1, exp.expression))
            Reindex(buffer)

            Return Nothing
        End If

        Dim target = Expression.CreateExpression({lt}, opts)

        If target Like GetType(SymbolReference) Then
            ' invoke function
            ' func(...)
            buffer.RemoveRange(state.Value.Range.Min - 1, state.Value.Range.Length + 2)
            exp = New FunctionInvoke(DirectCast(target.expression, SymbolReference).symbol, opts.GetStackTrace(t), ExpressionCollection.GetExpressions(exp.expression))
            buffer.Insert(state.Value.Range.Min - 1, New SyntaxToken(-1, exp.expression))
            Reindex(buffer)
        ElseIf target.isException Then
            Return target
        Else
            Throw New NotImplementedException
        End If

        Return Nothing
    End Function

    Private Function PopOut() As SyntaxResult
        If (state = stack.Pop(t, buffer.Count - 1)).MisMatched Then
            Throw New SyntaxErrorException
        Else
            Return PopOutStack()
        End If
    End Function

    Private Function PopOutStack() As SyntaxResult
        Dim range = state.Value.GetRange(buffer).ToArray
        Dim exp = range.GetExpression(fromComma:=False, opts)

        If exp.isException Then
            Return exp
        End If

        If Not range.First Like GetType(Token) Then
            Return Nothing
        End If

        Dim left = range.First.TryCast(Of Token)
        Dim leftToken As SyntaxToken = state.Value.Left(buffer)

        If left.name = TokenType.open Then
            If left.text = "(" Then
                If leftToken Is Nothing Then
                    ' (...)
                    state.Value.RemoveRange(buffer)
                    buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                ElseIf leftToken Like GetType(Token) Then
                    If leftToken.IsToken(TokenType.operator) Then
                        ' operator for binary expression, example like:
                        ' 1 / (...)
                        buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Length + 1)
                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, ExpressionCollection.GetExpressions(exp.expression).First))
                        Reindex(buffer)

                        Return Nothing
                    Else
                        Return ParseFuncInvoke(lt:=leftToken.TryCast(Of Token), exp)
                    End If
                Else
                    Dim target = leftToken.TryCast(Of Expression)

                    If TypeOf target Is SymbolReference Then
                        ' invoke function
                        ' func(...)
                        state.Value.RemoveRange(buffer)
                        exp = New FunctionInvoke(target, Nothing, ExpressionCollection.GetExpressions(exp.expression))
                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
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
                    buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                ElseIf leftToken Like GetType(Token) Then
                    If leftToken.TryCast(Of Token) = (TokenType.close, ")") Then
                        ' is a possible function declare
                        Dim index = Traceback(buffer, {TokenType.keyword})

                        buffer.RemoveRange(state.Value.Range.Min + 1, state.Value.Range.Length - 1)
                        buffer.Insert(state.Value.Range.Min + 1, New SyntaxToken(-1, exp.expression))
                        Reindex(buffer)

                        range = buffer.Skip(index - 1).Take(buffer.Count - index + 1).ToArray
                        exp = range.GetExpression(fromComma:=True, opts)

                        If exp Is Nothing OrElse exp.isException Then
                            Return exp
                        End If

                        buffer.RemoveRange(index - 1, range.Length)
                        buffer.Insert(index - 1, New SyntaxToken(-1, exp.expression))
                        Reindex(buffer)
                    ElseIf leftToken.TryCast(Of Token) = (TokenType.sequence, ":") Then
                        ' is json value
                        buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Length + 1)
                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                        Reindex(buffer)
                    ElseIf leftToken.TryCast(Of Token) = (TokenType.open, "[") Then
                        ' json array [{...}]
                        buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Length + 1)
                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                        Reindex(buffer)
                    ElseIf leftToken.TryCast(Of Token) = (TokenType.open, "(") Then
                        buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Length + 1)
                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                        Reindex(buffer)
                    ElseIf leftToken.IsToken(TokenType.keyword) Then
                        ' else {}
                        ' else if {}
                        ' try {}
                        buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Length + 1)
                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                        Reindex(buffer)
                    ElseIf leftToken.isComma Then
                        ' last element in json literal
                        ' 
                        ' {...,...}
                        buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Length + 1)
                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                        Reindex(buffer)
                    End If
                Else

                End If
            ElseIf left.text = "[" Then
                If leftToken Is Nothing Then
                    ' json vector literal
                    buffer.PopAll()
                    Return exp
                ElseIf leftToken Like GetType(Token) Then
                    Dim tl As Token = leftToken.TryCast(Of Token)

                    If tl = (TokenType.operator, "=") OrElse
                        tl = (TokenType.open, "(") OrElse
                        tl.name = TokenType.sequence OrElse
                        tl = (TokenType.keyword, {"of", "in"}) Then

                        ' create new symbol with initial value
                        Dim index = Traceback(buffer, {TokenType.keyword})

                        exp = New VectorLiteral(ExpressionCollection.GetExpressions(exp.expression))
                        buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Length + 1)
                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                        Reindex(buffer)
                    Else
                        Throw New NotImplementedException
                    End If
                End If
            End If
        End If
    End Function

    Private Iterator Function GetExpressions(lines As Pointer(Of Token)) As IEnumerable(Of SyntaxResult)
        ' find the max stack closed scope
        Do While (i = ++lines) IsNot Nothing
            t = i

            If t.name = TokenType.comment Then
                Continue Do
            End If

            If isNotDelimiter(t) Then
                buffer.Add(New SyntaxToken(buffer.Count, t))
            End If

            If t.name = TokenType.open Then
                stack.Push(t, buffer.Count - 1)
            ElseIf t.name = TokenType.close Then
                Yield PopOut()
            ElseIf t.name = TokenType.comma Then
                If Not stack.isEmpty Then
                    If stack.PeekLast.token.text <> "(" Then
                        ' skip for parse part of value in json literal
                        Continue Do
                    End If
                End If

                Dim index = Traceback(buffer, {TokenType.comma, TokenType.open})
                Dim range = buffer.Skip(index - 1).Take(buffer.Count - index).ToArray
                Dim exp As SyntaxResult

                If range.Last.IsToken(TokenType.close, "}") Then
                    ' usually be a json literal
                    index = Traceback(buffer, {TokenType.open})
                    range = buffer.Skip(index - 1).Take(buffer.Count - index).ToArray
                    exp = range.GetExpression(fromComma:=True, opts)
                Else
                    exp = range.GetExpression(fromComma:=True, opts)
                End If

                If exp.isException OrElse exp.expression Is Nothing Then
                    Continue Do
                End If

                buffer.RemoveRange(index - 1, range.Length)
                buffer.Insert(index - 1, New SyntaxToken(-1, exp.expression))
                Reindex(buffer)

            ElseIf isTerminator(t) Then
                If stack.isEmpty Then
                    ' removes the last terminator
                    buffer.Pop()
                    ' get an expression scope with max stack close range
                    Dim exp = buffer.ToArray.GetExpression(fromComma:=True, opts)

                    If exp Is Nothing OrElse exp.isException Then
                        ' needs add more token into the buffer list
                        ' do no action
                    Else
                        buffer.Clear()
                        Yield exp.expression
                    End If
                Else
                    If t.name = TokenType.newLine Then
                        Dim index = Traceback(buffer, {TokenType.comma, TokenType.open})
                        Dim range = buffer.Skip(index).Take(buffer.Count - index - 1).ToArray
                        Dim exp As SyntaxResult = range.GetExpression(fromComma:=True, opts)

                        ' remove current newline token
                        Call buffer.Pop()

                        If exp Is Nothing OrElse exp.isException OrElse exp.expression Is Nothing Then
                            Continue Do
                        ElseIf exp.expression.expressionName = ExpressionTypes.SequenceLiteral Then
                            ' handling of the syntax parser error for a:b
                            ' which is confused with the json literal in javascript
                            Continue Do
                        End If

                        buffer.RemoveRange(index, range.Length)
                        buffer.Insert(index, New SyntaxToken(-1, exp.expression))
                        buffer.Insert(index + 1, New SyntaxToken(-1, New Token(TokenType.terminator, ";")))
                        Reindex(buffer)
                    End If
                End If
            End If
        Loop

        If buffer > 0 Then
            Yield buffer.PopAll.GetExpression(fromComma:=True, opts)
        End If
    End Function

End Class
