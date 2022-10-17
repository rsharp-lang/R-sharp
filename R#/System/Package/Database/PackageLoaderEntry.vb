#Region "Microsoft.VisualBasic::6e13aec6da55642901912d85e378092b, R-sharp\R#\System\Package\Database\PackageLoaderEntry.vb"

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

    '   Total Lines: 147
    '    Code Lines: 88
    ' Comment Lines: 40
    '   Blank Lines: 19
    '     File Size: 4.98 KB


    '     Class PackageInfo
    ' 
    '         Properties: [namespace], category, cites, description, publisher
    '                     revision, symbols, url
    ' 
    '     Class PackageLoaderEntry
    ' 
    '         Properties: [module], basePackage
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FromLoaderInfo, GetLoader, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Development.Package

    Public Class PackageInfo

        ''' <summary>
        ''' Package name
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute>
        Public Property [namespace] As String

        ''' <summary>
        ''' Package summary information
        ''' </summary>
        ''' <returns></returns>
        <XmlText>
        Public Property description As String

        ''' <summary>
        ''' This plugins project's home page url.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <XmlAttribute>
        Public Property url As String

        ''' <summary>
        ''' Your name or E-Mail
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <XmlElement>
        Public Property publisher As String

        <XmlAttribute>
        Public Property revision As Integer

        ''' <summary>
        ''' 这个脚本模块包的文献引用列表
        ''' </summary>
        ''' <returns></returns>
        Public Property cites As String

        <XmlAttribute>
        Public Property category As APICategories = APICategories.SoftwareTools

        Public Property symbols As String()
    End Class

    <XmlType("package")>
    Public Class PackageLoaderEntry : Inherits PackageInfo

        ''' <summary>
        ''' The package loader entry information
        ''' </summary>
        ''' <returns></returns>
        Public Property [module] As TypeInfo
        ''' <summary>
        ''' the base package name of current dll module file
        ''' if current .NET assembly module file is installed
        ''' from the nuget zip package.
        ''' </summary>
        ''' <returns></returns>
        Public Property basePackage As String

        Sub New()
        End Sub

        ''' <summary>
        ''' Get package loading entry information
        ''' </summary>
        ''' <param name="exception"></param>
        ''' <returns>
        ''' load .NET context into runtme via this object value
        ''' </returns>
        Public Function GetLoader(ByRef exception As Exception) As Package
            ' 20220502 handling mzkit_win32 release
            Dim level1Parent As String = App.HOME.ParentPath
            Dim level2Parent As String = level1Parent.ParentPath
            Dim dllDirectory As String() = {
                $"{App.HOME}",
                $"{App.HOME}/Library",
                $"{App.HOME}/library",
                $"{level1Parent}/Library",
                $"{level1Parent}/library",
                $"{level2Parent}/Library",
                $"{level2Parent}/library"
            }
            Dim loader As Type = [module].GetType(
                knownFirst:=True,
                throwEx:=False,
                getException:=exception,
                searchPath:=dllDirectory
            )
            Dim info As New PackageAttribute([namespace]) With {
                .Category = category,
                .Cites = cites,
                .Description = description,
                .Publisher = publisher,
                .Revision = revision,
                .Url = url
            }

            If loader Is Nothing AndAlso Not exception Is Nothing Then
                If TypeOf exception Is DllNotFoundException Then
                    Dim dllDirectoryList As String = dllDirectory _
                        .Select(Function(dir)
                                    Return dir.GetDirectoryFullPath
                                End Function) _
                        .Distinct _
                        .JoinBy(", ")

                    exception = New DllNotFoundException($"{exception.Message}, SetDllDirectory [{dllDirectoryList}]")
                End If
            End If

            If loader Is Nothing Then
                Return Nothing
            Else
                Return New Package(info, package:=loader)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return [namespace]
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromLoaderInfo(info As Package) As PackageLoaderEntry
            Return New PackageLoaderEntry With {
                .category = info.info.Category,
                .cites = info.info.Cites,
                .description = info.info.Description,
                .[module] = New TypeInfo(info.package),
                .[namespace] = info.info.Namespace,
                .publisher = info.info.Publisher,
                .revision = info.info.Revision,
                .url = info.info.Url,
                .symbols = info.ls
            }
        End Function
    End Class
End Namespace
