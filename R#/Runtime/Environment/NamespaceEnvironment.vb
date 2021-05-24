Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package.File
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
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return $"{[namespace]}: {symbols.Keys.GetJson}"
        End Function

    End Class

    Public Class SymbolNamespaceSolver : Implements IEnumerable(Of NamespaceEnvironment)

        Public ReadOnly Property attachedNamespace As New Dictionary(Of String, NamespaceEnvironment)

        Default Public ReadOnly Property GetNamespace(ref As String) As NamespaceEnvironment
            Get
                Return attachedNamespace.TryGetValue(ref)
            End Get
        End Property

        Public ReadOnly Property packageNames As String()
            Get
                Return attachedNamespace.Keys.ToArray
            End Get
        End Property

        Public Function hasNamespace(pkgName As String) As Boolean
            Return attachedNamespace.ContainsKey(pkgName)
        End Function

        Public Function Add([namespace] As PackageNamespace) As NamespaceEnvironment
            attachedNamespace([namespace].packageName) = New NamespaceEnvironment([namespace].packageName, [namespace].libPath)
            Return attachedNamespace([namespace].packageName)
        End Function

        Public Function FindSymbol(namespace$, symbolName$) As RFunction
            If Not attachedNamespace.ContainsKey([namespace]) Then
                Return Nothing
            Else
                Return attachedNamespace([namespace]).symbols.TryGetValue(symbolName)
            End If
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of NamespaceEnvironment) Implements IEnumerable(Of NamespaceEnvironment).GetEnumerator
            For Each item As NamespaceEnvironment In attachedNamespace.Values
                Yield item
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace