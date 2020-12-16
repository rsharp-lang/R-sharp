Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Namespace System.Package.File.Expressions

    Public Class RCallFunction : Inherits RExpression

        Public Property target As RExpression
        Public Property sourceMap As StackFrame
        Public Property parameters As RExpression()
        Public Property [namespace] As String

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            Dim func As Expression = target.GetExpression(desc)
            Dim arguments As Expression() = parameters _
                .Select(Function(a)
                            Return a.GetExpression(desc)
                        End Function) _
                .ToArray

            Return New FunctionInvoke(func, sourceMap, arguments) With {
                .[namespace] = [namespace]
            }
        End Function
    End Class
End Namespace