#Region "Microsoft.VisualBasic::626cf236086a01855bca0e862aebac13, R#\Runtime\Package\PackageManager.vb"

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
    '         Properties: packageDocs
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FindPackage, InstallLocals
    ' 
    '         Sub: Flush
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components.Configuration

Namespace Runtime.Package

    Public Class PackageManager

        ReadOnly pkgDb As LocalPackageDatabase
        ReadOnly config As Options

        Public ReadOnly Property packageDocs As New AnnotationDocs

        Sub New(config As Options)
            Me.pkgDb = LocalPackageDatabase.Load(config.lib)
            Me.config = config
        End Sub

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
            Dim packageIndex = pkgDb.packages.ToDictionary(Function(pkg) pkg.namespace)
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
    End Class
End Namespace
