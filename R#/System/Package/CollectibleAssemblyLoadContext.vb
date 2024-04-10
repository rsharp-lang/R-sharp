Imports System.Reflection
Imports System.Runtime.Loader

Namespace Development.Package

    ''' <summary>
    ''' https://stackoverflow.com/questions/27266907/no-appdomains-in-net-core-why
    ''' </summary>
    Public Class CollectibleAssemblyLoadContext : Inherits AssemblyLoadContext

        Protected Overrides Function Load(assemblyName As AssemblyName) As Assembly

        End Function
    End Class
End Namespace