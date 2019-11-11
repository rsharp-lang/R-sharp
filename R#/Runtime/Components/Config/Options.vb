
Namespace Runtime.Components.Configuration

    ''' <summary>
    ''' Data reader of <see cref="ConfigFile"/>
    ''' </summary>
    Public Class Options

        ReadOnly file As ConfigFile

        Sub New(configs As ConfigFile)
            Me.file = configs
        End Sub

        Public Function getOption(opt As String) As String

        End Function

        Public Function setOption(opt$, value$) As String

        End Function

    End Class
End Namespace