#Region "Microsoft.VisualBasic::a099ad28adcb57697a769ea61989d330, R#\System\Package\CollectibleAssemblyLoadContext.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 44
    '    Code Lines: 31
    ' Comment Lines: 3
    '   Blank Lines: 10
    '     File Size: 1.53 KB


    '     Class CollectibleAssemblyLoadContext
    ' 
    '         Properties: runtime
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetAssemblyStream, InMemory_Resolver, Load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
