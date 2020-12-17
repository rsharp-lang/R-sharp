Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace System.Package.File

    Public Class Dependency

        Public Property packages As String()
        Public Property library As String

        Sub New()
        End Sub

        Sub New([imports] As [Imports])
            library = ValueAssign.GetSymbol([imports].library)

            If TypeOf [imports].packages Is Literal Then
                packages = {DirectCast([imports].packages, Literal).value.ToString}
            Else
                packages = DirectCast([imports].packages, VectorLiteral).values _
                    .Select(AddressOf ValueAssign.GetSymbol) _
                    .ToArray
            End If
        End Sub

        Sub New(require As Require)
            packages = require.packages.Select(AddressOf ValueAssign.GetSymbol).ToArray
        End Sub

        Public Shared Iterator Function GetDependency(loading As IEnumerable(Of Expression)) As IEnumerable(Of Dependency)
            For Each line As Expression In loading
                Select Case line.GetType
                    Case GetType([Imports]) : Yield New Dependency(DirectCast(line, [Imports]))
                    Case GetType(Require) : Yield New Dependency(DirectCast(line, Require))
                    Case Else
                        Throw New InvalidProgramException($"'{line.GetType.FullName}' is not a dependency expression!")
                End Select
            Next
        End Function

    End Class
End Namespace