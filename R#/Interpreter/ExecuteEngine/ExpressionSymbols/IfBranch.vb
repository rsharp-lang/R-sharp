Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine

    Public Class IfBranch : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Dim ifTest As Expression
        Dim trueClosure As Expression
        Dim elseClosure As Expression

        Sub New(tokens As List(Of Token()))
            Throw New NotImplementedException
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim test As Boolean = Runtime.getFirst(ifTest.Evaluate(envir))

            If test Then
                Return trueClosure.Evaluate(envir)
            ElseIf elseClosure Is Nothing Then
                Return Nothing
            Else
                Return elseClosure.Evaluate(envir)
            End If
        End Function
    End Class
End Namespace