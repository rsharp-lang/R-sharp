Imports Microsoft.VisualBasic.Linq
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
                .body = code _
                    .Skip(1) _
                    .Take(code.Length - 2) _
                    .DoCall(AddressOf ClosureExpression.ParseExpressionTree),
                .funcName = "else_branch_internal",
                .params = {}
            }
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            If envir.ifPromise = 0 Then
                Throw New SyntaxErrorException
            Else
                Dim last As IfBranch.IfPromise

                If envir.ifPromise.Last.Result = True Then
                    last = envir.ifPromise.Pop
                Else
                    last = New IfBranch.IfPromise(closure.Invoke(envir, {}), False) With {
                        .assignTo = envir.ifPromise.Last.assignTo
                    }
                End If

                Call last.DoValueAssign(envir)

                Return last
            End If
        End Function
    End Class

    Public Class ElseIfBranch : Inherits IfBranch

        Public Sub New(tokens As IEnumerable(Of Token))
            MyBase.New(tokens)
        End Sub
    End Class
End Namespace