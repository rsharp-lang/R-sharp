Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.Utility
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace System

    Module UnixManPagePrinter

        Public Sub printManPage(api As RMethodInfo, docs As ProjectMember, markdown As RContentOutput)
            Dim man As UnixManPage = api.CreateManPage(docs)

            Call markdown.Write(UnixManPage.ToString(man, ""))
            Call markdown.Flush()
        End Sub

        <Extension>
        Public Function CreateManPage(api As RMethodInfo, docs As ProjectMember) As UnixManPage
            Dim package As PackageAttribute = api.GetRawDeclares.DeclaringType.GetCustomAttribute(Of PackageAttribute)
            Dim info = api.GetRawDeclares.DeclaringType.Assembly.FromAssembly
            Dim man As New UnixManPage With {
                .AUTHOR = package.Publisher,
                .BUGS = "",
                .COPYRIGHT = info.AssemblyCopyright,
                .DESCRIPTION = docs.Summary,
                .index = New Index With {
                    .category = package.Category,
                    .index = api.name,
                    .[date] = info.BuiltTime,
                    .keyword = api.name,
                    .title = api.name
                },
                .LICENSE = "",
                .NAME = api.name,
                .SEE_ALSO = package.Namespace,
                .FILES = api.GetRawDeclares.DeclaringType.Module.FullyQualifiedName
            }

            Return man
        End Function
    End Module
End Namespace