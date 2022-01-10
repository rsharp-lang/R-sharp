#If netcore5 = 1 Then
Imports System.Data
#End If
Imports System.Runtime.CompilerServices
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
            expr = blocks.ParseExpression(opts)
        End If

        Return expr
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
