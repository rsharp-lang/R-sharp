Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace System.Package.File.Expressions

    Public Class RFunction : Inherits RExpression
        Implements INamedValue

        Public Property name As String Implements INamedValue.Key
        Public Property parameters As RSymbol()

        Public Overrides Function GetExpression() As Expression
            Throw New NotImplementedException()
        End Function

        Public Shared Function FromSymbol(symbol As DeclareNewFunction) As RExpression
            Dim name As String = symbol.funcName
            Dim params As RExpression() = symbol.params.Select(AddressOf RSymbol.GetSymbol).ToArray

            For Each item In params
                If TypeOf item Is ParserError Then
                    Return item
                End If
            Next

            Return New RFunction With {
                .name = name,
                .parameters = params _
                    .Select(Function(a) DirectCast(a, RSymbol)) _
                    .ToArray
            }
        End Function
    End Class
End Namespace