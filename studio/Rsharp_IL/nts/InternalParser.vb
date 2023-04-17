Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components

Public Module InternalParser

    <Extension>
    Public Function ParsePyScript(script As Rscript, Optional debug As Boolean = False) As Program
        ' Return New SyntaxTree(script, debug).ParseTsScript()
    End Function
End Module
