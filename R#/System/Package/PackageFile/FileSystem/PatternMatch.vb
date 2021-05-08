Imports System.Text.RegularExpressions
Imports regexp = System.Text.RegularExpressions.Regex

Namespace Development.Package.File

    Public Class PatternMatch

        ReadOnly r As regexp

        Sub New(pattern As String)
            r = New regexp(pattern, RegexOptions.Compiled Or RegexOptions.Multiline)
        End Sub

        Public Function isMatch(relpath As String) As Boolean
            Return r.Match(relpath).Success
        End Function
    End Class
End Namespace