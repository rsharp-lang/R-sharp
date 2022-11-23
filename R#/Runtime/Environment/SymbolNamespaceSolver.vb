#Region "Microsoft.VisualBasic::e3eb902d4bc584db03a684fc2b1a4b30, R-sharp\R#\Runtime\Environment\SymbolNamespaceSolver.vb"

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

    '   Total Lines: 104
    '    Code Lines: 76
    ' Comment Lines: 10
    '   Blank Lines: 18
    '     File Size: 4.42 KB


    '     Class SymbolNamespaceSolver
    ' 
    '         Properties: attachedNamespace, packageNames
    ' 
    '         Function: (+2 Overloads) Add, FindPackageSymbol, FindSymbol, GetEnumerator, hasNamespace
    '                   IEnumerable_GetEnumerator, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime

    Public Class SymbolNamespaceSolver : Implements IEnumerable(Of PackageEnvironment)

        Public ReadOnly Property attachedNamespace As New Dictionary(Of String, PackageEnvironment)
        Public ReadOnly Property env As GlobalEnvironment

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

        Public Function Add([namespace] As PackageNamespace) As PackageEnvironment
            attachedNamespace([namespace].packageName) = New PackageEnvironment(env, [namespace].packageName, [namespace].libPath)
            attachedNamespace([namespace].packageName).SetPackage([namespace])

            Return attachedNamespace([namespace].packageName)
        End Function

        Public Function Add(pkgName$, libdll$) As PackageEnvironment
            attachedNamespace(pkgName) = New PackageEnvironment(env, pkgName, libdll.ParentPath)
            attachedNamespace(pkgName).SetPackage(New PackageNamespace(pkgName, libdll.ParentPath))

            Return attachedNamespace(pkgName)
        End Function

        Public Function FindSymbol(namespace$, symbolName$) As RFunction
            If Not attachedNamespace.ContainsKey([namespace]) Then
                Return Nothing
            Else
                Dim ns As PackageEnvironment = attachedNamespace([namespace])
                Dim symbol = ns.FindFunction(symbolName, [inherits]:=True)

                Return symbol?.value
            End If
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
            Return attachedNamespace.Keys.GetJson
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
