Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly

Namespace Runtime.Package

    ''' <summary>
    ''' Parser of the <see cref="Project"/> assembly
    ''' </summary>
    Public Class AnnotationDocs

        ReadOnly projects As New Dictionary(Of String, Project)

        Public Function GetAnnotations(package As Type) As ProjectType
            Dim assembly As Assembly = package.Assembly
            Dim project As Project
            Dim projectKey As String = assembly.ToString
            Dim docXml As String = assembly.Location.TrimSuffix & ".xml"
            Dim type As ProjectType

            If Not projects.ContainsKey(projectKey) Then
                projects(projectKey) = ProjectSpace.CreateDocProject(docXml)
            End If

            project = projects(projectKey)
            type = project.GetType(package.FullName)

            Return type
        End Function

    End Class
End Namespace