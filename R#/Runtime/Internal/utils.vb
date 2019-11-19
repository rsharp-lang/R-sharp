Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Internal

    Module utils

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="packages">The dll file name</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function installPackages(packages As String(), envir As Environment) As Object
            For Each pkgName As String In packages.SafeQuery

            Next
        End Function
    End Module
End Namespace