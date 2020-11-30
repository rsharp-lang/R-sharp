Namespace System.Package.File

    Public Class DESCRIPTION

        Public Property Package As String
        Public Property Type As String
        Public Property Title As String
        Public Property Version As String
        Public Property [Date] As String
        Public Property Author As String
        Public Property Maintainer As String
        Public Property Description As String
        Public Property License As String
        Public Property MetaData As Dictionary(Of String, String)

        Public Shared Function Parse(file As String) As DESCRIPTION

        End Function

    End Class
End Namespace