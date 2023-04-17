Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Public Module InternalParser

    <Extension>
    Public Function ParseTsScript(script As Rscript, Optional debug As Boolean = False) As Program
        Return New SyntaxTree(script, debug).ParseTsScript()
    End Function

    Public Function ParseTypeScriptLine(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult

    End Function
End Module
