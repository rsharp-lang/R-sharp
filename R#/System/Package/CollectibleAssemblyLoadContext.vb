Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Runtime.Loader
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime

Namespace Development.Package

    ''' <summary>
    ''' https://stackoverflow.com/questions/27266907/no-appdomains-in-net-core-why
    ''' </summary>
    Public Class CollectibleAssemblyLoadContext : Inherits AssemblyLoadContext

        ReadOnly in_memory As PackageNamespace

        Public ReadOnly Property runtime As PackageEnvironment

        Sub New(pkg As PackageNamespace, env As PackageEnvironment)
            Me.runtime = env
            Me.in_memory = pkg

            AddHandler Resolving, AddressOf InMemory_Resolver
        End Sub

        Private Function InMemory_Resolver(context As AssemblyLoadContext, name As AssemblyName) As Assembly
            Return Load(assemblyName:=name)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetAssemblyStream(libdll As String) As MemoryStream
            Return in_memory.libPath.OpenFile(libdll)
        End Function

        Protected Overrides Function Load(assemblyName As AssemblyName) As Assembly
            Try
                Return Assembly.Load(assemblyName)
            Catch ex As Exception
                Return Assembly.Load(GetAssemblyStream($"/lib/assembly/{assemblyName.Name}.dll").ToArray)
            End Try
        End Function

    End Class
End Namespace