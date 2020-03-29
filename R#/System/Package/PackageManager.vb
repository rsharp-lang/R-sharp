#Region "Microsoft.VisualBasic::90703acd457499ae38dc2017deb09328, R#\System\Package\PackageManager.vb"

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

    '     Class PackageManager
    ' 
    '         Properties: loadedPackages, packageDocs
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FindPackage, GenericEnumerator, GetEnumerator, GetPackageDocuments, hasLibFile
    '                   InstallLocals
    ' 
    '         Sub: (+2 Overloads) Dispose, Flush
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.System.Configuration

Namespace System.Package

    Public Class PackageManager : Implements IDisposable, Enumeration(Of Package)

        ReadOnly pkgDb As LocalPackageDatabase
        ReadOnly config As Options

        Public ReadOnly Property packageDocs As New AnnotationDocs
        Public ReadOnly Property loadedPackages As New Index(Of String)

        Sub New(config As Options)
            Me.pkgDb = LocalPackageDatabase.Load(config.lib)
            Me.config = config
        End Sub

        ''' <summary>
        ''' Check if the given dll module <paramref name="libraryFileName"/> is exists in database or not.
        ''' </summary>
        ''' <param name="libraryFileName"></param>
        ''' <returns></returns>
        Public Function hasLibFile(libraryFileName As String) As Boolean
            Return pkgDb.hasLibFile(libraryFileName)
        End Function

        Public Function GetPackageDocuments(pkgName As String) As String
            Dim type As Type = Me.FindPackage(pkgName, Nothing).package
            Dim docs As ProjectType = packageDocs.GetAnnotations(type)

            If docs Is Nothing Then
                Return Nothing
            Else
                Return docs.Summary
            End If
        End Function

        ''' <summary>
        ''' If the package is not exists or load package failure
        ''' then this function returns nothing
        ''' </summary>
        ''' <param name="packageName"></param>
        ''' <param name="exception"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindPackage(packageName As String, ByRef exception As Exception) As Package
            Return pkgDb.FindPackage(packageName, exception)
        End Function

        ''' <summary>
        ''' 在调用了这个函数进行包模块的安装之后，需要调用<see cref="Flush()"/>函数更新数据库才可以完成安装
        ''' </summary>
        ''' <param name="dllFile"></param>
        ''' <returns></returns>
        Public Function InstallLocals(dllFile As String) As String()
            Dim packageIndex As Dictionary(Of String, PackageLoaderEntry) = pkgDb.packages _
                .AsEnumerable _
                .ToDictionary(Function(pkg)
                                  Return pkg.namespace
                              End Function)
            Dim names As New List(Of String)

            For Each pkg As Package In PackageLoader.ParsePackages(dll:=dllFile)
                With PackageLoaderEntry.FromLoaderInfo(pkg)
                    ' 新的package信息会覆盖掉旧的package信息
                    packageIndex(.namespace) = .ByRef
                End With

                Call names.Add(pkg.namespace)
                Call $"load: {pkg.info.Namespace}".__INFO_ECHO
            Next

            pkgDb.packages = packageIndex.Values.ToArray
            pkgDb.system = GetType(LocalPackageDatabase).Assembly.FromAssembly

            Return names.ToArray
        End Function

        ''' <summary>
        ''' 将程序包数据库更新到硬盘文件之上
        ''' </summary>
        Public Sub Flush()
            Call pkgDb.GetXml.SaveTo(config.lib)
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call Flush()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

        Public Iterator Function GenericEnumerator() As IEnumerator(Of Package) Implements Enumeration(Of Package).GenericEnumerator
            Dim pkg As Package

            For Each loader As PackageLoaderEntry In pkgDb.packages.AsEnumerable
                pkg = loader.GetLoader(Nothing)

                If pkg Is Nothing Then
                    ' missing from current environment
                    Yield New Package(loader)
                Else
                    Yield pkg
                End If
            Next
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator Implements Enumeration(Of Package).GetEnumerator
            Yield GenericEnumerator()
        End Function
    End Class
End Namespace
