Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' declare a new temp symbol: ``LET x = ...``
    ''' </summary>
    Public Class SymbolDeclare : Inherits LinqKeywordExpression

        ''' <summary>
        ''' is a <see cref="Literal"/> or <see cref="VectorLiteral"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property symbol As Expression
        Public Property typeName As String

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "LET"
            End Get
        End Property

        Public Overrides Function Exec(context As ExecutableContext) As Object
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace