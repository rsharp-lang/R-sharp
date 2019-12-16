#Region "Microsoft.VisualBasic::a72323d1fa6dfb0f879b40ab183a4a8f, R#\System\Config\Options.vb"

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
    '         Properties: [lib], digits, f64Format, HTTPUserAgent, localConfig
    '                     maxPrint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getOption, setOption
    ' 
    '         Sub: (+2 Overloads) Dispose, flush
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
    Public Class Options : Implements IFileReference, IDisposable

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

        ''' <summary>
        ''' controls the number of significant (see signif) digits to print when printing 
        ''' numeric values. It is a suggestion only. Valid values are 1...22 with default 
        ''' 7. See the note in print.default about values greater than 15.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property digits As Integer
            Get
                Return getOption("digits", [default]:=7).ParseInteger
            End Get
        End Property

        ''' <summary>
        ''' ``F`` or ``G``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property f64Format As String
            Get
                Return getOption("f64.format", [default]:="F")
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

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects).
                    Call flush()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
            Dispose(True)
            ' TODO: uncomment the following line if Finalize() is overridden above.
            ' GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class
End Namespace
