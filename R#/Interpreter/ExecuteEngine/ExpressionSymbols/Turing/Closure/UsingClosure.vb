Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class UsingClosure : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly params As DeclareNewVariable
        ReadOnly closure As ClosureExpression

        Sub New(code As IEnumerable(Of Token()))
            Dim input = code.IteratesALL.ToArray
            Dim tokens As Token()() = input _
                .SplitByTopLevelDelimiter(TokenType.close) _
                .ToArray
            Dim parmPart As Token() = tokens(Scan0)

            If tokens(1).Length = 1 AndAlso tokens(1)(Scan0) = (TokenType.close, ")") Then
                parmPart = parmPart + tokens(1).AsList
            End If

            closure = New ClosureExpression(tokens(2).Skip(1))
            tokens = parmPart.SplitByTopLevelDelimiter(TokenType.keyword, tokenText:="as")
            params = New DeclareNewVariable(tokens(Scan0), tokens(2))
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Using env As New Environment(envir, params.ToString)
                Dim resource As Object = params.Evaluate(env)
                Dim result As Object

                If resource Is Nothing Then
                    Return Internal.stop("Target is nothing!", env)
                ElseIf Program.isException(resource) Then
                    Return resource
                ElseIf Not resource.GetType.ImplementInterface(GetType(IDisposable)) Then
                    Return Message.InCompatibleType(GetType(IDisposable), resource.GetType, env)
                End If

                ' run using closure and get result
                result = closure.Evaluate(env)

                ' then dispose the target
                ' release handle and clean up the resource
                Call DirectCast(resource, IDisposable).Dispose()

                Return result
            End Using
        End Function

        Public Overrides Function ToString() As String
            Return $"using {params} {{
    # using closure internal
    {closure}
}}"
        End Function
    End Class
End Namespace