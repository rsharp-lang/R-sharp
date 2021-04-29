Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    Public Class RDefaultExpressionAttribute : Inherits RInteropAttribute

        Public Shared Function ParseDefaultExpression(strExp As String) As Expression
            Return Program.CreateProgram(Rscript.FromText(strExp.Trim("~"c)), debug:=False, [error]:=Nothing).execQueue.First
        End Function

    End Class
End Namespace