Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter
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

            Return expr
        End Function
    End Module
End Namespace