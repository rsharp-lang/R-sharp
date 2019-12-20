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

        Friend Function GetHtml(x As Object) As String
            Dim keyType As Type = x.GetType

            If keyType Is GetType(vbObject) Then
                Return GetHtml(DirectCast(x, vbObject).target)
            End If

            If RtoHtml.ContainsKey(keyType) Then
                Return RtoHtml(keyType)(x)
            Else
                Throw New InvalidProgramException(keyType.FullName)
            End If
        End Function
    End Module
End Namespace