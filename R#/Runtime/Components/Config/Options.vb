
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Runtime.Package

Namespace Runtime.Components.Configuration

    ''' <summary>
    ''' Data reader of <see cref="ConfigFile"/>
    ''' </summary>
    Public Class Options : Implements IFileReference

        ReadOnly file As ConfigFile
        ReadOnly configValues As Dictionary(Of String, String)

        ''' <summary>
        ''' Package library repository file path.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property [lib] As String
            Get
                Return getOption(NameOf([lib]), [default]:=LocalPackageDatabase.localDb)
            End Get
        End Property

        Public Property localConfig As String Implements IFileReference.FilePath

        Sub New(configs As String)
            Me.file = ConfigFile.Load(configs)
            Me.localConfig = configs
            Me.configValues = file.config _
                .ToDictionary(Function(cfg) cfg.name,
                              Function(cfg)
                                  Return cfg.text
                              End Function)
        End Sub

        Public Function getOption(opt As String, Optional default$ = Nothing) As String
            If configValues.ContainsKey(opt) Then
                Return configValues(opt)
            Else
                Return setOption(opt, [default])
            End If
        End Function

        Public Function setOption(opt$, value$) As Object
            If configValues.ContainsKey(opt) Then
                configValues(opt) = value
                file.config.First(Function(c) c.name = opt).text = value
            Else
                configValues.Add(opt, value)
                file.config.Add(New NamedValue With {.name = opt, .text = value})
            End If

            Call file.GetXml.SaveTo(localConfig)

            Return opt
        End Function

    End Class
End Namespace