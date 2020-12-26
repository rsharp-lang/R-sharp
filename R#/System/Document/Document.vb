Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace System

    Public Class Document

        Public Property title As String
        Public Property description As String
        Public Property parameters As NamedValue()
        Public Property returns As String
        Public Property author As String()
        Public Property details As String
        Public Property keywords As String()
        Public Property declares As FunctionDeclare

        Public Overrides Function ToString() As String
            Return title
        End Function

    End Class

    Public Class FunctionDeclare

        Public Property name As String
        Public Property parameters As NamedValue(Of String)()

    End Class
End Namespace