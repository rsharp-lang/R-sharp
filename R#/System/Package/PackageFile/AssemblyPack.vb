Imports Microsoft.VisualBasic.Linq

Namespace Development.Package.File

    Public Class AssemblyPack : Implements Enumeration(Of String)

        Public Property framework As String
        ''' <summary>
        ''' dll files, apply for md5 checksum calculation
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' 程序会将几乎所有的文件都打包进去
        ''' </remarks>
        Public Property assembly As String()
        Public Property directory As String

        Public Function GetFileContents() As String()
            Return directory _
                .ListFiles("*.*") _
                .Where(Function(path)
                           Return Not path.ExtensionSuffix("pdb")
                       End Function) _
                .ToArray
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of String) Implements Enumeration(Of String).GenericEnumerator
            For Each dll As String In assembly
                Yield dll
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of String).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class
End Namespace