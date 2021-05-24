#Region "Microsoft.VisualBasic::c471bab4f5a681d959fe82398a9094b6, studio\Rsharp_kit\roxygenNet\roxygen.vb"

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

    ' Module roxygen
    ' 
    '     Function: markdown2Html, ParseDocuments, roxygenize
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.MIME.text.markdown
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

''' <summary>
''' # In-Line Documentation for R
''' 
''' Generate your Rd documentation, 'NAMESPACE' file,
''' And collation field using specially formatted comments. Writing
''' documentation In-line With code makes it easier To keep your
''' documentation up-To-Date As your requirements change. 'roxygenNet' is
''' inspired by the 'roxygen2' system from Rstudio.
''' </summary>
<Package("roxygen", Category:=APICategories.SoftwareTools)>
Public Module roxygen

    <ExportAPI("parse")>
    Public Function ParseDocuments(script As String) As list
        Dim list As New list(GetType(Document))
        Dim R As Rscript = Rscript.AutoHandleScript(handle:=script)

        For Each item As Document In RoxygenDocument.ParseDocuments(R)
            list.slots(item.declares.name) = item
        Next

        Return list
    End Function

    ''' <summary>
    ''' ### Process a package with the Rd, namespace and collate roclets.
    ''' 
    ''' This is the workhorse function that uses roclets, the built-in document 
    ''' transformation functions, to build all documentation for a package. 
    ''' See the documentation for the individual roclets, ``rd_roclet()``, 
    ''' ``namespace_roclet()``, and for ``update_collate()``, for more details.
    ''' </summary>
    ''' <param name="package_dir">
    ''' Location of package top level directory. Default is working directory.
    ''' </param>
    ''' <returns>NULL</returns>
    ''' <remarks>
    ''' Note that roxygen2 is a dynamic documentation system: it works by inspecting 
    ''' loaded objects in the package. This means that you must be able to load 
    ''' the package in order to document it: see load for details.
    ''' </remarks>
    <ExportAPI("roxygenize")>
    Public Function roxygenize(package_dir As String) As Object
        Dim man_dir As String = $"{package_dir}/man"
        Dim meta As DESCRIPTION = DESCRIPTION.Parse($"{package_dir}/DESCRIPTION")
        Dim man As UnixManPage
        Dim Rscript As Rscript

        For Each Rfile As String In ls - l - r - "*.R" <= $"{package_dir}/R"
            Rscript = Rscript.AutoHandleScript(handle:=Rfile)

            For Each symbol As Document In RoxygenDocument.ParseDocuments(Rscript)
                man = symbol.UnixMan
                man.COPYRIGHT = $"Copyright © {meta.Author}, {meta.License} Licensed {Now.Year}"
                man.LICENSE = meta.License

                Call man.ToString.SaveTo($"{man_dir}/{symbol.declares.name}.1")
            Next
        Next

        Return Nothing
    End Function

    ''' <summary>
    ''' convert the markdown text content to html text
    ''' </summary>
    ''' <param name="markdown"></param>
    ''' <returns></returns>
    <ExportAPI("markdown2Html")>
    Public Function markdown2Html(markdown As String) As String
        Return New MarkdownHTML().Transform(text:=markdown)
    End Function
End Module
