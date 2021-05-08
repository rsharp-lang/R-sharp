Imports Microsoft.VisualBasic.Language

Namespace Development.Package.File

    ''' <summary>
    ''' file handler of ``.Rbuildignore``
    ''' </summary>
    Public Class Rbuildignore

        ReadOnly patterns As Predicate(Of String)()

        Sub New(patterns As IEnumerable(Of Predicate(Of String)))
            Me.patterns = patterns.ToArray
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="relpath">
        ''' a relative path to the directory of DESCRIPTION meta data file.
        ''' </param>
        ''' <returns></returns>
        Public Function IsFileIgnored(relpath As String) As Boolean
            ' test on all pattern match
            For Each pattern As Predicate(Of String) In patterns
                If pattern(relpath) Then
                    Return True
                End If
            Next

            Return False
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="file">
        ''' a text file its file path or the content data text itself
        ''' </param>
        ''' <returns></returns>
        Public Shared Function CreatePatterns(file As String) As Rbuildignore
            Dim list As String() = file _
                .LineIterators _
                .Where(Function(line) Not line.StringEmpty) _
                .ToArray
            Dim patterns As New List(Of Predicate(Of String))

            For Each line As String In list
                If line.StartsWith("^") AndAlso line.EndsWith("$") Then
                    patterns.Add(AddressOf New PatternMatch(line).isMatch)
                Else
                    patterns.Add(AddressOf New RegularMatch(line).isMatch)
                End If
            Next

            Return New Rbuildignore(patterns)
        End Function

    End Class
End Namespace