Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace Development.CodeAnalysis

    ''' <summary>
    ''' the general definition model of the symbol or function in typescript
    ''' </summary>
    Public Class SymbolTypeDefine

        Public Property name As String
        Public Property parameters As NamedValue(Of String)()

        ''' <summary>
        ''' the return/symbol value type of current function
        ''' </summary>
        ''' <returns></returns>
        Public Property value As String = "any"

        Public ReadOnly Property isSymbol As Boolean
            Get
                Return parameters Is Nothing
            End Get
        End Property

        Public Overrides Function ToString() As String
            If isSymbol Then
                Return $"{name}:{value}"
            Else
                Return $"function {name}({getParameterList.JoinBy(", ")}): {value}"
            End If
        End Function

        Private Iterator Function getParameterList() As IEnumerable(Of String)
            For Each a As NamedValue(Of String) In parameters
                If a.Value Is Nothing Then
                    ' is required
                    Yield $"{a.Name}: {a.Description}"
                Else
                    Yield $"{a.Name}?: {a.Description}"
                End If
            Next
        End Function

    End Class
End Namespace