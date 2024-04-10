Imports System.IO
Imports System.Reflection
Imports System.Runtime.Loader
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Development.Package

    ''' <summary>
    ''' https://stackoverflow.com/questions/27266907/no-appdomains-in-net-core-why
    ''' </summary>
    Public Class CollectibleAssemblyLoadContext : Inherits AssemblyLoadContext

        ReadOnly in_memory As PackageNamespace

        Sub New(pkg As PackageNamespace)
            in_memory = pkg
        End Sub

        Public Function GetAssemblyStream(libdll As String) As Stream
            Return in_memory.libPath.OpenFile(libdll)
        End Function

        Protected Overrides Function Load(assemblyName As AssemblyName) As Assembly
            Throw New NotImplementedException
        End Function
    End Class
End Namespace