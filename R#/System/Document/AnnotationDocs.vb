#Region "Microsoft.VisualBasic::edeeb96bdd471bfafa826b0b80ae7bdd, R-sharp\R#\System\Document\AnnotationDocs.vb"

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

    '   Total Lines: 93
    '    Code Lines: 57
    ' Comment Lines: 21
    '   Blank Lines: 15
    '     File Size: 3.46 KB


    '     Class AnnotationDocs
    ' 
    '         Function: (+2 Overloads) GetAnnotations
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

        Public Function GetAnnotations(package As Type) As ProjectType
            Dim assembly As LibraryAssembly = package.Assembly
            Dim project As Project
            Dim projectKey As String = assembly.ToString
            Dim docXml As String = assembly.Location.TrimSuffix & ".xml"
            Dim type As ProjectType

            If Not docXml.FileExists Then
                ' get xml docs from app path or annotation folder
                ' in app home folder
                For Each dir As String In {"/", "Annotation", "Library"}
                    docXml = $"{App.HOME}/{dir}/{assembly.Location.BaseName}.xml"

                    If docXml.FileExists Then
                        Exit For
                    End If
                Next
            End If

            If docXml.FileExists AndAlso Not projects.ContainsKey(projectKey) Then
                projects(projectKey) = ProjectSpace.CreateDocProject(docXml)
            End If

            If projects.ContainsKey(projectKey) Then
                project = projects(projectKey)
                type = project.GetType(package.FullName)
            Else
                type = Nothing
            End If

            Return type
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
            Dim docs As ProjectMember = GetAnnotations(api.GetRawDeclares)

            If out.env <> OutputEnvironments.Html Then
                Call printConsole(api, docs)
            Else
                Call printHtml(api, docs, out)
            End If
        End Sub
    End Class
End Namespace
