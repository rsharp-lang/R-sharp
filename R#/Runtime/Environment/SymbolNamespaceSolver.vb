#Region "Microsoft.VisualBasic::5d4dbf1ade3a8c8bedeeadcc7e41cd1c, R-sharp\R#\Runtime\Environment\SymbolNamespaceSolver.vb"

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

    '   Total Lines: 89
    '    Code Lines: 67
    ' Comment Lines: 4
    '   Blank Lines: 18
    '     File Size: 3.55 KB


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
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime
    Public Class SymbolNamespaceSolver : Implements IEnumerable(Of NamespaceEnvironment)

        Public ReadOnly Property attachedNamespace As New Dictionary(Of String, NamespaceEnvironment)

        Default Public ReadOnly Property GetNamespace(ref As String) As NamespaceEnvironment
            Get
                Return attachedNamespace.TryGetValue(ref)
            End Get
        End Property

        Public ReadOnly Property packageNames As String()
            Get
                Return attachedNamespace.Keys.ToArray
            End Get
        End Property

        Public Function hasNamespace(pkgName As String) As Boolean
            Return attachedNamespace.ContainsKey(pkgName)
        End Function

        Public Function Add([namespace] As PackageNamespace) As NamespaceEnvironment
            attachedNamespace([namespace].packageName) = New NamespaceEnvironment([namespace].packageName, [namespace].libPath)
            Return attachedNamespace([namespace].packageName)
        End Function

        Public Function Add(pkgName$, libdll$) As NamespaceEnvironment
            attachedNamespace(pkgName) = New NamespaceEnvironment(pkgName, libdll.ParentPath)
            Return attachedNamespace(pkgName)
        End Function

        Public Function FindSymbol(namespace$, symbolName$) As RFunction
            If Not attachedNamespace.ContainsKey([namespace]) Then
                Return Nothing
            Else
                Return attachedNamespace([namespace]).symbols.TryGetValue(symbolName)
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

            Dim manifest = $"{libdir}/manifest/symbols.json"
            Dim symbols As Dictionary(Of String, String) = manifest.LoadJsonFile(Of Dictionary(Of String, String))

            If Not symbols.ContainsKey(symbolName) Then
                Return Nothing
            End If

            Dim symbolFile As String = $"{libdir}/src/{symbols(symbolName)}"
            Dim meta As DESCRIPTION = $"{libdir}/index.json".LoadJsonFile(Of DESCRIPTION)

            Using bin As New BinaryReader(symbolFile.Open)
                Dim symbolExpression = BlockReader _
                    .Read(bin) _
                    .Parse(desc:=meta)

                Return symbolExpression.Evaluate(env)
            End Using
        End Function

        Public Overrides Function ToString() As String
            Return attachedNamespace.Keys.GetJson
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of NamespaceEnvironment) Implements IEnumerable(Of NamespaceEnvironment).GetEnumerator
            For Each item As NamespaceEnvironment In attachedNamespace.Values
                Yield item
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
