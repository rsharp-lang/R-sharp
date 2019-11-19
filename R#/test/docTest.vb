Imports SMRUCC.Rsharp.Runtime.Package

Module docTest

    Sub Main()
        Dim doc As New AnnotationDocs
        Dim result = doc.GetAnnotations(GetType(LICENSE))

        Pause()
    End Sub
End Module
