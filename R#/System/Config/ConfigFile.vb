#Region "Microsoft.VisualBasic::75c64f427003d977fa7cfd48366c5853, E:/GCModeller/src/R-sharp/R#//System/Config/ConfigFile.vb"

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

    '   Total Lines: 88
    '    Code Lines: 62
    ' Comment Lines: 14
    '   Blank Lines: 12
    '     File Size: 3.15 KB


    '     Class ConfigFile
    ' 
    '         Properties: config, localConfigs, size, startups, system
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EmptyConfigs, GenericEnumerator, GetStartupLoadingPackages, Load
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Development.Configuration

    Public Class ConfigFile : Inherits XmlDataModel
        Implements IList(Of NamedValue)

        <XmlAttribute>
        Public Property size As Integer Implements IList(Of NamedValue).size
            Get
                Return config.Length
            End Get
            Set(value As Integer)
                ' readonly
                ' do nothing
            End Set
        End Property

        <XmlElement> Public Property system As AssemblyInfo
        <XmlElement> Public Property config As NamedValue()
        <XmlElement> Public Property startups As StartupConfigs

        ''' <summary>
        ''' 脚本执行引擎的默认配置文件路径
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property localConfigs As String

        ''' <summary>
        ''' the directory path for save common config file 
        ''' of the GCModeller products. 
        ''' </summary>
        Shared ReadOnly GCModellerSettings As String

        Shared Sub New()
#If NET48 Then
            If App.IsMicrosoftPlatform Then
                GCModellerSettings = $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/{App.ProductName}/.settings"
            Else
                GCModellerSettings = $"{Options.UnixLib}/.settings"
            End If
#Else
            GCModellerSettings = $"{Options.UnixLib}/.settings"
#End If

            localConfigs = $"{GCModellerSettings}/R#.configs.xml".GetFullPath
        End Sub

        ''' <summary>
        ''' default is ``["base", "utils", "grDevices", "stats"]``
        ''' </summary>
        ''' <returns></returns>
        Public Function GetStartupLoadingPackages() As String()
            If startups Is Nothing Then
                Return StartupConfigs.DefaultLoadingPackages
            ElseIf startups.loadingPackages.IsNullOrEmpty Then
                Return StartupConfigs.DefaultLoadingPackages
            Else
                Return startups.loadingPackages
            End If
        End Function

        Public Shared Function EmptyConfigs() As ConfigFile
            Return New ConfigFile With {
                .config = {},
                .system = GetType(ConfigFile).Assembly.FromAssembly
            }
        End Function

        Public Shared Function Load(configs As String) As ConfigFile
            If configs.FileLength < 100 Then
                Return EmptyConfigs()
            Else
                Return configs.LoadXml(Of ConfigFile)
            End If
        End Function

        Public Iterator Function GenericEnumerator() As IEnumerator(Of NamedValue) Implements Enumeration(Of NamedValue).GenericEnumerator
            For Each item As NamedValue In config
                Yield item
            Next
        End Function
    End Class
End Namespace
