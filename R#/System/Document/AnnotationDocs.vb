#Region "Microsoft.VisualBasic::6e05466fa1d2125494af6d57dc7af03e, E:/GCModeller/src/R-sharp/R#//System/Document/AnnotationDocs.vb"

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

    '   Total Lines: 124
    '    Code Lines: 70
    ' Comment Lines: 34
    '   Blank Lines: 20
    '     File Size: 4.79 KB


    '     Class AnnotationDocs
    ' 
    '         Function: get_clr_xml_document, (+2 Overloads) GetAnnotations
    ' 
    '         Sub: PrintHelp
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Serialization
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports LibraryAssembly = System.Reflection.Assembly

Namespace Development

    ''' <summary>
    ''' Parser of the <see cref="Project"/> assembly
    ''' </summary>
    Public Class AnnotationDocs

        ReadOnly projects As New Dictionary(Of String, Project)

        ''' <summary>
        ''' get xml document of a given clr type
        ''' </summary>
        ''' <param name="package"></param>
        ''' <returns></returns>
        Public Function GetAnnotations(package As Type) As ProjectType
            Dim assembly As LibraryAssembly = package.Assembly
            Dim project As Project
            Dim projectKey As String = assembly.ToString
            Dim type As ProjectType
            Dim docXml As String = get_clr_xml_document(assembly)

            If docXml.FileExists AndAlso Not projects.ContainsKey(projectKey) Then
                projects(projectKey) = ProjectSpace.CreateDocProject(docXml)
            End If

            ' 20240304 compiler special type its full name will be nothing
            ' skip such situation
            If package.FullName IsNot Nothing AndAlso projects.ContainsKey(projectKey) Then
                project = projects(projectKey)
                type = project.GetType(package.FullName)
            Else
                type = Nothing
            End If

            Return type
        End Function

        ''' <summary>
        ''' a helper function for get the xml comment document for a given dll file
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <returns>the file path to the xml document file, could be nothing if not found</returns>
        Private Shared Function get_clr_xml_document(assembly As LibraryAssembly) As String
            Dim docXml As String = assembly.Location.TrimSuffix & ".xml"
            Dim clr_package_dir As String = assembly.Location.ParentPath
            Dim is_package_assembly As Boolean = clr_package_dir.BaseName = "assembly"
            Dim clr_docs As String = $"{clr_package_dir.ParentPath.ParentPath}/package/clr"
            Dim xml_name As String = docXml.FileName

            If docXml.FileExists Then
                Return docXml
            End If

            ' get xml docs from app path or annotation folder
            ' in app home folder
            For Each dir As String In {"/", "Annotation", "Library"}
                docXml = $"{App.HOME}/{dir}/{assembly.Location.BaseName}.xml"

                If docXml.FileExists Then
                    Return docXml
                End If
            Next

            docXml = $"{clr_docs}/{xml_name}"

            If docXml.FileExists Then
                Return docXml
            End If

            ' no related document file could be found
            Return Nothing
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="func"></param>
        ''' <param name="requireNoneNull">
        ''' 默认当没有做注释的时候，这个函数会返回空值
        ''' 反之这个参数为TRUE的时候会返回空文档对象实例
        ''' </param>
        ''' <returns></returns>
        Public Function GetAnnotations(func As MethodInfo, Optional requireNoneNull As Boolean = False) As ProjectMember
            Dim type As ProjectType = GetAnnotations(func.DeclaringType)
            Dim docs As ProjectMember

            If type Is Nothing Then
                ' 整个类型都没有xml注释
                docs = Nothing
            Else
                ' 可能目标函数对象没有xml注释
                ' 在这里假设没有重载？
                docs = type.GetMethods(func.Name).ElementAtOrDefault(Scan0)
            End If

            If docs Is Nothing AndAlso requireNoneNull Then
                docs = New ProjectMember(type)
            End If

            Return docs
        End Function

        ''' <summary>
        ''' Print help information about the given R api method 
        ''' </summary>
        ''' <param name="api"></param>
        Public Sub PrintHelp(api As RMethodInfo, out As RContentOutput)
            Dim docs As ProjectMember = GetAnnotations(api.GetNetCoreCLRDeclaration)

            If out.env <> OutputEnvironments.Html Then
                Call printConsole(api, docs)
            Else
                Call printHtml(api, docs, out)
            End If
        End Sub
    End Class
End Namespace
