Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Package

Namespace Interpreter.ExecuteEngine

    Public Class [Imports] : Inherits Expression

        Public ReadOnly Property packages As Expression
        Public ReadOnly Property library As Expression
        Public Overrides ReadOnly Property type As TypeCodes

        Sub New(code As IEnumerable(Of Token()))
            With code.ToArray
                If Not .ElementAt(Scan0).isKeyword("imports") OrElse Not .ElementAt(2).isKeyword("from") Then
                    Throw New SyntaxErrorException
                Else
                    packages = .ElementAt(1).DoCall(AddressOf Expression.CreateExpression)
                    library = .ElementAt(3).DoCall(AddressOf Expression.CreateExpression)
                End If
            End With
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As Index(Of String) = Runtime.asVector(Of String)(Me.packages.Evaluate(envir)) _
                .AsObjectEnumerator _
                .Select(Function(o)
                            Return Scripting.ToString(o, Nothing)
                        End Function) _
                .ToArray
            Dim libDll = Scripting.ToString(Runtime.getFirst(library.Evaluate(envir)))

            If libDll.StringEmpty Then
                Return Internal.stop("No package module provided!", envir)
            End If

            Dim packages = PackageLoader.ParsePackages($"{App.HOME}/{libDll}") _
                .Where(Function(pkg) pkg.info.Namespace Like names) _
                .GroupBy(Function(pkg) pkg.namespace) _
                .ToDictionary(Function(pkg) pkg.Key,
                              Function(group)
                                  Return group.First
                              End Function)

            For Each required In names.Objects
                If packages.ContainsKey(required) Then
                    Call ImportsPackage.ImportsStatic(envir.GlobalEnvironment, packages(required).package)
                Else
                    Return Internal.stop({
                        $"There is no package named '{required}' in given module!",
                        $"namespace: {required}",
                        $"library module: {libDll}"}, envir
                    )
                End If
            Next

            Return Nothing
        End Function
    End Class
End Namespace