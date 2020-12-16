Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public Class RSymbol : Inherits RExpression
        Implements INamedValue

        Public Property name As String Implements INamedValue.Key
        Public Property type As TypeCodes
        Public Property value As RExpression
        Public Property [readonly] As Boolean

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            Return New DeclareNewSymbol({name}, value.GetExpression(desc), type, [readonly])
        End Function

        Public Shared Function GetSymbol(x As DeclareNewSymbol) As RExpression
            If (x.isTuple) Then
                Return New ParserError({"multiple name tuple is not supported!", x.ToString})
            Else
                Return New RSymbol With {
                    .name = x.names(Scan0),
                    .[readonly] = x.is_readonly,
                    .type = x.m_type,
                    .value = RExpression.CreateFromSymbolExpression(x.m_value)
                }
            End If
        End Function
    End Class
End Namespace