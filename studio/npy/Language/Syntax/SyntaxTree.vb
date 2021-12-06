Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Public Module SyntaxTree

    <Extension>
    Public Function ParsePyScript(script As Rscript, Optional debug As Boolean = False) As Program
        Dim scanner As New PyScanner(script.script)
        Dim opts As New SyntaxBuilderOptions With {
            .source = script,
            .debug = debug
        }
        Dim tokens As Token() = scanner.GetTokens.ToArray
        Dim lines As PythonLine() = tokens _
            .Where(Function(t) t.name <> TokenType.comment) _
            .Split(Function(t) t.name = TokenType.newLine) _
            .Where(Function(l) l.Length > 0) _
            .Select(Function(t) New PythonLine(t)) _
            .Where(Function(l) l.tokens.Length > 0) _
            .ToArray
        Dim stack As New Stack(Of PythonCodeDOM)
        Dim result As SyntaxResult
        Dim python As New PythonCodeDOM With {
            .keyword = "python",
            .level = -1,
            .script = New List(Of Expression)
        }
        Dim current As PythonCodeDOM = python
        Dim released As New Index(Of String)

        For Each line As PythonLine In lines
            ' 每一行前面的空格数量作为层级关系
            If line(Scan0).name = TokenType.keyword Then
                Select Case line(Scan0).text
                    Case "def"
                        Dim args As New List(Of DeclareNewSymbol)

                        tokens = line.tokens.Skip(3).Take(line.tokens.Length - 5).ToArray
                        result = DeclareNewFunctionSyntax.getParameters(tokens, args, opts)

                        If line.levels > current.level Then
                            stack.Push(current)
                        ElseIf line.levels = current.level Then
                            ' 结束了上一个block
                            stack.Peek.Add(current.ToExpression(released))
                        End If

                        current = New FunctionTag With {
                           .keyword = line(Scan0).text,
                           .level = line.levels,
                           .script = New List(Of Expression),
                           .funcname = line(1).text,
                           .arguments = args,
                           .stackframe = opts.GetStackTrace(line(1))
                        }

                    Case "for"

                        tokens = line.tokens.Skip(1).Take(line.tokens.Length - 2).ToArray

                        Dim data = tokens.SplitByTopLevelDelimiter(TokenType.keyword, False, tokenText:="in").ToArray
                        Dim vars = data(0).SplitByTopLevelDelimiter(TokenType.comma).Select(Function(t) Expression.CreateExpression(t, opts).expression).ToArray
                        Dim seqs = Expression.CreateExpression(data(2), opts).expression

                        If line.levels > current.level Then
                            stack.Push(current)
                        ElseIf line.levels = current.level Then
                            ' 结束了上一个block
                            stack.Peek.Add(current.ToExpression(released))
                        End If

                        current = New ForTag With {
                            .data = seqs,
                            .stackFrame = opts.GetStackTrace(line(0)),
                            .keyword = "for",
                            .level = line.levels,
                            .script = New List(Of Expression),
                            .vars = vars
                        }

                    Case "return"

                        tokens = line.tokens.Skip(1).ToArray
                        result = Expression.CreateExpression(tokens, opts)

                        If result.isException Then
                            Throw result.error.exception
                        Else
                            current.Add(New ReturnValue(result.expression))
                        End If

                    Case "import"

                        tokens = line.tokens.Skip(1).ToArray

                        Dim names As Expression() = tokens _
                            .Split(Function(t) t.name = TokenType.comma) _
                            .Select(Function(block)
                                        Return Expression.CreateExpression(block, opts).expression
                                    End Function) _
                            .ToArray

                        current.Add(New [Imports](Nothing, New VectorLiteral(names), source:=script.source))

                    Case "if"

                        tokens = line.tokens.Skip(1).Take(line.tokens.Length - 3).ToArray

                        Dim test As SyntaxResult = Expression.CreateExpression(tokens, opts)

                        If line.levels > current.level Then
                            stack.Push(current)
                        ElseIf line.levels = current.level Then
                            ' 结束了上一个block
                            stack.Peek.Add(current.ToExpression(released))
                        End If

                        current = New IfTag With {
                           .keyword = line(Scan0).text,
                           .level = line.levels,
                           .script = New List(Of Expression),
                           .test = test.expression,
                           .stackframe = opts.GetStackTrace(line(1))
                        }

                    Case Else
                        Throw New NotImplementedException
                End Select
            ElseIf line.levels > current.level Then
                If current.keyword.StringEmpty Then
                    Throw New SyntaxErrorException
                End If

                result = ParsePythonLine(line, opts)

                If result.isException Then
                    Throw result.error.exception
                Else
                    current.Add(result)
                End If
            ElseIf line.levels <= current.level Then
                ' 结束当前的对象
                stack.Peek.Add(current.ToExpression(released))
                current = stack.Peek
                result = ParsePythonLine(line, opts)

                If result.isException Then
                    Throw result.error.exception
                Else
                    current.Add(result)
                End If
            End If
        Next

        ' do release of stack
        If current.level >= 0 AndAlso Not current.GetHashCode.ToHexString Like released Then
            stack.Peek.Add(current.ToExpression(released))
        End If

        Do While stack.Count > 0
            current = stack.Pop

            If stack.Count > 0 Then
                stack.Peek.Add(current.ToExpression(released))
            Else
                Exit Do
            End If
        Loop

        Return New Program(python.script)
    End Function

    Private Function ParsePythonLine(line As PythonLine, opts As SyntaxBuilderOptions) As SyntaxResult
        Return Expression.CreateExpression(line.tokens, opts)
    End Function

End Module


