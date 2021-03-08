Imports Microsoft.VisualBasic.ApplicationServices.Debugging

Namespace Development

    Public Class RuntimeError : Inherits VisualBasicAppException

        Public Sub New(ex As Exception, calls As String)
            MyBase.New(ex, calls)
        End Sub
    End Class
End Namespace