Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Interpreter.SyntaxParser

    Friend Class NamespaceReferenceProcessor : Inherits GenericSymbolOperatorProcessor

        Sub New()
            Call MyBase.New("::")
        End Sub

        Protected Overrides Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim namespaceRef As Expression
            Dim syntaxTemp As SyntaxResult = a.TryCast(Of SyntaxResult)

            If syntaxTemp.isException Then
                Return syntaxTemp
            ElseIf b.VA.isException Then
                Return b.VA
            End If

            Dim nsSymbol$ = DirectCast(syntaxTemp.expression, SymbolReference).symbol

            If TypeOf b.VA.expression Is FunctionInvoke Then
                ' a::b() function invoke
                Dim calls As FunctionInvoke = b.VA.expression
                calls.namespace = nsSymbol
                namespaceRef = calls
            ElseIf TypeOf b.VA.expression Is SymbolReference Then
                ' a::b view function help info
                namespaceRef = New NamespaceFunctionSymbolReference(nsSymbol, b.VA.expression)
            Else
                Return New SyntaxResult(New SyntaxErrorException, opts.debug)
            End If

            Return New SyntaxResult(namespaceRef)
        End Function
    End Class
End Namespace