Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File.Expressions

    Public Class RImports : Inherits RExpression

        Public Property packages As String()
        Public Property [module] As String

        Public Overrides Function GetExpression(desc As DESCRIPTION) As Expression
            Dim names As Expression() = packages _
                .Select(Function(name) New Literal(name)) _
                .ToArray

            If [module].StringEmpty Then
                ' require
                Return New Require(names)
            Else
                Return New [Imports](New VectorLiteral(names), New Literal([module]))
            End If
        End Function

        Public Shared Function GetRImports([imports] As [Imports]) As RExpression
            Dim packages As List(Of String)
            Dim [module] As String

            If TypeOf [imports].packages Is Literal Then
                packages = New List(Of String) From {DirectCast([imports].packages, Literal).ValueStr}
            ElseIf TypeOf [imports].packages Is VectorLiteral Then
                packages = New List(Of String)

                For Each str As Expression In DirectCast([imports].packages, VectorLiteral)
                    If Not TypeOf str Is Literal Then
                        Return New ParserError({"package name should be a string literal!", [imports].ToString})
                    Else
                        packages.Add(DirectCast(str, Literal).value)
                    End If
                Next
            Else
                Return New ParserError({"unsupported expression type for package name!", [imports].ToString})
            End If

            If TypeOf [imports].library Is Literal Then
                [module] = DirectCast([imports].library, Literal).ValueStr
            Else
                Return New ParserError({"library module name should be a string literal!", [imports].ToString})
            End If

            Return New RImports With {
                .[module] = [module],
                .packages = packages
            }
        End Function

        Public Shared Function GetRImports(require As Require) As RExpression
            Dim packages As New List(Of String)

            For Each pkgName As Expression In require.packages
                packages.Add(ValueAssign.GetSymbol(pkgName))
            Next

            Return New RImports With {
                .packages = packages.ToArray
            }
        End Function
    End Class
End Namespace