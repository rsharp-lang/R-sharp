Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    <HideModuleName>
    Module Extensions

        <Extension>
        Public Function RunProgram(code As Token(), envir As Environment) As Object
            Dim program As New Program With {
               .expressionQueue = code.GetExpressions.ToArray
            }

            Return program.Execute(envir)
        End Function

        ReadOnly ignores As Index(Of TokenType) = {TokenType.comment, TokenType.terminator}

        <Extension>
        Public Iterator Function GetExpressions(code As Token()) As IEnumerable(Of Expression)
            For Each block In code.SplitByTopLevelDelimiter(TokenType.terminator)
                If block.Length = 0 OrElse (block.Length = 1 AndAlso block(Scan0).name Like ignores) Then
                    ' skip code comments
                    ' do nothing
                Else
                    Yield Expression.CreateExpression(block)
                End If
            Next
        End Function
    End Module
End Namespace