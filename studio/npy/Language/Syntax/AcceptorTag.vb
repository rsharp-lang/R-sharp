Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Public Class AcceptorTag : Inherits PythonCodeDOM

    Public Overrides Function ToExpression(release As Index(Of String)) As Expression
        Return MyBase.ToExpression(release)
    End Function

End Class
