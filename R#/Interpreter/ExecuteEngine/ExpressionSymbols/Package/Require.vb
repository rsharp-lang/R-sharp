Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class Require : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Property packages As Expression()

        Sub New(names As Token())
            packages = names _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(b) Not b.isComma) _
                .Select(AddressOf Expression.CreateExpression) _
                .ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As New List(Of String)
            Dim [global] As GlobalEnvironment = envir.globalEnvironment
            Dim pkgName As String
            Dim message As Message

            For Each name As Expression In packages
                pkgName = ValueAssign.GetSymbol(name)
                message = [global].LoadLibrary(pkgName)

                If Not message Is Nothing Then
                    Call Interpreter.printMessageInternal(message)
                End If
            Next

            Return names.ToArray
        End Function

        Public Overrides Function ToString() As String
            Return $"require({packages.JoinBy(", ")})"
        End Function
    End Class
End Namespace