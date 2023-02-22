#Region "Microsoft.VisualBasic::527f79ef2596002a7dd24d352ecd39fb, D:/GCModeller/src/R-sharp/R#//Runtime/Environment/SymbolNamespaceSolver.vb"

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

    '   Total Lines: 171
    '    Code Lines: 109
    ' Comment Lines: 35
    '   Blank Lines: 27
    '     File Size: 7.30 KB


    '     Class SymbolNamespaceSolver
    ' 
    '         Properties: attachedNamespace, env, packageNames
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: (+2 Overloads) Add, FindPackageSymbol, FindSymbol, GetEnumerator, hasNamespace
    '                   IEnumerable_GetEnumerator, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime

    Public Class SymbolNamespaceSolver : Implements IEnumerable(Of PackageEnvironment)

        Public ReadOnly Property attachedNamespace As New Dictionary(Of String, PackageEnvironment)
        Public ReadOnly Property env As GlobalEnvironment

        ''' <summary>
        ''' an overrloads of [func_name => [namespace => func]]
        ''' </summary>
        Friend ReadOnly funcOverloads As New Dictionary(Of String, Dictionary(Of String, RFunction))

        Default Public ReadOnly Property GetNamespace(ref As String) As PackageEnvironment
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return attachedNamespace.TryGetValue(ref)
            End Get
        End Property

        Public ReadOnly Property packageNames As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return attachedNamespace.Keys.ToArray
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(env As GlobalEnvironment)
            Me.env = env
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function hasNamespace(pkgName As String) As Boolean
            Return attachedNamespace.ContainsKey(pkgName)
        End Function

        ''' <summary>
        ''' attach a R# zip/project folder source package
        ''' </summary>
        ''' <param name="[namespace]"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 20221126 due to the reason of zip/source folder package 
        ''' its directory path is required for loading the internal 
        ''' .NET assembly file, so we needs to overrides the old
        ''' package module at here?
        ''' </remarks>
        Public Function Add([namespace] As PackageNamespace) As PackageEnvironment
            attachedNamespace([namespace].packageName) = New PackageEnvironment(env, [namespace].packageName, [namespace].libPath)
            attachedNamespace([namespace].packageName) _
                .SetPackage([namespace]) _
                .Attach(Me)

            Return attachedNamespace([namespace].packageName)
        End Function

        ''' <summary>
        ''' add a package module from a given .NET assembly file
        ''' </summary>
        ''' <param name="pkgName">
        ''' the name of the package module inside a .NET dll file
        ''' </param>
        ''' <param name="libdll">
        ''' the file path of the target .NET dll file
        ''' </param>
        ''' <returns></returns>
        Public Function Add(pkgName$, libdll$) As PackageEnvironment
            Dim pkg As PackageEnvironment = attachedNamespace.TryGetValue(pkgName)

            If pkg Is Nothing Then
                pkg = New PackageEnvironment(env, pkgName, libdll.ParentPath)

                attachedNamespace(pkgName) = pkg
                attachedNamespace(pkgName) _
                    .SetPackage(New PackageNamespace(pkgName, libdll.ParentPath)) _
                    .Attach(Me)
            End If

            Return pkg
        End Function

        Public Function FindSymbol(namespace$, symbolName$) As RFunction
            If Not attachedNamespace.ContainsKey([namespace]) Then
                Return Nothing
            ElseIf funcOverloads.ContainsKey(symbolName) AndAlso funcOverloads(symbolName).ContainsKey([namespace]) Then
                Return funcOverloads(symbolName)([namespace])
            Else
                Dim ns As PackageEnvironment = attachedNamespace([namespace])
                Dim func As Symbol = ns.funcSymbols.TryGetValue(symbolName)

                If func IsNot Nothing AndAlso func.value.GetType.ImplementInterface(Of INamespaceReferenceSymbol) Then
                    If DirectCast(func.value, INamespaceReferenceSymbol).namespace = [namespace] Then
                        Return func.value
                    End If
                End If

                Dim symbol As Symbol = ns.FindFunction(symbolName, [inherits]:=True)
                Return symbol?.value
            End If
        End Function

        Public Iterator Function GetDllDirectories() As IEnumerable(Of String)
            For Each pkg As PackageEnvironment In Me.attachedNamespace.Values
                Dim dir As String = $"{pkg.libpath}/lib/assembly"

                Yield pkg.libpath

                If dir.DirectoryExists Then
                    Yield dir
                End If
            Next
        End Function

        ''' <summary>
        ''' load symbol via search of the manifest in package 
        ''' </summary>
        ''' <returns></returns>
        Public Function FindPackageSymbol(namespace$, symbolName$, env As Environment) As RFunction
            Dim libdir As String = PackageLoader2.GetPackageDirectory(env.globalEnvironment.options, [namespace])

            If Not libdir.DirectoryExists Then
                Return Nothing
            End If

            Dim manifest = $"{libdir}/package/manifest/symbols.json"
            Dim symbols As Dictionary(Of String, String) = manifest.LoadJsonFile(Of Dictionary(Of String, String))

            If symbols.IsNullOrEmpty Then
                Return Nothing
            ElseIf Not symbols.ContainsKey(symbolName) Then
                Return Nothing
            Else
                ' load target package directory
                ' for attatch all of the upstream dependency symbols
                Call PackageLoader2.LoadPackage(libdir, env.globalEnvironment)
            End If

            Dim symbolFile As String = $"{libdir}/lib/src/{symbols(symbolName)}"
            Dim meta As DESCRIPTION = $"{libdir}/package/index.json".LoadJsonFile(Of DESCRIPTION)

            Using bin As New BinaryReader(symbolFile.Open)
                Try
                    Dim symbolExpression As Expression = BlockReader _
                       .Read(bin) _
                       .Parse(desc:=meta)

                    Return symbolExpression.Evaluate(env)
                Catch ex As Exception
                    Throw New Exception($"Error while load symbol {[namespace]}::{symbolName} ({symbolFile}): <{ex.GetType.FullName}>{ex.Message}!")
                End Try
            End Using
        End Function

        ''' <summary>
        ''' gets all attached namespace list in json string array format.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{attachedNamespace.Count} namespace is attached: " & attachedNamespace.Keys.ToArray.GetJson
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of PackageEnvironment) Implements IEnumerable(Of PackageEnvironment).GetEnumerator
            For Each ns As PackageEnvironment In attachedNamespace.Values
                Yield ns
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
