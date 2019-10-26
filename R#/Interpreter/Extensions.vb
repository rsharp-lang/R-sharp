Imports System.Runtime.CompilerServices
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

        <Extension>
        Public Iterator Function GetExpressions(code As Token()) As IEnumerable(Of Expression)
            For Each block In code.Split(Function(t) t.name = TokenType.terminator)
                If block(Scan0).name = TokenType.keyword Then
                    Dim keyword As String = block(Scan0).text

                    Select Case keyword
                        Case "let" : Yield New DeclareNewVariable(block)
                        Case Else
                            Throw New SyntaxErrorException
                    End Select
                End If
            Next
        End Function
    End Module
End Namespace