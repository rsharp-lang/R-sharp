Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime

    Public Class NamespaceEnvironment

        Public ReadOnly Property [namespace] As String
        Public ReadOnly Property symbols As New Dictionary(Of String, RFunction)
        Public ReadOnly Property libpath As String

        Sub New(namespace$, libpath$)
            Me.namespace = [namespace]
            Me.libpath = libpath
        End Sub

        Public Sub AddSymbols(symbols As IEnumerable(Of RFunction))
            For Each symbol As RFunction In symbols
                Call Me.symbols.Add(symbol.name, symbol)

                If TypeOf symbol Is DeclareNewFunction Then
                    DirectCast(symbol, DeclareNewFunction).Namespace = [namespace]
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return $"{[namespace]}: {symbols.Keys.GetJson}"
        End Function

    End Class
End Namespace