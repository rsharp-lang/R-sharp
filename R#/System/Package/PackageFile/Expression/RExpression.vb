Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File.Expressions

    Public MustInherit Class RExpression

        Public MustOverride Function GetExpression() As Expression

        Public Shared Function CreateFromSymbolExpression(exec As Expression) As RExpression
            If TypeOf exec Is [Imports] Then
                Return RImports.GetRImports(DirectCast(exec, [Imports]))
            ElseIf TypeOf exec Is Require Then
                Return RImports.GetRImports(DirectCast(exec, Require))
            ElseIf TypeOf exec Is DeclareNewFunction Then
                Return RFunction.FromSymbol(exec)
            Else
                Return New ParserError
            End If
        End Function
    End Class

    Public Class ParserError : Inherits RExpression

        Public Property message As String()

        Public Overrides Function GetExpression() As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace