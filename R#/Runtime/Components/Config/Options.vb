
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text.Xml.Models

Namespace Runtime.Components.Configuration

    ''' <summary>
    ''' Data reader of <see cref="ConfigFile"/>
    ''' </summary>
    Public Class Options : Implements IFileReference

        ReadOnly file As ConfigFile
        ReadOnly configValues As Dictionary(Of String, String)

        Public ReadOnly Property [lib] As String
            Get
                Return getOption(NameOf([lib]))
            End Get
        End Property

        Public Property localConfig As String Implements IFileReference.FilePath

        Sub New(configs As String)
            Me.file = ConfigFile.Load(configs)
            Me.localConfig = configs
        End Sub

        Public Function getOption(opt As String) As String
            If configValues.ContainsKey(opt) Then
                Return configValues(opt)
            Else
                Return Nothing
            End If
        End Function

        Public Function setOption(opt$, value$, envir As Environment) As Object
            If configValues.ContainsKey(opt) Then
                configValues(opt) = value
                file.config.First(Function(c) c.name = opt).text = value
            Else
                configValues.Add(opt, value)
                file.config.Add(New NamedValue With {.name = opt, .text = value})
            End If

            Try
                Return file.GetXml.SaveTo(localConfig)
            Catch ex As Exception
                Return Internal.stop(ex, envir)
            End Try
        End Function

    End Class
End Namespace