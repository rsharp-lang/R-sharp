Namespace Interpreter.ExecuteEngine.LINQ.Syntax

    Public Class SyntaxParserResult

        Public message As Exception
        Public expression As Expression

        Public ReadOnly Property isError As Boolean
            Get
                Return Not message Is Nothing
            End Get
        End Property

        Sub New(err As Exception)
            message = err
        End Sub

        Sub New(expr As Expression)
            expression = expr
        End Sub

        Public Shared Widening Operator CType(exp As Expression) As SyntaxParserResult
            Return New SyntaxParserResult(exp)
        End Operator
    End Class
End Namespace