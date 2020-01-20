Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Interpreter.SyntaxParser

    Friend Class SyntaxResult

        Public ReadOnly [error] As Exception
        Public ReadOnly expression As Expression

        ''' <summary>
        ''' 
        ''' </summary>
        Public ReadOnly stackTrace As String

        Public ReadOnly Property isException As Boolean
            Get
                Return Not [error] Is Nothing
            End Get
        End Property

        Public Sub New(syntax As Expression)
            Me.expression = syntax
        End Sub

        Sub New(err As Exception)
            Me.stackTrace = Environment.StackTrace
            Me.error = err
        End Sub

        Public Overrides Function ToString() As String
            If isException Then
                Return stackTrace
            Else
                Return expression.ToString
            End If
        End Function

        Public Shared Widening Operator CType(syntax As Expression) As SyntaxResult
            Return New SyntaxResult(syntax)
        End Operator

    End Class
End Namespace