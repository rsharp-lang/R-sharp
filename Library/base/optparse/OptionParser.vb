Imports SMRUCC.Rsharp.Development.CommandLine

Public Class OptionParser

    Public Property usage As String
    Public Property option_list As OptionParserOption()
    Public Property add_help_option As Boolean = True
    Public Property description As String
    Public Property epilogue As String

    Public Function GetDocument() As CommandLineDocument

    End Function

End Class