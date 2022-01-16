#If netcore5 = 1 Then
Imports System.Data
#End If
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports Rscript = SMRUCC.Rsharp.Runtime.Components.Rscript

Public Class SyntaxTree

    ReadOnly script As Rscript
    ReadOnly debug As Boolean = False
    ReadOnly scanner As PyScanner
    ReadOnly opts As SyntaxBuilderOptions
    ReadOnly stack As New Stack(Of PythonCodeDOM)
    ReadOnly python As New PythonCodeDOM With {
        .keyword = "python",
        .level = -1,
        .script = New List(Of Expression)
    }

    ''' <summary>
    ''' current python code dom node
    ''' </summary>
    Dim current As PythonCodeDOM

    Sub New(script As Rscript, Optional debug As Boolean = False)
        Me.debug = debug
        Me.script = script
        Me.scanner = New PyScanner(script.script)
        Me.opts = New SyntaxBuilderOptions(AddressOf ParsePythonLine) With {
            .source = script,
            .debug = debug
        }
    End Sub

    Private Function getLines(tokens As IEnumerable(Of Token)) As IEnumerable(Of PythonLine)
        Dim allTokens As Token() = tokens.ToArray
        Dim lineTokens = allTokens _
            .Where(Function(t) t.name <> TokenType.comment) _
            .Split(Function(t) t.name = TokenType.newLine) _
            .Where(Function(l) l.Length > 0) _
            .ToArray
        Dim lines = lineTokens _
            .Select(Function(t) New PythonLine(t)) _
            .ToArray

        Return From line As PythonLine In lines Where line.length > 0
    End Function

    ''' <summary>
    ''' try push <see cref="current"/> python code dom node into stack
    ''' </summary>
    ''' <param name="line"></param>
    Private Sub pushBlock(line As PythonLine, [next] As PythonCodeDOM)
        If line.levels > current.level Then
            Call stack.Push(current)
        ElseIf line.levels = current.level Then
            ' 结束了上一个block
            Call stack.Peek.Add(current.ToExpression())
        End If

        current = [next]
    End Sub

    Private Sub startFunctionDefine(line As PythonLine)
        Dim args As New List(Of DeclareNewSymbol)
        Dim tokens As Token() = line.tokens.Skip(3).Take(line.tokens.Length - 5).ToArray
        Dim result = DeclareNewFunctionSyntax.getParameters(tokens, args, opts)

        Call pushBlock(line, [next]:=New FunctionTag With {
           .keyword = line(Scan0).text,
           .level = line.levels,
           .script = New List(Of Expression),
           .funcname = line(1).text,
           .arguments = args,
           .stackframe = opts.GetStackTrace(line(1))
        })
    End Sub

    Private Sub startForLoopDefine(line As PythonLine)
        Dim tokens As Token() = line.tokens.Skip(1).Take(line.tokens.Length - 2).ToArray

        If tokens(Scan0) = (TokenType.open, "(") AndAlso tokens.Last = (TokenType.close, ")") Then
            tokens = tokens _
                .Skip(1) _
                .Take(tokens.Length - 2) _
                .ToArray
        End If

        Dim data = tokens.SplitByTopLevelDelimiter(TokenType.keyword, False, tokenText:="in").ToArray
        Dim vars = data(0) _
            .SplitByTopLevelDelimiter(TokenType.comma) _
            .Select(Function(t)
                        Return ParsePythonLine(t, opts).expression
                    End Function) _
            .ToArray
        Dim seqs = ParsePythonLine(data(2), opts).expression

        Call pushBlock(line, [next]:=New ForTag With {
            .data = seqs,
            .stackFrame = opts.GetStackTrace(line(0)),
            .keyword = "for",
            .level = line.levels,
            .script = New List(Of Expression),
            .vars = vars
        })
    End Sub

    Private Sub addValueReturn(line As PythonLine)
        Dim tokens = line.tokens.Skip(1).ToArray
        Dim result = ParsePythonLine(tokens, opts)

        If result.isException Then
            Throw result.error.exception
        Else
            Call addLine(line.levels, New ReturnValue(result.expression))
        End If
    End Sub

    Private Sub addPkgImport(line As PythonLine)
        Dim package As Token = line.tokens(1)

        If line.tokens(2) <> (TokenType.keyword, "import") Then
            Throw New NotImplementedException
        End If

        ' from ... import ...
        Dim tokens = line.tokens.Skip(3).ToArray
        Dim pkgName = ParsePythonLine({package}, opts)
        Dim list As Expression() = tokens _
            .SplitByTopLevelDelimiter(TokenType.comma, includeKeyword:=True) _
            .Where(Function(r)
                       Return Not (r.Length = 1 AndAlso r(Scan0).name = TokenType.comma)
                   End Function) _
            .Select(Function(t)
                        Dim expr = ParsePythonLine(t, opts).expression

                        If TypeOf expr Is SymbolReference Then
                            expr = New Literal(DirectCast(expr, SymbolReference).symbol)
                        End If

                        Return expr
                    End Function) _
            .ToArray
        Dim libname As New Literal(ValueAssignExpression.GetSymbol(pkgName.expression))
        Dim importPkgs As New [Imports](New VectorLiteral(list), libname, source:=script.source)

        Call addLine(line.levels, importPkgs)
    End Sub

    Private Sub addPkgLoad(line As PythonLine)
        Dim tokens As Token() = line.tokens.Skip(1).ToArray
        Dim names As Expression() = tokens _
            .Split(Function(t) t.name = TokenType.comma) _
            .Select(Function(block)
                        Dim expr As Expression = ParsePythonLine(block, opts).expression

                        If TypeOf expr Is SymbolReference Then
                            expr = New Literal(DirectCast(expr, SymbolReference).symbol)
                        End If

                        Return expr
                    End Function) _
            .ToArray

        For Each name As Expression In names
            If TypeOf name Is Literal Then
                Dim modulefile As String = DirectCast(name, Literal).ValueStr
                Dim testpath As String = $"{script.GetSourceDirectory}/{modulefile}"

                If testpath.FileExists Then
                    Call addLine(line.levels, New [Imports](Nothing, New VectorLiteral({name}), source:=script.source))
                Else
                    Call addLine(line.levels, New Require(modulefile))
                End If
            Else
                Call addLine(line.levels, New [Imports](Nothing, New VectorLiteral({name}), source:=script.source))
            End If
        Next
    End Sub

    Private Sub startIfDefine(line As PythonLine)
        Dim tokens As Token() = line.tokens _
            .Skip(1) _
            .Take(line.tokens.Length - 2) _
            .ToArray
        Dim test As SyntaxResult = ParsePythonLine(tokens, opts)

        Call pushBlock(line, [next]:=New IfTag With {
            .keyword = line(Scan0).text,
            .level = line.levels,
            .script = New List(Of Expression),
            .test = test.expression,
            .stackframe = opts.GetStackTrace(line(1))
        })
    End Sub

    Private Sub startElseDefine(line As PythonLine)
        Call pushBlock(line, [next]:=New ElseTag With {
            .keyword = "else",
            .level = line.levels,
            .script = New List(Of Expression),
            .stackframe = opts.GetStackTrace(line(Scan0))
        })
    End Sub

    Private Sub startAcceptorDefine(line As PythonLine)
        ' 20220112 acceptor syntax
        '
        ' func(...):
        '    line1
        '    line2
        Dim tokens = line.tokens.Take(line.length - 1).ToArray
        Dim result = ParsePythonLine(tokens, opts)

        If result.isException Then
            Throw result.error.exception
        ElseIf Not result Like GetType(FunctionInvoke) Then
            Throw New InvalidExpressionException
        End If

        Call pushBlock(line, [next]:=New AcceptorTag With {
            .keyword = "calls",
            .level = line.levels,
            .script = New List(Of Expression),
            .target = DirectCast(result.expression, FunctionInvoke)
        })
    End Sub

    Public Sub createError(line As PythonLine)
        Dim result = ParsePythonLine(line.tokens.Skip(1), opts)

        If result.isException Then
            Throw result.error.exception
        Else
            Dim [stop] As New FunctionInvoke(
                funcName:="stop",
                stackFrame:=opts.GetStackTrace(line(Scan0)),
                result.expression
            )

            Call addLine(line.levels, [stop])
        End If
    End Sub

    Private Sub addLine(lineLevels As Integer, expr As Expression)
        If lineLevels > current.level Then
            If current.keyword.StringEmpty Then
                Throw New SyntaxErrorException
            Else
                Call current.Add(expr)
            End If
        ElseIf lineLevels <= current.level Then
            If stack.Count = 1 Then
                ' 结束当前的对象
                Call stack.Peek.Add(current.ToExpression())
            ElseIf stack.Peek Is current Then
                ' 结束当前的对象
                Call stack.Pop()
                Call stack.Peek.Add(current.ToExpression())
            Else
                Call stack.Peek.Add(current.ToExpression())
            End If

            current = stack.Peek
            current.Add(expr)
        End If
    End Sub

    Public Function ParsePyScript() As Program
        current = python

        For Each line As PythonLine In getLines(scanner.GetTokens)
            ' 每一行前面的空格数量作为层级关系
            If line(Scan0).name = TokenType.keyword Then
                Select Case line(Scan0).text
                    Case "def" : Call startFunctionDefine(line)
                    Case "for" : Call startForLoopDefine(line)
                    Case "return" : Call addValueReturn(line)
                    Case "from" : Call addPkgImport(line)
                    Case "import" : Call addPkgLoad(line)
                    Case "if" : Call startIfDefine(line)
                    Case "else" : Call startElseDefine(line)
                    Case "raise" : Call createError(line)

                    Case Else
                        Throw New NotImplementedException
                End Select
            ElseIf line(-1).name = TokenType.sequence Then
                Call startAcceptorDefine(line)
            Else
                Dim result = ParsePythonLine(line.tokens, opts)

                If result.isException Then
                    Throw result.error.exception
                Else
                    Call addLine(line.levels, result.expression)
                End If
            End If
        Next

        Do While stack.Count > 0
            current = stack.Pop

            If stack.Count > 0 Then
                Call stack.Peek.Add(current.ToExpression())
            Else
                Exit Do
            End If
        Loop

        Return New Program(python.script)
    End Function

End Class


