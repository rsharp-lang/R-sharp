Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer

Namespace Language.Implementation

    Module ValueAssignSyntax

        Public Function AssignValue(target As Token(), value As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim valueBlocks = value.SplitByTopLevelDelimiter(TokenType.comma, includeKeyword:=True)
            Dim symbolNames = DeclareNewSymbolSyntax.getNames(target)

            If symbolNames Like GetType(SyntaxErrorException) Then
                Dim targetExpr = opts.ParseExpression(target, opts)

                If Not targetExpr.isException Then
                    Dim valueData = opts.ParseExpression(value, opts)

                    If valueData.isException Then
                        Return valueData
                    End If

                    If TypeOf targetExpr.expression Is SymbolIndexer Then
                        Return New ValueAssignExpression({targetExpr.expression}, valueData.expression)
                    ElseIf TypeOf targetExpr.expression Is FunctionInvoke Then
                        Return New ByRefFunctionCall(
                            invoke:=targetExpr.expression,
                            value:=valueData.expression,
                            stackFrame:=DirectCast(targetExpr.expression, FunctionInvoke).stackFrame
                        )
                    Else

                    End If
                End If

                Return SyntaxResult.CreateError(
                    err:=symbolNames.TryCast(Of SyntaxErrorException),
                    opts:=opts.SetCurrentRange(target)
                )
            End If

            Dim targetSymbols As Literal() = symbolNames _
                .TryCast(Of String()) _
                .Select(Function(name) New Literal(name)) _
                .ToArray

            If valueBlocks > 1 Then
                ' is tuple value
                ' a,b = b,a
                Dim stack As StackFrame = opts.GetStackTrace(target(0), "tuple_assign")
                Dim tuple As New List(Of Expression)
                Dim expr As SyntaxResult
                Dim i As i32 = Scan0

                For Each block As Token() In valueBlocks.Where(Function(b) Not b.isComma)
                    expr = block.ParsePythonLine(opts)

                    If expr.isException Then
                        Return expr
                    Else
                        Call tuple.Add(New ValueAssignExpression(New Expression() {targetSymbols(++i)}, expr.expression))
                    End If
                Next

                Dim list As New FunctionInvoke("list", stack, tuple.ToArray)

                Return New ValueAssignExpression(targetSymbols, list) With {.isByRef = True}
            Else
                Dim valueExpr As SyntaxResult = opts.ParseExpression(value, opts)

                If valueExpr.isException Then
                    Return valueExpr
                Else
                    Return New ValueAssignExpression(targetSymbols, valueExpr.expression) With {.isByRef = True}
                End If
            End If
        End Function

        ''' <summary>
        ''' (xxx,yyy,zzz)
        ''' </summary>
        ''' <param name="value"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        <Extension>
        Public Function TupleParser(value As Token(), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim valueBlocks = value.Skip(1).Take(value.Length - 2).SplitByTopLevelDelimiter(TokenType.comma, includeKeyword:=True)
            Dim stack As StackFrame = opts.GetStackTrace(value(0), "tuple_assign")
            Dim tuple As New List(Of Expression)
            Dim expr As SyntaxResult
            Dim i As i32 = Scan0

            For Each block As Token() In valueBlocks.Where(Function(b) Not b.isComma)
                expr = block.ParsePythonLine(opts)

                If expr.isException Then
                    Return expr
                Else
                    Call tuple.Add(expr.expression)
                End If
            Next

            ' use R# list as python tuple
            Dim list As New FunctionInvoke("list", stack, tuple.ToArray)
            Return list
        End Function
    End Module
End Namespace