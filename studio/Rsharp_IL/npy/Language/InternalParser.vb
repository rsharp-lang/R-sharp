#Region "Microsoft.VisualBasic::e52b82f181baeca430e31050a8ec4705, R-sharp\studio\npy\Language\InternalParser.vb"

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

'   Total Lines: 150
'    Code Lines: 116
' Comment Lines: 9
'   Blank Lines: 25
'     File Size: 6.71 KB


' Module InternalParser
' 
'     Function: isCommaSymbol, isPythonTuple, ParsePyScript, ParsePythonLine, removeExtension
' 
' /********************************************************************************/

#End Region

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
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser.SyntaxImplements
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
            ' 0 1 2
            ' a = b

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
                ElseIf blocks = 2 AndAlso blocks(Scan0) _
                    .TakeWhile(Function(t)
                                   ' deal with the expression liked
                                   ' 1:nrow(x)
                                   Return t <> (TokenType.open, "(")
                               End Function) _
                    .Count = 1 Then

                    ' identifier(xxx)
                    Dim func = FunctionInvokeSyntax.FunctionInvoke(blocks.IteratesALL.ToArray, opts)

                    If func.isException Then
                        Return func
                    Else
                        Dim callFunc As FunctionInvoke = DirectCast(func.expression, FunctionInvoke)

                        If TypeOf callFunc.funcName Is Literal AndAlso DirectCast(callFunc.funcName, Literal).ValueStr.IndexOf("."c) > 0 Then
                            Return New SyntaxResult(New PipelineFunction(func.expression))
                        Else
                            Return func
                        End If
                    End If
                Else
                    blocks = New List(Of Token()) From {blocks.IteratesALL.ToArray}
                End If
            ElseIf blocks.Last.isFunctionInvoke AndAlso blocks.Take(blocks.Count - 1).All(Function(t) t.isOperator(".") OrElse t.isIdentifier) Then
                ' fix for R function call liked: write.csv(xxx)
                Dim prefix As String = blocks.Take(blocks.Count - 2).Select(Function(d) d.First.text).JoinBy("")
                Dim invoke = FunctionInvokeSyntax.FunctionInvoke(blocks.Last, opts)
                Dim calls As FunctionInvoke = invoke.expression
                Dim name = DirectCast(calls.funcName, Literal).ValueStr
                Dim fullName As String = $"{prefix}.{name}"

                calls = New FunctionInvoke(fullName, calls.stackFrame, calls.parameters)

                Return New SyntaxResult(New PipelineFunction(calls))
            ElseIf blocks >= 3 Then

                For i As Integer = 1 To blocks.Count - 1
                    If blocks(i).Length = 1 AndAlso blocks(i)(Scan0) = (TokenType.operator, ".") Then

                    End If
                Next

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
