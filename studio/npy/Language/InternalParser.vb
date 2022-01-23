#If netcore5 = 1 Then
#End If
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Python.Language
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports Rscript = SMRUCC.Rsharp.Runtime.Components.Rscript

Public Module InternalParser

    <Extension>
    Public Function ParsePyScript(script As Rscript, Optional debug As Boolean = False) As Program
        Return New SyntaxTree(script, debug).ParsePyScript()
    End Function

    <Extension>
    Friend Function ParsePythonLine(line As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
        Dim blocks = line.SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True)
        Dim expr As SyntaxResult

        If blocks >= 3 AndAlso blocks(1).isOperator("=") Then
            ' python tuple syntax is not support in 
            ' Rscript, translate tuple syntax from
            ' python script as list syntax into rscript
            expr = Implementation.AssignValue(blocks(0), blocks.Skip(2).IteratesALL.ToArray, opts)
        ElseIf blocks = 1 AndAlso blocks(Scan0).isPythonTuple Then
            expr = Implementation.TupleParser(blocks(Scan0), opts)
        Else
            If blocks = 1 Then
                blocks = blocks(Scan0).SplitByTopLevelDelimiter(
                    delimiter:=TokenType.close,
                    includeKeyword:=True,
                    tokenText:=")"
                )

                If blocks > 2 AndAlso
                    blocks(Scan0).First.name = TokenType.identifier AndAlso
                    blocks(Scan0)(1).name = TokenType.open Then

                    Dim chain As FunctionInvoke = opts.ParseExpression(blocks.Take(2).IteratesALL, opts).expression
                    Dim pipNext As SyntaxResult

                    For Each block In blocks.Skip(2).SlideWindows(2, offset:=2)
                        pipNext = opts.ParseExpression(block.IteratesALL, opts)

                        If pipNext.isException Then
                            Return pipNext
                        ElseIf TypeOf pipNext.expression Is FunctionInvoke Then
                            Dim firstArg As Expression = chain

                            chain = pipNext.expression
                            chain = New FunctionInvoke(
                                funcVar:=chain.funcName.removeExtension,
                                stackFrame:=chain.stackFrame,
                                {firstArg}.JoinIterates(chain.parameters).ToArray
                            )
                        Else
                            Throw New NotImplementedException
                        End If
                    Next

                    Return chain
                Else
                    blocks = New List(Of Token()) From {blocks.IteratesALL.ToArray}
                End If
            End If

            expr = blocks.ParseExpression(opts)
        End If

        Return expr
    End Function

    <Extension>
    Private Function removeExtension(funcName As Expression) As Expression
        If TypeOf funcName Is SymbolReference Then
            funcName = New SymbolReference(DirectCast(funcName, SymbolReference).symbol.Trim("."c))
        ElseIf TypeOf funcName Is Literal Then
            funcName = New Literal(DirectCast(funcName, Literal).ValueStr.Trim("."c))
        End If

        Return funcName
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Friend Function isCommaSymbol(b As Token()) As Boolean
        Return b.Length = 1 AndAlso b(Scan0) = (TokenType.comma, ",")
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Friend Function isPythonTuple(b As Token()) As Boolean
        Return b.Any(Function(t) t = (TokenType.comma, ",")) AndAlso b.First = (TokenType.open, "(") AndAlso b.Last = (TokenType.close, ")")
    End Function
End Module
