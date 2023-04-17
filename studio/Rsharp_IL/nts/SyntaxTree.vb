Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Public Class SyntaxToken

    ''' <summary>
    ''' <see cref="Token"/> or <see cref="Expression"/>
    ''' </summary>
    ''' <returns></returns>
    Public Property value As Object
    Public Property index As Integer

    Sub New(i As Integer, t As Token)
        index = i
        value = t
    End Sub

    Sub New(i As Integer, exp As Expression)
        index = i
        value = exp
    End Sub

    Public Overrides Function ToString() As String
        Return $"<{index}> {value.ToString}"
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function [TryCast](Of T As Class)() As T
        Return TryCast(value, T)
    End Function

    Public Shared Operator Like(t As SyntaxToken, type As Type) As Boolean
        If t Is Nothing OrElse t.value Is Nothing Then
            Return False
        Else
            Return t.value.GetType Is type
        End If
    End Operator

    Public Shared Iterator Function Cast(list As IEnumerable(Of SyntaxToken)) As IEnumerable(Of [Variant](Of Expression, String))
        For Each item In list
            If item Like GetType(Token) Then
                Yield New [Variant](Of Expression, String)(item.TryCast(Of Token).text)
            Else
                Yield New [Variant](Of Expression, String)(item.TryCast(Of Expression))
            End If
        Next
    End Function

End Class

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

                        If left.name = TokenType.open AndAlso left.text = "(" Then
                            Dim leftToken As SyntaxToken = state.Value.Left(buffer)

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
                                    exp = New FunctionInvoke(target.expression, opts.GetStackTrace(t), ExpressionCollecton.GetExpressions(exp))
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
                                    exp = New FunctionInvoke(target, Nothing, ExpressionCollecton.GetExpressions(exp))
                                    buffer.Insert(state.Value.Range.Min, New SyntaxToken(-1, exp))
                                    Reindex(buffer)
                                Else
                                    Throw New NotImplementedException
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

    Private Shared Sub Reindex(ByRef buffer As List(Of SyntaxToken))
        For i As Integer = 0 To buffer.Count - 1
            buffer(i).index = i
        Next
    End Sub

    ''' <summary>
    ''' traceback the index to the last comma or open token
    ''' </summary>
    ''' <returns></returns>
    Private Shared Function Traceback(buffer As List(Of SyntaxToken)) As Integer
        For i As Integer = buffer.Count - 2 To 0 Step -1
            If buffer(i) Like GetType(Token) Then
                Select Case buffer(i).TryCast(Of Token).name
                    Case TokenType.comma, TokenType.open
                        Return i + 1
                    Case Else
                        ' do nothing
                End Select
            End If
        Next

        Return 0
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
