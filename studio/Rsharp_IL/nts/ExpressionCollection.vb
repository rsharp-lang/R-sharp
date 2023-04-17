Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

''' <summary>
''' A temp expression collection for the function invoke parameters
''' </summary>
Public Class ExpressionCollection : Inherits Expression

    Public Overrides ReadOnly Property type As TypeCodes
    Public Overrides ReadOnly Property expressionName As Rsharp.Development.Package.File.ExpressionTypes

    Public Property expressions As Expression()

    Public Shared Function GetExpressions(exp As Expression) As Expression()
        If TypeOf exp Is ExpressionCollection Then
            Return DirectCast(exp, ExpressionCollection).expressions
        Else
            Return {exp}
        End If
    End Function

    Public Overrides Function Evaluate(envir As Rsharp.Runtime.Environment) As Object
        Throw New NotImplementedException()
    End Function
End Class