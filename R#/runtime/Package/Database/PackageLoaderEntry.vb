Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Package

    Public Class PackageLoaderEntry

        <XmlAttribute>
        Public Property [namespace] As String
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
        Public Property [module] As TypeInfo

        Sub New()
        End Sub

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
                .url = info.info.Url
            }
        End Function
    End Class
End Namespace