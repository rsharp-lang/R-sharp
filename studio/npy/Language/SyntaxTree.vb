#Region "Microsoft.VisualBasic::915ee3a89c3c790a471667709dbe2a38, R-sharp\studio\npy\Language\SyntaxTree.vb"

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

    '   Total Lines: 385
    '    Code Lines: 312
    ' Comment Lines: 19
    '   Blank Lines: 54
    '     File Size: 14.31 KB


    ' Class SyntaxTree
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: getLines, ParsePyScript
    ' 
    '     Sub: addLine, addPkgImport, addPkgLoad, addValueReturn, createError
    '          pushBlock, startAcceptorDefine, startElseDefine, startForLoopDefine, startFunctionDefine
    '          startIfDefine
    ' 
    ' /********************************************************************************/

#End Region

#If netcore5 = 1 Then
Imports System.Data
#End If
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Language.CodeDom
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

    <DebuggerStepThrough>
    Sub New(script As Rscript, Optional debug As Boolean = False)
        Me.debug = debug
        Me.script = script
        Me.scanner = New PyScanner(script.script)
        Me.opts = New SyntaxBuilderOptions(AddressOf ParsePythonLine, Function(c, s) New PyScanner(c, s)) With {
            .source = script,
            .debug = debug,
            .pipelineSymbols = {"."}
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
        If stack.Count = 0 Then
            stack.Push(python)
        End If

        If line.levels > current.level Then
            If current Is python OrElse current Is stack.Peek Then
                ' do nothing
            Else
                Call stack.Push(current)
            End If
        ElseIf line.levels = current.level Then
            If stack.Peek Is current Then
                stack.Pop()
            End If

            ' 结束了上一个block
            Call stack.Peek.Add(current.ToExpression())
        Else
            stack.Peek.Add(current.ToExpression)

            ' line.levels < current.level
            Do While stack.Peek.level >= [next].level
                current = stack.Pop
                stack.Peek.Add(current.ToExpression)
            Loop
        End If

        current = [next]
    End Sub

    Private Sub startFunctionDefine(line As PythonLine)
        Dim args As New List(Of DeclareNewSymbol)
        Dim tokens As Token() = line.tokens _
            .Skip(3) _
            .Take(line.tokens.Length - 5) _
            .ToArray
        Dim result = DeclareNewFunctionSyntax.getParameters(tokens, args, opts)

        Call pushBlock(line, [next]:=New FunctionTag With {
            .keyword = line(Scan0).text,
            .level = line.levels,
            .script = New List(Of Expression),
            .funcName = line(1).text,
            .arguments = args,
            .stackframe = opts.GetStackTrace(line(1))
        })
    End Sub

    Private Sub startForLoopDefine(line As PythonLine)
        Dim tokens As Token() = line.tokens _
            .Skip(1) _
            .Take(line.tokens.Length - 2) _
            .ToArray

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
            Throw New NotImplementedException("invalid package import statement: " & line.ToString)
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
        If current.script > 0 AndAlso TypeOf expr Is FunctionInvoke AndAlso DirectCast(DirectCast(expr, FunctionInvoke).funcName, Literal).ValueStr.StartsWith(".") Then
            Dim invoke As FunctionInvoke = DirectCast(expr, FunctionInvoke)
            invoke.parameters = {current.script.Last}.JoinIterates(invoke.parameters).ToArray
            DirectCast(invoke.funcName, Literal).value = DirectCast(invoke.funcName, Literal).ValueStr.TrimStart("."c)
            current.script.RemoveLast
            current.script.Add(invoke)
            Return
        End If

        If lineLevels > current.level Then
            If current.keyword.StringEmpty Then
                Throw New SyntaxErrorException
            Else
                Call current.Add(expr)
            End If
        ElseIf lineLevels <= current.level Then
            If stack.Count = 0 Then
                stack.Push(python)
            End If

            If stack.Count = 1 Then
                ' 结束当前的对象
                Call stack.Peek.Add(current.ToExpression())
            ElseIf stack.Peek Is current Then
                ' 结束当前的对象
                Call stack.Pop()
                Call stack.Peek.Add(current.ToExpression())
            Else
                Call stack.Peek.Add(current.ToExpression())
                current = stack.Pop()
                current.Add(expr)
                stack.Peek.Add(current.ToExpression)

                Return
            End If

            current = stack.Peek
            current.Add(expr)
        End If
    End Sub

    ReadOnly reserved As Index(Of String) = {
        "if", "for", "def", "class"
    }

    Public Function ParsePyScript() As Program
        current = python

        For Each line As PythonLine In getLines(scanner.GetTokens)
            If line.length > 1 AndAlso line(Scan0).name = TokenType.keyword AndAlso line(1).name = TokenType.operator Then
                If Not line(Scan0).text Like reserved Then
                    line(Scan0).name = TokenType.identifier
                End If
            End If

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

        If stack.Count > 0 AndAlso Not current Is stack.Peek Then
            stack.Push(current)
        End If

        Do While stack.Count > 0 AndAlso Not current Is python
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
