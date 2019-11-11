Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' ``func(a) &lt;- value``
    ''' </summary>
    Public Class ByRefFunctionCall : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        ReadOnly funcName$
        ReadOnly target As Expression
        ReadOnly value As Expression

        Sub New(invoke As Token(), value As Token())
            Dim target As FunctionInvoke = Expression.CreateExpression(invoke)

            Me.value = Expression.CreateExpression(value)
            Me.funcName = target.funcName
            Me.target = target.parameters(Scan0)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Select Case funcName
                Case "names"
                    Return Runtime.Internal.names(target.Evaluate(envir), value.Evaluate(envir), envir)
                Case Else
                    Return Message.SyntaxNotImplemented(envir, $"byref call of {funcName}")
            End Select
        End Function
    End Class
End Namespace