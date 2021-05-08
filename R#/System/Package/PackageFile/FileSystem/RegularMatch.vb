
Namespace Development.Package.File

    Public Class RegularMatch

        ReadOnly filepath As String

        Sub New(pattern As String)
            filepath = Norm(pattern)
        End Sub

        Private Shared Function Norm(relpath As String) As String
            Return relpath.Replace("\", "/").StringReplace("[/]{2,}", "/").Trim("/"c, "."c)
        End Function

        Public Function isMatch(relpath As String) As Boolean
            Return Norm(relpath).StartsWith(filepath)
        End Function
    End Class
End Namespace