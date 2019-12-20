Imports Microsoft.VisualBasic.Serialization

Namespace Runtime.Internal

    Public Module htmlPrinter

        ReadOnly RtoHtml As New Dictionary(Of Type, IStringBuilder)

        ''' <summary>
        ''' <see cref="Object"/> -> <see cref="String"/>
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="formatter"></param>
        Public Sub AttachHtmlFormatter(Of T)(formatter As IStringBuilder)
            RtoHtml(GetType(T)) = formatter
        End Sub

        Public Function GetHtml(x As Object) As String
            Return RtoHtml(x.GetType)(x)
        End Function
    End Module
End Namespace