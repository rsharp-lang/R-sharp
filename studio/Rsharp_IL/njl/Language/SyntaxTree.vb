#Region "Microsoft.VisualBasic::8370c8e5721919306cd9b686868b1c2f, R-sharp\studio\njl\Language\SyntaxTree.vb"

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

'   Total Lines: 297
'    Code Lines: 231
' Comment Lines: 16
'   Blank Lines: 50
'     File Size: 11.46 KB


'     Class SyntaxTree
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: getLines, ParseJlScript
' 
'         Sub: endCurrent, importModule, includeFile, requirePackages, startAcceptorDefine
'              (+2 Overloads) startClosureDefine, startForLoopDefine, startFunctionDefine, startIfDefine, startUsingDefine
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Language.CodeDom
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser.SyntaxImplements
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
            Me.opts = New SyntaxBuilderOptions(AddressOf ParseJuliaLine, Function(c, s) New JlScanner(c, s)) With {
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
            Dim tokens As Token() = line.tokens.Skip(3).Take(line.tokens.Length - 4).ToArray
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

        Private Sub startForLoopDefine(line As TokenLine)
            Dim tokens As Token() = line.tokens.Skip(1).Take(line.tokens.Length - 1).ToArray

            If tokens(Scan0) = (TokenType.open, "(") AndAlso tokens.Last = (TokenType.close, ")") Then
                tokens = tokens _
                    .Skip(1) _
                    .Take(tokens.Length - 2) _
                    .ToArray
            End If

            Dim data = tokens.SplitByTopLevelDelimiter(TokenType.operator, False, tokenText:="=").ToArray
            Dim vars = data(0) _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Select(Function(t)
                            Return ParseJuliaLine(t, opts).expression
                        End Function) _
                .ToArray
            Dim seqs = ParseJuliaLine(data(2), opts).expression

            current = New ForTag With {
                .data = seqs,
                .stackFrame = opts.GetStackTrace(line(0)),
                .keyword = "for",
                .script = New List(Of Expression),
                .vars = vars
            }

            stack.Push(current)
        End Sub

        Private Sub startAcceptorDefine(line As TokenLine)
            Dim expr = opts.ParseExpression(line.tokens.Take(line.length - 1), opts)

            If expr.isException Then
                Throw New InvalidProgramException
            End If

            current = New AcceptorTag With {
                .keyword = "call",
                .script = New List(Of Expression),
                .target = expr.expression,
                .level = 0
            }

            stack.Push(current)
        End Sub

        Public Sub endCurrent()
            Dim popOut As Boolean = False

            If stack.Peek Is current AndAlso Not current Is julia Then
                popOut = True
                stack.Pop()
            End If

            stack.Peek.Add(current.ToExpression)

            If Not popOut Then
                current = stack.Pop
            Else
                current = stack.Peek
            End If

            If stack.Count = 1 AndAlso stack.Peek Is julia Then
                ' julia.Add(current.ToExpression)
                current = julia
            End If
        End Sub

        Private Sub requirePackages(line As TokenLine)
            Dim pkgNames = line.tokens _
                                .Skip(1) _
                                .SplitByTopLevelDelimiter(TokenType.comma) _
                                .Where(Function(t)
                                           Return Not (t.Length = 1 AndAlso t(0).name = TokenType.comma)
                                       End Function) _
                                .Select(Function(t) opts.ParseExpression(t, opts)) _
                                .ToArray
            Dim require As New Require(pkgNames.Select(Function(name) name.expression))

            Call current.Add(require)
        End Sub

        Private Sub importModule(line As TokenLine)
            Dim nameRef = opts.ParseExpression(line.tokens.Skip(1), opts)
            Dim nameStr As SymbolReference = nameRef.expression
            Dim names As String() = nameStr.symbol.Split("."c)
            Dim moduleName As String = names(0)
            Dim pkgName As String = names(1)
            Dim import As New [Imports](pkgName, moduleName)

            Call current.Add(import)
        End Sub

        Private Sub includeFile(line As TokenLine)
            Dim script = opts.ParseExpression(line.tokens.Skip(1), opts)
            Dim import As New [Imports](Nothing, script.expression, opts.source.source)

            Call current.Add(import)
        End Sub

        ''' <summary>
        ''' begin ... end
        ''' </summary>
        ''' <param name="line"></param>
        Private Sub startClosureDefine(line As TokenLine)
            current = New ClosureTag With {
                .keyword = "begin",
                .script = New List(Of Expression),
                .level = 0
            }

            stack.Push(current)
        End Sub

        ''' <summary>
        ''' x = begin ... end
        ''' </summary>
        ''' <param name="target"></param>
        Private Sub startClosureDefine(target As Expression())
            current = New ClosureTag With {
                .keyword = "begin",
                .assignTarget = target,
                .level = 0,
                .script = New List(Of Expression)
            }

            stack.Push(current)
        End Sub

        Private Sub startIfDefine(line As TokenLine)
            Dim test = opts.ParseExpression(line.tokens.Skip(1), opts)

            If test.isException Then
                Throw New NotImplementedException
            End If

            current = New IfTag With {
                .keyword = "if",
                .level = 0,
                .script = New List(Of Expression),
                .stackframe = opts.GetStackTrace(line(Scan0)),
                .test = test.expression
            }

            stack.Push(current)
        End Sub

        Private Sub startUsingDefine(line As TokenLine)
            Dim arg = line(-1)
            Dim auto = line.tokens.Take(line.length - 2).ToArray
            Dim autoExpr = opts.ParseExpression(auto, opts)

            If autoExpr.isException Then
                Throw New NotImplementedException
            End If

            current = New UsingTag With {
                .auto = autoExpr.expression,
                .keyword = "using",
                .level = 0,
                .script = New List(Of Expression),
                .symbol = arg.text,
                .sourceMap = opts.GetStackTrace(line(Scan0))
            }

            stack.Push(current)
        End Sub

        Public Function ParseJlScript() As Program
            current = julia
            stack.Push(julia)

            For Each line As TokenLine In getLines(scanner.GetTokens)
                If line(Scan0).name = TokenType.keyword Then
                    Select Case line(Scan0).text
                        Case "function" : Call startFunctionDefine(line)
                        Case "for" : Call startForLoopDefine(line)
                        Case "end" : Call endCurrent()
                        Case "using" : Call requirePackages(line)
                        Case "import" : Call importModule(line)
                        Case "include" : Call includeFile(line)
                        Case "begin" : Call startClosureDefine(line)
                        Case "if" : Call startIfDefine(line)
                        Case "return"

                            Call current.Add(New ReturnValue(opts.ParseExpression(line.tokens.Skip(1), opts).expression))

                        Case Else
                            Throw New NotImplementedException(line.ToString)
                    End Select
                ElseIf line(-1) = (TokenType.sequence, ":") Then
                    Call startAcceptorDefine(line)
                ElseIf line.length > 2 AndAlso line(-1).name = TokenType.identifier AndAlso line(-2) = (TokenType.keyword, "do") Then
                    ' xxx do x
                    ' using x as xxx {
                    '    ...
                    ' }
                    Call startUsingDefine(line)
                Else
                    Dim expr = opts.ParseExpression(line.tokens, opts)

                    If expr.isException Then
                        Throw New InvalidProgramException
                    ElseIf expr.expression.expressionName = ExpressionTypes.SymbolAssign Then
                        Dim assign As ValueAssignExpression = DirectCast(expr.expression, ValueAssignExpression)

                        If TypeOf assign.value Is SymbolReference AndAlso DirectCast(assign.value, SymbolReference).symbol = "begin" Then
                            Call startClosureDefine(assign.targetSymbols)
                            Continue For
                        End If
                    End If

                    Call current.Add(expr)
                End If
            Next

            Return New Program(julia.script)
        End Function
    End Class
End Namespace
