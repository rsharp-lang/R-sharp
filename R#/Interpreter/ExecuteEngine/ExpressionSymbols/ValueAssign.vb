Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class ValueAssign : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim targetSymbol As String
        Dim isByRef As Boolean
        Dim value As Expression

        Sub New(tokens As Token())
            targetSymbol = tokens(Scan0).text
            isByRef = tokens(Scan0).text = "="
            value = tokens.Skip(2).ToArray.DoCall(AddressOf Expression.CreateExpression)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object = Me.value.Evaluate(envir)
            Dim target As Variable = envir(targetSymbol)

            If isByRef Then
                target.value = value
            Else
                If value.GetType.IsInheritsFrom(GetType(Array)) Then
                    target.value = DirectCast(value, Array).Clone
                Else
                    target.value = value
                End If
            End If

            Return value
        End Function
    End Class
End Namespace