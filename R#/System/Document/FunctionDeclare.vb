Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development

    Public Class FunctionDeclare

        Public Property name As String
        Public Property parameters As NamedValue()
        Public Property sourceMap As StackFrame

        Public Overrides Function ToString() As String
            Return $"{name}({parameters _
                .Select(Function(a)
                            If a.text.StringEmpty Then
                                Return a.name
                            Else
                                Return $"{a.name} = {a.text}"
                            End If
                        End Function) _
                .JoinBy(", ")})"
        End Function

        Public Shared Function GetArgument(arg As DeclareNewSymbol) As NamedValue
            Dim name As String = arg.names.JoinBy(", ")
            Dim val As String = Nothing

            If arg.hasInitializeExpression Then
                val = valueText(arg.m_value)
            End If

            Return New NamedValue With {
                .name = name,
                .Text = val
            }
        End Function

        Private Shared Function valueText(expr As Expression) As String
            Select Case expr.GetType
                Case GetType(Literal)
                    Dim literal As Literal = expr

                    Select Case literal.type
                        Case TypeCodes.boolean : Return literal.Evaluate(Nothing).ToString.ToUpper
                        Case TypeCodes.double, TypeCodes.integer : Return literal.Evaluate(Nothing).ToString
                        Case TypeCodes.string : Return literal.Evaluate(Nothing).ToString
                        Case Else

                            If literal.isNull Then
                                Return "NULL"
                            Else
                                Throw New InvalidCastException
                            End If

                    End Select
                Case GetType(VectorLiteral)

                    Return $"[{DirectCast(expr, VectorLiteral).Select(AddressOf valueText).JoinBy(", ")}]"

                Case GetType(SymbolReference)

                    Return DirectCast(expr, SymbolReference).symbol

                Case GetType(FunctionInvoke)

                    Dim funCall As FunctionInvoke = DirectCast(expr, FunctionInvoke)
                    Dim args = funCall.parameters.Select(AddressOf valueText).ToArray

                    Return $"{valueText(funCall.funcName)}({args.JoinBy(", ")})"

                Case GetType(ValueAssign)

                    Dim assign As ValueAssign = DirectCast(expr, ValueAssign)
                    Dim symbol As String() = assign.targetSymbols.Select(AddressOf valueText).ToArray
                    Dim symbolText As String

                    If symbol.Length = 1 Then
                        symbolText = symbol(Scan0)
                    Else
                        symbolText = $"[{symbol.JoinBy(", ")}]"
                    End If

                    Return $"{symbolText} = {valueText(assign.value)}"

                Case Else
                    Throw New NotImplementedException(expr.ToString)
            End Select
        End Function
    End Class
End Namespace