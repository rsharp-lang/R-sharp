#Region "Microsoft.VisualBasic::04232ed0ec67ca12c9d21fbd4d3a0e3f, R#\Runtime\Components\Config\Options.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Class Options
    ' 
    '         Properties: [lib], localConfig
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: getOption, setOption
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Runtime.Package

Namespace Runtime.Components.Configuration

    ''' <summary>
    ''' Data reader of <see cref="ConfigFile"/>
    ''' </summary>
    Public Class Options : Implements IFileReference

        ReadOnly file As ConfigFile

        ''' <summary>
        ''' The memory cache value of the configuration.
        ''' </summary>
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

        ''' <summary>
        ''' Max count number for print vector. integer, defaulting to 999. 
        ''' print or show methods can make use of this option, to limit the 
        ''' amount of information that is printed, to something in the 
        ''' order of (and typically slightly less than) max.print entries.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property maxPrint As Integer
            Get
                Return getOption("max.print", [default]:=999).ParseInteger
            End Get
        End Property

        Public ReadOnly Property HTTPUserAgent As String
            Get
                Return getOption("HTTPUserAgent", [default]:=Defaults.HTTPUserAgent)
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

        ''' <summary>
        ''' Get configuration value string, if the option is not exists in current configuration, 
        ''' then this function will create a new configuration value with use default value.
        ''' </summary>
        ''' <param name="opt"></param>
        ''' <param name="default$"></param>
        ''' <returns></returns>
        Public Function getOption(opt As String, Optional default$ = Nothing) As String
            If configValues.ContainsKey(opt) Then
                Return configValues(opt)
            Else
                Return setOption(opt, [default])
            End If
        End Function

        ''' <summary>
        ''' Set configuration value and update the configuration database.
        ''' </summary>
        ''' <param name="opt$"></param>
        ''' <param name="value$"></param>
        ''' <returns></returns>
        Public Function setOption(opt$, value$) As Object
            If configValues.ContainsKey(opt) Then
                configValues(opt) = value
                file.config.First(Function(c) c.name = opt).text = value
            Else
                configValues.Add(opt, value)
                file.config.Add(New NamedValue With {.name = opt, .text = value})
            End If

            Call flush()

            Return opt
        End Function

        ''' <summary>
        ''' Save configuration file
        ''' </summary>
        Public Sub flush()
            Call file _
                .GetXml _
                .SaveTo(localConfig)
        End Sub

    End Class
End Namespace
