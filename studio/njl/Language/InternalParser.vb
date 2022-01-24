Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.CodeAnalysis
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports Rscript = SMRUCC.Rsharp.Runtime.Components.Rscript

Namespace Language

    Public Module InternalParser

        <Extension>
        Public Function ParseJlScript(script As Rscript, Optional debug As Boolean = False) As Program
            Return New SyntaxTree(script, debug).ParseJlScript()
        End Function

        <Extension>
        Friend Function ParseJuliaLine(line As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim blocks = line.SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True)
            Dim expr As SyntaxResult = blocks.ParseExpression(opts)

            'If blocks > 2 AndAlso blocks(1).isOperator("=") Then
            '    ' value assign or simple function
            '    ' simple function: f(x,y) = x + y
            '    ' means:
            '    '    function f(x, y)
            '    '      return x + y
            '    '    end
            '    '    

            'End If
            If TypeOf expr.expression Is ByRefFunctionCall Then
                Dim byrefCall As ByRefFunctionCall = DirectCast(expr.expression, ByRefFunctionCall)
                Dim symbols As Index(Of String) = SymbolAnalysis _
                    .GetSymbolReferenceList(byrefCall.value) _
                    .Select(Function(a) a.Name) _
                    .Indexing
                ' 如果右边的符号全部来自于左边的函数参数，则认为是一个julia中的简单函数申明？
                Dim args = {byrefCall.target}.JoinIterates(byrefCall.arguments).ToArray

                ' function declare
                ' f(x,y) = x ^ y
                ' 
                ' byref function
                ' f(a$x, y) = x ^ y
                '
                If TypeOf byrefCall.funcRef Is Literal AndAlso args.All(Function(t) TypeOf t Is SymbolReference) Then
                    If args.All(Function(a) DirectCast(a, SymbolReference).symbol Like symbols) Then
                        ' is julia simple function declare
                        Dim name As String = DirectCast(byrefCall.funcRef, Literal).ValueStr
                        Dim params As DeclareNewSymbol() = args.Select(Function(a) New DeclareNewSymbol(DirectCast(a, SymbolReference).symbol, byrefCall.stackFrame)).ToArray
                        Dim content As New ClosureExpression(byrefCall.value)
                        Dim func As New DeclareNewFunction(name, params, content, byrefCall.stackFrame)

                        Return New SyntaxResult(func)
                    End If
                End If
            End If

            Return expr
        End Function
    End Module
End Namespace