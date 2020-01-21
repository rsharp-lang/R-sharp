Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Namespace Interpreter.SyntaxParser

    Friend Class PipelineProcessor : Inherits GenericSymbolOperatorProcessor

        Public Sub New()
            MyBase.New(":>")
        End Sub

        Protected Overrides Function expression(a As [Variant](Of SyntaxResult, String), b As [Variant](Of SyntaxResult, String), opts As SyntaxBuilderOptions) As SyntaxResult
            Dim pip As Expression

            If a.VA.isException Then
                Return a
            ElseIf b.VA.isException Then
                Return b
            Else
                pip = buildPipeline(a.VA.expression, b.VA.expression)
            End If

            If pip Is Nothing Then
                If b.VA.expression.DoCall(AddressOf isFunctionTuple) Then
                    Dim invokes As VectorLiteral = b.VA.expression
                    Dim calls As New List(Of Expression)

                    For Each [call] As Expression In invokes
                        calls += buildPipeline(a.VA.expression, [call])
                    Next

                    Return New SyntaxResult(New VectorLiteral(calls))
                Else
                    Return New SyntaxResult(New SyntaxErrorException, opts.debug)
                End If
            Else
                Return New SyntaxResult(pip)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function isFunctionTuple(b As Expression) As Boolean
            If Not TypeOf b Is VectorLiteral Then
                Return False
            ElseIf Not DirectCast(b, VectorLiteral) _
                .All(Function(e)
                         Return TypeOf e Is FunctionInvoke OrElse TypeOf e Is SymbolReference
                     End Function) Then

                Return False
            End If

            Return True
        End Function

        Private Function buildPipeline(a As Expression, b As Expression) As Expression
            Dim pip As FunctionInvoke

            If TypeOf a Is VectorLiteral Then
                With DirectCast(a, VectorLiteral)
                    If .length = 1 AndAlso TypeOf .First Is ValueAssign Then
                        a = .First
                    End If
                End With
            End If

            If TypeOf b Is FunctionInvoke Then
                pip = b
                pip.parameters.Insert(Scan0, a)
            ElseIf TypeOf b Is SymbolReference Then
                pip = New FunctionInvoke(DirectCast(b, SymbolReference).symbol, a)
            Else
                pip = Nothing
            End If

            Return pip
        End Function
    End Class
End Namespace