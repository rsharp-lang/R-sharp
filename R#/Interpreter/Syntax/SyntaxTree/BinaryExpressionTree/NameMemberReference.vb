Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Interpreter.SyntaxParser

    Friend Class NameMemberReferenceProcessor : Inherits GenericSymbolOperatorProcessor

        Sub New()
            Call MyBase.New("$")
        End Sub

        Protected Overrides Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim nameSymbol As String

            If a.VA.isException Then
                Return a
            ElseIf b.VA.isException Then
                Return b
            End If

            Dim typeofName As Type = b.VA.expression.GetType

            If typeofName Is GetType(SymbolReference) Then
                nameSymbol = DirectCast(b.VA.expression, SymbolReference).symbol
            ElseIf typeofName Is GetType(Literal) Then
                nameSymbol = DirectCast(b.VA.expression, Literal).value
            ElseIf typeofName Is GetType(FunctionInvoke) Then
                Dim invoke As FunctionInvoke = b.VA.expression
                Dim funcVar As New SymbolIndexer(a.VA.expression, invoke.funcName)

                Return New SyntaxResult(New FunctionInvoke(funcVar, invoke.parameters.ToArray))
            Else
                Return New SyntaxResult(New NotImplementedException, opts.debug)
            End If

            ' a$b symbol reference
            Dim symbolRef As New SymbolIndexer(a.VA.expression, New Literal(nameSymbol))
            Return New SyntaxResult(symbolRef)
        End Function
    End Class
End Namespace