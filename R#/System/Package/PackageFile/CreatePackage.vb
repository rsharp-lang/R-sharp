Imports System.IO
Imports SMRUCC.Rsharp.Runtime.Components

Namespace System.Package.File

    Public Module CreatePackage

        ''' <summary>
        ''' build a R# package file
        ''' </summary>
        ''' <param name="target">
        ''' the target directory that contains the necessary 
        ''' files for create a R# package file.</param>
        ''' <param name="outfile">the output zip file stream</param>
        ''' <returns></returns>
        Public Function Build(target As String, outfile As Stream) As Message
            Dim Rsrc As String() = $"{target}/R/".ListFiles("*.R").ToArray
            Dim desc As DESCRIPTION = DESCRIPTION.Parse($"{target}/DESCRIPTION")
            Dim file As New PackageModel With {
                .info = desc
            }

            Call file.Flush(outfile)
        End Function
    End Module
End Namespace