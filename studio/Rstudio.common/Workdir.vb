
Imports System.Runtime.CompilerServices

''' <summary>
''' config current workspace directory
''' </summary>
Module Workdir

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="dir"></param>
    ''' <param name="Rscript">
    ''' the absolute full path of the target rscript file
    ''' </param>
    ''' <returns></returns>
    <Extension>
    Public Function TranslateWorkdir(dir As String, Rscript As String) As String
        If dir.ToLower = ("@dir") Then
            Return Rscript.ParentPath
        End If

        Return dir.GetDirectoryFullPath
    End Function

End Module
