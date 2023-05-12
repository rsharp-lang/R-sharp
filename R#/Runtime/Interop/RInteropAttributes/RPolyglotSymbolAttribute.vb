Imports System.Reflection

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=True, Inherited:=True)>
    Public Class RPolyglotSymbolAttribute : Inherits RInteropAttribute

        Public ReadOnly Property [Alias] As String

        Sub New(name As String)
            [Alias] = name
        End Sub

        Public Overrides Function ToString() As String
            Return [Alias]
        End Function

        Public Shared Iterator Function GetAlternativeNames(pkg As Type) As IEnumerable(Of String)
            Dim attrs = pkg.GetCustomAttributes(Of RPolyglotSymbolAttribute)

            For Each a As RPolyglotSymbolAttribute In attrs
                Yield a.Alias
            Next
        End Function

    End Class
End Namespace