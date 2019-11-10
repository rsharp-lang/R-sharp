Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

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
                    packages = Expression.CreateExpression(.ElementAt(1))
                    library = Expression.CreateExpression(.ElementAt(3))
                End If
            End With
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim names As Index(Of String) = Runtime.asVector(Of String)(packages.Evaluate(envir)) _
                .AsObjectEnumerator _
                .Select(Function(o)
                            Return Scripting.ToString(o, Nothing)
                        End Function) _
                .ToArray
            Dim libDll = Scripting.ToString(Runtime.getFirst(library.Evaluate(envir)), Nothing)

            If libDll.StringEmpty Then
                Return Internal.stop("No package module provided!", envir)
            End If

            Throw New NotImplementedException()
        End Function
    End Class
End Namespace