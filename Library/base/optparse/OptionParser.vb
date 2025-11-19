Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.CommandLine
Imports SMRUCC.Rsharp.Development.Package.File

Public Class OptionParser

    Public Property usage As String
    Public Property option_list As OptionParserOption()
    Public Property add_help_option As Boolean = True
    Public Property description As String
    Public Property epilogue As String
    Public Property title As String
    Public Property dependency As String()

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function GetDocument() As CommandLineDocument
        Return New CommandLineDocument With {
            .arguments = option_list _
                .Select(Function(a) a.CreateArgumentInternal) _
                .ToArray,
            .info = description,
            .title = title,
            .sourceScript = App.CommandLine.Name,
            .dependency = dependency _
                .SafeQuery _
                .Select(Function(name) New Dependency(name)) _
                .ToArray
        }
    End Function

End Class