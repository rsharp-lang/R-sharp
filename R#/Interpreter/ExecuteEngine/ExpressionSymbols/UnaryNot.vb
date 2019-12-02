Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class UnaryNot : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.boolean
            End Get
        End Property

        ReadOnly logical As Expression

        Sub New(logical As Expression)
            Me.logical = logical
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim logicals As Boolean() = Runtime.asLogical(logical.Evaluate(envir))
            Dim nots As Boolean() = logicals _
                .Select(Function(b) Not b) _
                .ToArray

            Return nots
        End Function
    End Class
End Namespace