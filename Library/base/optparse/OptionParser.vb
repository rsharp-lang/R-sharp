Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Development.CommandLine

Public Class OptionParser

    Public Property usage As String
    Public Property option_list As OptionParserOption()
    Public Property add_help_option As Boolean = True
    Public Property description As String
    Public Property epilogue As String
    Public Property title As String

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetDocument() As CommandLineDocument
        Return New CommandLineDocument With {
            .arguments = option_list _
                .Select(Function(a) a.CreateArgumentInternal) _
                .ToArray,
            .info = description,
            .title = title
        }
    End Function

End Class