#Region "Microsoft.VisualBasic::b0935213fd8749429f72e31c46e3d5b8, D:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//SyntaxParser/SyntaxTree.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xie (genetics@smrucc.org)
'       xieguigang (xie.guigang@live.com)
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 
' 
' This program is free software: you can redistribute it and/or modify
' it under the terms of the GNU General Public License as published by
' the Free Software Foundation, either version 3 of the License, or
' (at your option) any later version.
' 
' This program is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
' GNU General Public License for more details.
' 
' You should have received a copy of the GNU General Public License
' along with this program. If not, see <http://www.gnu.org/licenses/>.



' /********************************************************************************/

' Summaries:


' Code Statistics:

'   Total Lines: 405
'    Code Lines: 295
' Comment Lines: 51
'   Blank Lines: 59
'     File Size: 16.99 KB


' Class SyntaxTree
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: GetExpressions, ParseFuncInvoke, ParseTsScript, PopOut, PopOutCallerStack
'               PopOutClosureStack, PopOutStack, PopOutVectorStack
' 
' /********************************************************************************/

#End Region

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
            buffer.RemoveRange(state.Value.Range.Min + 1, state.Value.Range.Interval - 1)
            buffer.Insert(state.Value.Range.Min + 1, New SyntaxToken(-1, exp.expression))
            Reindex(buffer)

            Return Nothing
        ElseIf lt.isAnyKeyword("require") Then
            ' javascript require
            buffer.RemoveRange(state.Value.Range.Min - 1, state.Value.Range.Interval + 2)
            exp = New Require(ExpressionCollection.GetExpressions(exp.expression))
            buffer.Insert(state.Value.Range.Min - 1, New SyntaxToken(-1, exp.expression))
            Reindex(buffer)

            Return Nothing
        End If

        Dim target = Expression.CreateExpression({lt}, opts)

        If target Like GetType(SymbolReference) Then
            ' invoke function
            ' func(...)
            buffer.RemoveRange(state.Value.Range.Min - 1, state.Value.Range.Interval + 2)
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
        Dim range As SyntaxToken() = state.Value.GetRange(buffer).ToArray
        Dim exp = range.GetExpression(fromComma:=False, opts)

        If exp.isException Then
            If state.Value.Range.Min > 0 Then
                ' try to deal with a special syntax
                '
                ' import {...,...,...} from 'xxx'
                '
                If range(0).IsToken(TokenType.open, "{") AndAlso range.Last.IsToken(TokenType.close, "}") Then
                    ' is vector
                    If (buffer(state.Value.Range.Min - 1).IsToken(TokenType.keyword, "import")) Then
                        range(0) = New SyntaxToken(-1, New Token(TokenType.open, "[") With {.span = range(0).TryCast(Of Token).span})
                        range(range.Length - 1) = New SyntaxToken(-1, New Token(TokenType.close, "]") With {.span = range.Last.TryCast(Of Token).span})
                        exp = range.GetExpression(fromComma:=True, opts)

                        buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Interval + 1)
                        buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, ExpressionCollection.GetExpressions(exp.expression).First))
                        Reindex(buffer)

                        Return Nothing
                    End If
                End If
            End If

            Return exp
        End If

        If Not range.First Like GetType(Token) Then
            Return Nothing
        End If

        Dim left = range.First.TryCast(Of Token)
        Dim leftToken As SyntaxToken = state.Value.Left(buffer)

        If left.name <> TokenType.open Then
            Return Nothing
        End If

        If left.text = "(" Then
            Return PopOutCallerStack(leftToken, exp)
        ElseIf left.text = "{" Then
            Return PopOutClosureStack(leftToken, exp)
        ElseIf left.text = "[" Then
            Return PopOutVectorStack(leftToken, exp)
        End If

        Return Nothing
    End Function

    Private Function PopOutCallerStack(leftToken As SyntaxToken, exp As SyntaxResult) As SyntaxResult
        If leftToken Is Nothing Then
            ' (...)
            state.Value.RemoveRange(buffer)
            buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
        ElseIf leftToken Like GetType(Token) Then
            If leftToken.IsToken(TokenType.operator) Then
                ' operator for binary expression, example like:
                ' 1 / (...)
                buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Interval + 1)
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

        Return Nothing
    End Function

    Private Function PopOutClosureStack(leftToken As SyntaxToken, exp As SyntaxResult) As SyntaxResult
        If leftToken Is Nothing Then
            ' is a multiple line closure expression
            ' {...}
            state.Value.RemoveRange(buffer)
            buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
        ElseIf leftToken Like GetType(Token) Then
            If leftToken.TryCast(Of Token) = (TokenType.close, ")") Then
                ' is a possible function declare
                Dim index = Traceback(buffer, {TokenType.keyword})

                buffer.RemoveRange(state.Value.Range.Min + 1, state.Value.Range.Interval - 1)
                buffer.Insert(state.Value.Range.Min + 1, New SyntaxToken(-1, exp.expression))
                Reindex(buffer)

                Dim range As SyntaxToken() = buffer _
                    .Skip(index - 1) _
                    .Take(buffer.Count - index + 1) _
                    .ToArray

                exp = range.GetExpression(fromComma:=True, opts)

                If exp Is Nothing OrElse exp.isException Then
                    Return exp
                End If

                buffer.RemoveRange(index - 1, range.Length)
                buffer.Insert(index - 1, New SyntaxToken(-1, exp.expression))
                Reindex(buffer)
            ElseIf leftToken.TryCast(Of Token) = (TokenType.sequence, ":") Then
                ' is json value
                buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Interval + 1)
                buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                Reindex(buffer)
            ElseIf leftToken.TryCast(Of Token) = (TokenType.open, "[") Then
                ' json array [{...}]
                buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Interval + 1)
                buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                Reindex(buffer)
            ElseIf leftToken.TryCast(Of Token) = (TokenType.open, "(") Then
                buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Interval + 1)
                buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                Reindex(buffer)
            ElseIf leftToken.IsToken(TokenType.keyword) Then
                ' else {}
                ' else if {}
                ' try {}
                buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Interval + 1)
                buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                Reindex(buffer)
            ElseIf leftToken.isComma Then
                ' last element in json literal
                ' 
                ' {...,...}
                buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Interval + 1)
                buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                Reindex(buffer)
            End If
        Else
            Return Nothing
        End If

        Return Nothing
    End Function

    Private Function PopOutVectorStack(leftToken As SyntaxToken, exp As SyntaxResult) As SyntaxResult
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
                buffer.RemoveRange(state.Value.Range.Min, state.Value.Range.Interval + 1)
                buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp.expression))
                Reindex(buffer)
            ElseIf tl.name = TokenType.identifier OrElse tl.name = TokenType.keyword Then
                ' x['xxx'] symbol indexer
                Dim symbol As New SymbolReference(tl)
                Dim index As Expression
                Dim col As ExpressionCollection = exp.expression

                If col.expressions.Length > 1 Then
                    index = New VectorLiteral(col.expressions)
                Else
                    index = col.expressions.First
                End If

                Dim indexer As New SymbolIndexer(symbol, index)

                buffer.RemoveRange(state.Value.Range.Min - 1, state.Value.Range.Interval + 2)
                buffer.Insert(state.Value.Range.Min - 1, New SyntaxToken(-1, indexer))
                Reindex(buffer)
            Else
                Throw New NotImplementedException
            End If
        End If

        Return Nothing
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
