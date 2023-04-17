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
Imports Microsoft.VisualBasic.Emit.Marshal

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
                    Dim exp = range.GetExpression(opts)

                    Yield exp
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
