Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class ModeOf : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
        Public ReadOnly Property keyword As String
        Public ReadOnly Property target As Expression

        Sub New(keyword$, target As Token())
            Me.keyword = keyword
            Me.target = Expression.CreateExpression(target)
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object = target.Evaluate(envir)

            If keyword = "modeof" Then
                If value Is Nothing Then
                    Return TypeCodes.NA.Description
                Else
                    Return value.GetType.GetRTypeCode.Description
                End If
            Else
                If value Is Nothing Then
                    Return GetType(Void)
                Else
                    Return value.GetType
                End If
            End If
        End Function
    End Class
End Namespace