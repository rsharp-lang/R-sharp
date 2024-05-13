#Region "Microsoft.VisualBasic::9e71b6d9069f2042cfd541e8be3815b1, R#\System\Package\NuGet\metadata\nuspec.vb"

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

    '   Total Lines: 387
    '    Code Lines: 104
    ' Comment Lines: 259
    '   Blank Lines: 24
    '     File Size: 16.22 KB


    '     Class nuspec
    ' 
    '         Properties: metadata
    ' 
    '         Function: createDllFiles, CreatePackageIndex, createRReference, ToString
    ' 
    '     Class nugetmeta
    ' 
    '         Properties: authors, copyright, dependencies, description, frameworkAssemblies
    '                     icon, id, language, license, licenseUrl
    '                     projectUrl, readme, repository, requireLicenseAcceptance, tags
    '                     title, version
    ' 
    '         Function: ToString
    ' 
    '     Class tagValue
    ' 
    '         Properties: type, url, value
    ' 
    '     Class dependencies
    ' 
    '         Properties: dependency, targetFramework
    ' 
    '     Class dependency
    ' 
    '         Properties: exclude, id, include, version
    ' 
    '     Class frameworkAssembly
    ' 
    '         Properties: assemblyName, targetFramework
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports RDependency = SMRUCC.Rsharp.Development.Package.File.Dependency

Namespace Development.Package.NuGet.metadata

    ''' <summary>
    ''' A .nuspec file is an XML manifest that contains package
    ''' metadata. This manifest is used both to build the 
    ''' package and to provide information to consumers. The 
    ''' manifest is always included in a package.
    ''' 
    ''' https://docs.microsoft.com/en-us/nuget/reference/nuspec
    ''' </summary>
    <XmlRoot("package", [Namespace]:=nuspec.xmlnamespace)>
    <XmlType("package", [Namespace]:=nuspec.xmlnamespace)>
    Public Class nuspec

        Public Const xmlnamespace As String = "http://schemas.microsoft.com/packaging/2013/05/nuspec.xsd"

        Public Property metadata As nugetmeta

        Public Overrides Function ToString() As String
            Return metadata.ToString
        End Function

        ''' <summary>
        ''' 可以通过这个依赖列表从nuget上搜索所被引用的``R#``程序包列表
        ''' </summary>
        ''' <param name="loading"></param>
        ''' <returns></returns>
        Private Shared Iterator Function createRReference(loading As RDependency(), runtime As String) As IEnumerable(Of dependencies)
            For Each pkg As RDependency In loading
                Yield New dependencies With {
                    .targetFramework = runtime,
                    .dependency = pkg.packages _
                        .SafeQuery _
                        .Select(Function(loader)
                                    Return New dependency With {
                                        .id = loader,
                                        .exclude = "Build,Analyzers",
                                        .include = pkg.library,
                                        .version = "1.0.0.0"
                                    }
                                End Function) _
                        .ToArray
                }
            Next
        End Function

        Private Shared Iterator Function createDllFiles(list As AssemblyPack) As IEnumerable(Of frameworkAssembly)
            For Each fileName As String In list.assembly.SafeQuery
                Yield New frameworkAssembly With {
                    .assemblyName = fileName.BaseName,
                    .targetFramework = list.framework
                }
            Next
        End Function

        Public Shared Function CreatePackageIndex(pkg As PackageModel) As nuspec
            Dim index As DESCRIPTION = pkg.info
            Dim loading As RDependency() = pkg.loading
            Dim metadata As New nugetmeta With {
                .authors = index.Author,
                .copyright = index.License,
                .description = index.Description,
                .id = index.Package,
                .language = "en-US",
                .license = New tagValue With {.type = "expression", .value = index.License},
                .requireLicenseAcceptance = True,
                .title = index.Title,
                .version = index.Version,
                .dependencies = createRReference(loading, CreatePackage.getRuntimeTags).ToArray,
                .frameworkAssemblies = createDllFiles(pkg.assembly).ToArray
            }

            Return New nuspec With {.metadata = metadata}
        End Function

    End Class

    ''' <summary>
    ''' ### Required metadata elements
    ''' 
    ''' Although the following elements are the minimum 
    ''' requirements For a package, you should consider 
    ''' adding the Optional metadata elements To improve 
    ''' the overall experience developers have With your 
    ''' package.
    ''' 
    ''' These elements must appear within a &lt;metadata> 
    ''' element.
    ''' </summary>
    Public Class nugetmeta

        ''' <summary>
        ''' The case-insensitive package identifier, which 
        ''' must be unique across nuget.org or whatever 
        ''' gallery the package resides in. IDs may not 
        ''' contain spaces or characters that are not valid 
        ''' for a URL, and generally follow .NET namespace 
        ''' rules. See Choosing a unique package identifier 
        ''' for guidance.
        ''' 
        ''' When uploading a package to nuget.org, the id 
        ''' field Is limited to 128 characters.
        ''' </summary>
        ''' <returns></returns>
        Public Property id As String

        ''' <summary>
        ''' A human-friendly title of the package which may 
        ''' be used in some UI displays. (nuget.org and the 
        ''' Package Manager in Visual Studio do not show 
        ''' title)
        ''' 
        ''' When uploading a package to nuget.org, the title 
        ''' field Is limited to 256 characters but Is Not used 
        ''' for any display purposes.
        ''' </summary>
        ''' <returns></returns>
        Public Property title As String

        ''' <summary>
        ''' The version of the package, following the major.minor.patch 
        ''' pattern. Version numbers may include a pre-release 
        ''' suffix as described in Package versioning.
        ''' 
        ''' When uploading a package to nuget.org, the version 
        ''' field Is limited to 64 characters.
        ''' </summary>
        ''' <returns></returns>
        Public Property version As String
        ''' <summary>
        ''' A comma-separated list of packages authors, matching 
        ''' the profile names on nuget.org. These are displayed 
        ''' in the NuGet Gallery on nuget.org and are used to 
        ''' cross-reference packages by the same authors.
        ''' 
        ''' When uploading a package to nuget.org, the authors field 
        ''' Is limited to 4000 characters.
        ''' </summary>
        ''' <returns></returns>
        Public Property authors As String
        ''' <summary>
        ''' A Boolean value specifying whether the client must 
        ''' prompt the consumer to accept the package license 
        ''' before installing the package.
        ''' </summary>
        ''' <returns></returns>
        Public Property requireLicenseAcceptance As Boolean
        ''' <summary>
        ''' Supported with NuGet 4.9.0 and above
        ''' 
        ''' An SPDX license expression Or path To a license file 
        ''' within the package, often shown In UIs Like nuget.org. 
        ''' If you're licensing the package under a common 
        ''' license, like MIT or BSD-2-Clause, use the associated 
        ''' SPDX license identifier. 
        ''' 
        ''' For example:
        ''' 
        ''' ```
        ''' &lt;LICENSE type="expression">MIT&lt;/license>
        ''' ```
        ''' 
        ''' If your package is licensed under multiple common licenses, 
        ''' you can specify a composite license using the SPDX 
        ''' expression syntax version 2.0. 
        ''' 
        ''' For example:
        ''' 
        ''' ```
        ''' &lt;LICENSE type="expression">BSD-2-Clause OR MIT&lt;/license>
        ''' ```
        ''' </summary>
        ''' <returns></returns>
        Public Property license As tagValue
        ''' <summary>
        ''' A URL for the package's license, often shown 
        ''' in UIs like nuget.org.
        ''' 
        ''' When uploading a package to nuget.org, the 
        ''' licenseUrl field Is limited to 4000 characters.
        ''' </summary>
        ''' <returns></returns>
        Public Property licenseUrl As String
        ''' <summary>
        ''' Supported with NuGet 5.3.0 and above
        ''' 
        ''' It Is a path to an image file within the package, 
        ''' often shown in UIs Like nuget.org as the package 
        ''' icon. Image file size Is limited to 1 MB. Supported 
        ''' file formats include JPEG And PNG. We recommend
        ''' an image resolution of 128x128.
        ''' </summary>
        ''' <returns></returns>
        Public Property icon As String
        ''' <summary>
        ''' A URL for the package's home page, often shown 
        ''' in UI displays as well as nuget.org.
        ''' 
        ''' When uploading a package to nuget.org, the 
        ''' projectUrl field Is limited to 4000 characters.
        ''' </summary>
        ''' <returns></returns>
        Public Property projectUrl As String
        ''' <summary>
        ''' A description of the package for UI display.
        ''' 
        ''' When uploading a package to nuget.org, the 
        ''' description field Is limited to 4000 
        ''' characters.
        ''' </summary>
        ''' <returns></returns>
        Public Property description As String
        ''' <summary>
        ''' (1.5+) Copyright details for the package.
        ''' When uploading a package to nuget.org, the 
        ''' copyright field Is limited to 4000 
        ''' characters.
        ''' </summary>
        ''' <returns></returns>
        Public Property copyright As String
        ''' <summary>
        ''' Supported with NuGet 5.10.0 preview 2 and above
        ''' 
        ''' When packing a readme file, you need to use the 
        ''' readme element to specify the package path, relative 
        ''' to the root of the package. In addition to this, 
        ''' you need to make sure that the file Is included in 
        ''' the package. Supported file formats include only 
        ''' Markdown (.md).
        ''' </summary>
        ''' <returns></returns>
        Public Property readme As String
        ''' <summary>
        ''' A space-delimited list of tags and keywords that 
        ''' describe the package and aid discoverability of 
        ''' packages through search and filtering.
        ''' 
        ''' When uploading a package to nuget.org, the tags 
        ''' field Is limited to 4000 characters.
        ''' </summary>
        ''' <returns></returns>
        Public Property tags As String
        ''' <summary>
        ''' Repository metadata, consisting of four optional 
        ''' attributes: type and url (4.0+), and branch and
        ''' commit (4.6+). These attributes allow you to map
        ''' the .nupkg to the repository that built it, with 
        ''' the potential to get as detailed as the individual 
        ''' branch name and / or commit SHA-1 hash that built 
        ''' the package. This should be a publicly available 
        ''' url that can be invoked directly by a version 
        ''' control software. It should not be an html page as 
        ''' this is meant for the computer. For linking to 
        ''' project page, use the projectUrl field, instead.
        ''' 
        ''' When uploading a package to nuget.org, the type 
        ''' attribute is limited to 100 characters and the url 
        ''' attribute is limited to 4000 characters.
        ''' </summary>
        ''' <returns></returns>
        Public Property repository As tagValue
        ''' <summary>
        ''' A collection of zero or more &lt;dependency> elements 
        ''' specifying the dependencies for the package. Each 
        ''' dependency has attributes of id, version, include 
        ''' (3.x+), and exclude (3.x+). 
        ''' </summary>
        ''' <returns></returns>
        Public Property dependencies As dependencies()
        ''' <summary>
        ''' (1.2+) A collection of zero or more &lt;frameworkAssembly> 
        ''' elements identifying .NET Framework assembly references 
        ''' that this package requires, which ensures that references 
        ''' are added to projects consuming the package. Each 
        ''' frameworkAssembly has assemblyName and targetFramework
        ''' attributes.
        ''' </summary>
        ''' <returns></returns>
        Public Property frameworkAssemblies As frameworkAssembly()
        ''' <summary>
        ''' The locale ID for the package. 
        ''' </summary>
        ''' <returns></returns>
        Public Property language As String

        Public Overrides Function ToString() As String
            Return $"[{id}_{version}] {description}"
        End Function
    End Class

    Public Class tagValue

        <XmlAttribute> Public Property type As String
        <XmlAttribute> Public Property url As String

        <XmlText>
        Public Property value As String

    End Class

    <XmlType("group")>
    Public Class dependencies

        <XmlAttribute>
        Public Property targetFramework As String
        <XmlElement>
        Public Property dependency As dependency()
    End Class

    ''' <summary>
    ''' The &lt;dependencies> element within &lt;metadata> 
    ''' contains any number of &lt;dependency> elements that 
    ''' identify other packages upon which the top-level 
    ''' package depends. 
    ''' </summary>
    Public Class dependency

        ''' <summary>
        ''' (Required) The package ID of the dependency, such 
        ''' as "EntityFramework" and "NUnit", which is the name 
        ''' of the package nuget.org shows on a package page.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property id As String
        ''' <summary>
        ''' (Required) The range of versions acceptable as a 
        ''' dependency. See Package versioning for exact syntax. 
        ''' Floating versions are not supported.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property version As String
        ''' <summary>
        ''' A comma-delimited list of include/exclude tags (see 
        ''' below) indicating of the dependency to exclude in the
        ''' final package. The default value is build,analyzers 
        ''' which can be over-written. But content/ ContentFiles 
        ''' are also implicitly excluded in the final package 
        ''' which can't be over-written. Tags specified with 
        ''' exclude take precedence over those specified with 
        ''' include. For example, include="runtime, compile" 
        ''' exclude="compile" is the same as include="runtime".
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property exclude As String
        ''' <summary>
        ''' A comma-delimited list of include/exclude tags (see 
        ''' below) indicating of the dependency to include in the
        ''' final package. The default value is all.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property include As String
    End Class

    ''' <summary>
    ''' Framework assemblies are those that are part of the .NET 
    ''' framework and should already be in the global assembly cache 
    ''' (GAC) for any given machine. By identifying those assemblies 
    ''' within the &lt;frameworkAssemblies> element, a package can 
    ''' ensure that required references are added to a project in 
    ''' the event that the project doesn't have such references 
    ''' already. Such assemblies, of course, are not included in a 
    ''' package directly.
    ''' 
    ''' The &lt;frameworkAssemblies> element contains zero Or 
    ''' more &lt;frameworkAssembly> elements
    ''' </summary>
    Public Class frameworkAssembly
        ''' <summary>
        ''' (Required) The fully qualified assembly name.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property assemblyName As String
        ''' <summary>
        ''' (Optional) Specifies the target framework to which this 
        ''' reference applies. If omitted, indicates that the reference 
        ''' applies to all frameworks. See Target frameworks for 
        ''' the exact framework identifiers.
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property targetFramework As String
    End Class
End Namespace
