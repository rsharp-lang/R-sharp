Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Language.CodeDom
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Language

    Public Class SyntaxTree

        ReadOnly script As Rscript
        ReadOnly debug As Boolean = False
        ReadOnly scanner As JlScanner
        ReadOnly opts As SyntaxBuilderOptions
        ReadOnly stack As New Stack(Of PythonCodeDOM)
        ReadOnly julia As New PythonCodeDOM With {
            .keyword = "julia",
            .level = -1,
            .script = New List(Of Expression)
        }

        ''' <summary>
        ''' current python code dom node
        ''' </summary>
        Dim current As PythonCodeDOM

        <DebuggerStepThrough>
        Sub New(script As Rscript, Optional debug As Boolean = False)
            Me.debug = debug
            Me.script = script
            Me.scanner = New JlScanner(script.script)
            Me.opts = New SyntaxBuilderOptions(AddressOf ParseJuliaLine) With {
                .source = script,
                .debug = debug
            }
        End Sub

        Private Function getLines(tokens As IEnumerable(Of Token)) As IEnumerable(Of TokenLine)
            Dim allTokens As Token() = tokens.ToArray
            Dim lineTokens = allTokens _
                .Where(Function(t) t.name <> TokenType.comment) _
                .Split(Function(t) t.name = TokenType.newLine) _
                .Where(Function(l) l.Length > 0) _
                .ToArray
            Dim lines = lineTokens _
                .Select(Function(t) New TokenLine(t).StripDelimiterTokens()) _
                .ToArray

            Return From line As TokenLine In lines Where line.length > 0
        End Function

        Private Sub startFunctionDefine(line As TokenLine)
            Dim args As New List(Of DeclareNewSymbol)
            Dim tokens As Token() = line.tokens.Skip(3).Take(line.tokens.Length - 5).ToArray
            Dim result = DeclareNewFunctionSyntax.getParameters(tokens, args, opts)

            current = New FunctionTag With {
                .keyword = line(Scan0).text,
                .script = New List(Of Expression),
                .funcName = line(1).text,
                .arguments = args,
                .stackframe = opts.GetStackTrace(line(1))
            }

            Call stack.Push(current)
        End Sub

        Public Function ParseJlScript() As Program
            current = julia

            For Each line As TokenLine In getLines(scanner.GetTokens)
                If line(Scan0).name = TokenType.keyword Then
                    Select Case line(Scan0).text
                        Case "function" : Call startFunctionDefine(line)

                        Case Else
                            Throw New NotImplementedException(line.ToString)
                    End Select
                Else
                    Dim expr = opts.ParseExpression(line.tokens, opts)

                    If expr.isException Then
                        Throw New InvalidProgramException
                    End If

                    Call current.Add(expr)
                End If
            Next

            Return New Program(julia.script)
        End Function
    End Class
End Namespace