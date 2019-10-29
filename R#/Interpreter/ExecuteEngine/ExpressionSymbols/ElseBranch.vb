Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class ElseBranch : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Dim closure As DeclareNewFunction

        Sub New(code As Token())
            closure = New DeclareNewFunction With {
                .body = New Program With {
                    .execQueue = code.Skip(1).Take(code.Length - 2).ToArray _
                        .GetExpressions _
                        .ToArray
                },
                .funcName = "else_branch_internal",
                .params = {}
            }
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            If envir.ifPromise = 0 Then
                Throw New SyntaxErrorException
            Else
                If envir.ifPromise.Last.Result = True Then
                    Return envir.ifPromise.Pop
                Else
                    Return New IfBranch.IfPromise(closure.Invoke(envir, {}), False)
                End If
            End If
        End Function
    End Class

    Public Class ElseIfBranch : Inherits IfBranch

        Public Sub New(tokens As IEnumerable(Of Token))
            MyBase.New(tokens)
        End Sub
    End Class
End Namespace