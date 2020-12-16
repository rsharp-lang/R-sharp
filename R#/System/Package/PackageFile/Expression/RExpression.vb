Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace System.Package.File.Expressions

    Public MustInherit Class RExpression

        Public MustOverride Function GetExpression(desc As DESCRIPTION) As Expression

        Public Shared Function CreateFromSymbolExpression(exec As Expression) As RExpression
            If TypeOf exec Is [Imports] Then
                Return RImports.GetRImports(DirectCast(exec, [Imports]))
            ElseIf TypeOf exec Is Require Then
                Return RImports.GetRImports(DirectCast(exec, Require))
            ElseIf TypeOf exec Is DeclareNewFunction Then
                Return RFunction.FromSymbol(exec)
            Else
                Return New ParserError($"'{exec.GetType.FullName}' is not implemented!")
            End If
        End Function
    End Class

    Public Class ParserError : Inherits RExpression

        Public ReadOnly Property message As String()
        Public Property sourceMap As StackFrame

        Sub New(message As String(), Optional sourceMap As StackFrame = Nothing)
            Me.message = message
            Me.sourceMap = sourceMap
        End Sub

        Sub New(message As String, Optional sourceMap As StackFrame = Nothing)
            Me.message = {message}
            Me.sourceMap = sourceMap
        End Sub

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace