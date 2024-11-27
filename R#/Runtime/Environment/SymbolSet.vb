Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime

    ''' <summary>
    ''' A set of the R# runtime symbols
    ''' </summary>
    Public Class SymbolSet : Implements IEnumerable(Of Symbol)

        ReadOnly symbols As Dictionary(Of String, Symbol)

        Public ReadOnly Property SymbolNames As String()
            Get
                Return symbols.Keys.OrderBy(Function(a) a).ToArray
            End Get
        End Property

        Default Public Property Item(name As String) As Symbol
            Get
                If symbols.ContainsKey(name) Then
                    Return symbols(name)
                Else
                    Return Nothing
                End If
            End Get
            Set(value As Symbol)
                symbols(name) = value
            End Set
        End Property

        Sub New(symbols As IEnumerable(Of Symbol))
            Me.symbols = symbols.ToDictionary(Function(s) s.name)
        End Sub

        Public Overrides Function ToString() As String
            Dim const_s As Integer = symbols.Values.Where(Function(s) s.readonly).Count
            Dim let_s As Integer = symbols.Count - const_s

            Return $"{symbols.Count} symbols inside environment, {const_s} binding lock and {let_s} unlock binding."
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Symbol) Implements IEnumerable(Of Symbol).GetEnumerator
            For Each s As Symbol In symbols.Values
                Yield s
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace