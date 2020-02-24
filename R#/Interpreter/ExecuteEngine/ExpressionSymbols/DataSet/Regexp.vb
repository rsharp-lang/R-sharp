Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports r = System.Text.RegularExpressions.Regex

Namespace Interpreter.ExecuteEngine

    Public Class Regexp : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        Public ReadOnly Property pattern As String

        Sub New(regexp As String)
            pattern = regexp
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return New r(pattern)
        End Function

        Public Overrides Function ToString() As String
            Return $"/{pattern}/"
        End Function
    End Class
End Namespace