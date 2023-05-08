Imports System.IO.Compression
Imports System.Text

Namespace Development.Package.File

    Public Class NuGetZip

        Dim zip As ZipArchive
        Dim checksum As New StringBuilder
        Dim pkg As PackageModel

        <DebuggerStepThrough>
        Sub New(zip As ZipArchive, pkg As PackageModel)
            Me.zip = zip
            Me.pkg = pkg
        End Sub



    End Class
End Namespace