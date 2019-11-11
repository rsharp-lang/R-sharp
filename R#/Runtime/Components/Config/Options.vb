
Namespace Runtime.Components.Configuration

    ''' <summary>
    ''' Data reader of <see cref="ConfigFile"/>
    ''' </summary>
    Public Class Options

        ReadOnly file As ConfigFile

        Sub New(configs As ConfigFile)
            Me.file = configs
        End Sub

    End Class
End Namespace