Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' the linq expression of a specific keyword 
    ''' </summary>
    Public MustInherit Class LinqKeywordExpression : Inherits Expression

        Public MustOverride ReadOnly Property keyword As String

        Public Overrides ReadOnly Property name As String
            Get
                Return keyword
            End Get
        End Property

    End Class
End Namespace