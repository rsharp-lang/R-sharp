Imports SMRUCC.Rsharp.Development.CommandLine

Public Class OptionParserOption

    Public Property opt_str As String()
    Public Property [default] As Object
    Public Property type As String
    Public Property help As String
    Public Property action As String

    Friend Function CreateArgumentInternal() As CommandLineArgument

    End Function

End Class