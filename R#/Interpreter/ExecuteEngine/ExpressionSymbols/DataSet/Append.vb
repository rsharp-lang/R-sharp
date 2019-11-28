
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' ```
    ''' a &lt;&lt; b
    ''' ```
    ''' </summary>
    Public Class Append : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return a.type
            End Get
        End Property

        ReadOnly a, b As Expression

        Sub New(a As Expression, b As Expression)
            Me.a = a
            Me.b = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace