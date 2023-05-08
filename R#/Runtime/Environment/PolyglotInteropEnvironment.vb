Namespace Runtime

    ''' <summary>
    ''' symbols solver for javascript/python reference to R# object
    ''' </summary>
    Public Class PolyglotInteropEnvironment : Inherits Environment

        Sub New(_global As GlobalEnvironment)
            Call MyBase.New(_global)
        End Sub

        Public Sub AddInteropSymbol(symbol As String, value As Object)

        End Sub
    End Class
End Namespace