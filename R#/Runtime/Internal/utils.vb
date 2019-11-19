Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Package
Imports Microsoft.VisualBasic.Language

Namespace Runtime.Internal

    Module utils

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="packages">The dll file name</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function installPackages(packages As String(), envir As Environment) As Object
            Dim pkgMgr As PackageManager = envir.GlobalEnvironment.packages
            Dim namespaces As New List(Of String)

            For Each pkgName As String In packages.SafeQuery
                namespaces += pkgMgr.InstallLocals(pkgName)
            Next

            Return namespaces.ToArray
        End Function
    End Module
End Namespace