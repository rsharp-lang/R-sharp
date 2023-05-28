#Region "Microsoft.VisualBasic::f231ce0aed47ea84201cb48a2302e4e4, F:/GCModeller/src/R-sharp/R#//System/Package/Package.vb"

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

    '   Total Lines: 169
    '    Code Lines: 108
    ' Comment Lines: 35
    '   Blank Lines: 26
    '     File Size: 5.74 KB


    '     Class Package
    ' 
    '         Properties: [namespace], dllName, info, is_basePackage, isMissing
    '                     libPath, ls, package
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetFunction, GetFunctionInternal, GetPackageDescription, GetPackageModuleInfo, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Development.Package

    ''' <summary>
    ''' The R# package module wrapper
    ''' </summary>
    Public Class Package

        Public ReadOnly Property info As PackageAttribute

        ''' <summary>
        ''' the package assembly module.
        ''' </summary>
        ''' <returns>
        ''' the .NET context for imports methods and functions
        ''' </returns>
        Public ReadOnly Property package As Type

        Public ReadOnly Property is_basePackage As Boolean
            Get
                Return TypeOf info Is RBasePackageAttribute
            End Get
        End Property

        Public ReadOnly Property [namespace] As String
            Get
                Return info.Namespace
            End Get
        End Property

        Public ReadOnly Property libPath As String
            Get
                Return package.Assembly.Location
            End Get
        End Property

        ''' <summary>
        ''' the <see cref="package"/> assembly module is nothing means 
        ''' the current package object is missing on your filesystem.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isMissing As Boolean
            Get
                Return package Is Nothing
            End Get
        End Property

        ''' <summary>
        ''' Get all api names in this package module
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ls As String()
            Get
                Return ImportsPackage.GetAllApi(package).Keys
            End Get
        End Property

        ''' <summary>
        ''' get the .net clr dll assembly basename without extension suffix
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property dllName As String
            Get
                Return package.Assembly.Location.BaseName
            End Get
        End Property

        Dim assemblyInfoCache As AssemblyInfo

        Sub New(info As PackageAttribute, package As Type)
            Me.info = info
            Me.package = package
        End Sub

        ''' <summary>
        ''' For missing package
        ''' </summary>
        ''' <param name="loaderInfo"></param>
        Sub New(loaderInfo As PackageLoaderEntry)
            Me.info = New PackageAttribute(loaderInfo.namespace) With {
                .Category = loaderInfo.category,
                .Cites = loaderInfo.cites,
                .Description = loaderInfo.description,
                .Publisher = loaderInfo.publisher,
                .Revision = loaderInfo.revision,
                .Url = loaderInfo.url
            }
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPackageModuleInfo() As AssemblyInfo
            If assemblyInfoCache Is Nothing Then
                assemblyInfoCache = package.Assembly.FromAssembly
            End If

            Return assemblyInfoCache
        End Function

        ''' <summary>
        ''' [namespace::name -> entry_point]
        ''' </summary>
        Shared ReadOnly apiCache As New Dictionary(Of String, RMethodInfo)

        Public Function GetFunction(apiName As String) As RMethodInfo
            Dim ref$ = $"{[namespace]}::{apiName}"
            Dim api As RMethodInfo

            If Not apiCache.ContainsKey(ref) Then
                api = GetFunctionInternal(apiName)
                apiCache(ref) = api
            Else
                api = apiCache(ref)
            End If

            Return api
        End Function

        Private Function GetFunctionInternal(apiName As String) As RMethodInfo
            Dim apiHandle As NamedValue(Of MethodInfo) = ImportsPackage _
                .GetAllApi(package) _
                .FirstOrDefault(Function(api)
                                    Return api.Name = apiName
                                End Function)

            If apiHandle.IsEmpty Then
                Return Nothing
            Else
                Return New RMethodInfo(apiHandle)
            End If
        End Function

        Public Function GetPackageDescription(env As Environment, Optional remarks As Boolean = False) As String
            If isMissing Then
                Return Nothing
            Else
                Dim pkgMgr As PackageManager = env.globalEnvironment.packages
                Dim docs As String = pkgMgr.GetPackageDocuments([namespace])

                ' 20230201
                '
                ' if the package module is not a registered dll file
                ' inside the local package repository, then
                ' we should find the package document from the external
                ' location at here
                If docs Is Nothing Then
                    Dim xml = pkgMgr.packageDocs.GetAnnotations(package)

                    If Not xml Is Nothing Then
                        docs = If(remarks, xml.Remarks, xml.Summary)
                    End If
                End If

                Return docs
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{info.Namespace}: {info.Description}"
        End Function
    End Class
End Namespace
