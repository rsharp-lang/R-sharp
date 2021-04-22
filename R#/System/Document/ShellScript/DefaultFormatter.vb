Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports any = Microsoft.VisualBasic.Scripting

Namespace Development.CommandLine

    Module DefaultFormatter

        Public Function FormatDefaultString(expr As Expression) As String
            If TypeOf expr Is StringInterpolation Then
                Return FormatDefaultString(DirectCast(expr, StringInterpolation))
            ElseIf TypeOf expr Is Literal Then
                Return any.ToString(DirectCast(expr, Literal).value)
            ElseIf TypeOf expr Is FunctionInvoke Then
                Return FormatDefaultString(DirectCast(expr, FunctionInvoke))
            ElseIf TypeOf expr Is SymbolReference Then
                Return expr.ToString
            Else
                Return expr.ToString
            End If
        End Function

        Private Function FormatDefaultString(expr As FunctionInvoke) As String
            Dim args As String() = expr.parameters.Select(AddressOf FormatDefaultString).ToArray
            Dim name As String = FormatDefaultString(expr.funcName)

            Return $"{name}({args.JoinBy(", ")})"
        End Function

        Private Function FormatDefaultString(expr As StringInterpolation) As String
            Dim sb As New StringBuilder

            For Each part As Expression In expr.stringParts
                If TypeOf part Is Literal Then
                    sb.Append(any.ToString(DirectCast(part, Literal).value))
                Else
                    sb.Append($"${{{FormatDefaultString(part)}}}")
                End If
            Next

            Return sb.ToString
        End Function
    End Module
End Namespace