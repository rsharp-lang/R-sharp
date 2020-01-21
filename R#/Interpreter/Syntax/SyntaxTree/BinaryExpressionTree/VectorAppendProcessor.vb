
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Interpreter.SyntaxParser

    Friend Class VectorAppendProcessor : Inherits GenericSymbolOperatorProcessor

        Public Sub New()
            MyBase.New("<<")
        End Sub

        Protected Overrides Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult
            If a.VA.isException Then
                Return a
            ElseIf b.VA.isException Then
                Return b
            Else
                Return New SyntaxResult(New AppendOperator(a.VA.expression, b.VA.expression))
            End If
        End Function
    End Class
End Namespace