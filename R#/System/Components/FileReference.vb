Imports Microsoft.VisualBasic.ApplicationServices

Namespace Development.Components

    Public Class FileReference

        ''' <summary>
        ''' The file object reference inside the given <see cref="fs"/> wrapper
        ''' </summary>
        ''' <returns></returns>
        Public Property filepath As String
        Public Property fs As IFileSystemEnvironment

        Public Overrides Function ToString() As String
            Return $"<&H0x{fs.GetHashCode.ToHexString}, {fs.ToString}> '{filepath}'"
        End Function

    End Class
End Namespace