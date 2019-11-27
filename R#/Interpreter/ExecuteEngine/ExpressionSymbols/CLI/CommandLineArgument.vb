Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' Get commandline argument value by name
    ''' 
    ''' ```
    ''' ?'name'
    ''' ```
    ''' </summary>
    Public Class CommandLineArgument : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.string
            End Get
        End Property

        ''' <summary>
        ''' The argument name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As Expression

        Sub New(tokens As Token())
            If tokens.First = TokenType.iif Then
                name = tokens _
                    .Skip(1) _
                    .DoCall(AddressOf Expression.CreateExpression)
            Else
                name = Expression.CreateExpression(tokens)
            End If
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim arg As String = Runtime.getFirst(name.Evaluate(envir))
            Dim value As DefaultString = App.CommandLine(arg)

            Return CType(value, String)
        End Function
    End Class
End Namespace