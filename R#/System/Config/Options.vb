#Region "Microsoft.VisualBasic::19c3edd06c407e0a6181d8ceca209e2a, R#\System\Config\Options.vb"

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
'         Properties: [lib], [strict], digits, f64Format, HTTPUserAgent
'                     lib_loc, localConfig, log4vb_redirect, maxPrint, memoryLoad
'                     MimeType, nwarnings, stdout_multipline, verbose
' 
'         Constructor: (+2 Overloads) Sub New
' 
'         Function: getAllConfigs, getOption, setOption, ToString
' 
'         Sub: (+2 Overloads) Dispose, flush
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.My.FrameworkInternal
Imports Microsoft.VisualBasic.Net.Protocols.ContentTypes
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text.Xml.Models
Imports SMRUCC.Rsharp.Development.Package
Imports REnvironment = SMRUCC.Rsharp.Runtime.Environment

Namespace Development.Configuration

    ''' <summary>
    ''' Data reader of <see cref="ConfigFile"/>. The operations in this module will modify 
    ''' the environment configuration of the R# scripting engine and it also controls 
    ''' some of the behaviour of the sciBASIC.NET framework.
    ''' </summary>
    Public Class Options : Implements IFileReference, IDisposable

        Friend ReadOnly file As ConfigFile

        ''' <summary>
        ''' The memory cache value of the configuration.
        ''' </summary>
        ReadOnly configValues As Dictionary(Of String, String)

        Public ReadOnly Property nwarnings As Int32
            Get
                Return getOption(NameOf(nwarnings), [default]:=50)
            End Get
        End Property

        ''' <summary>
        ''' Package library repository file path.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property [lib] As String
            Get
                ' 程序包元数据库文件一般默认是
                ' 与配置文件在一个文件夹之中
                Return getOption(NameOf([lib]), [default]:=localConfig.ParentPath & "/packages.xml")
            End Get
        End Property

        ''' <summary>
        ''' config folder on unix platform is a fixed directory. 
        ''' </summary>
        Public Const UnixLib As String = "/etc/r_env/"

        ''' <summary>
        ''' the folder path for save the installed R# zip packages
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property lib_loc As String
            Get
                Dim defaultLibLoc As String

                If App.IsMicrosoftPlatform Then
                    ' 20210104
                    ' path will be ``/root/Library`` on the unix platform
                    ' this may cause environment conflicts when install in
                    ' docker env
                    defaultLibLoc = $"{Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)}/Library/R#/"
                Else
                    ' bugs fixed for config file conflicts between the host machine
                    ' and docker virtual machine.
                    defaultLibLoc = $"{UnixLib}/library/"
                End If

                Return getOption("lib.loc", [default]:=defaultLibLoc)
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
                Return getOption("max.print", [default]:=100).ParseInteger
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
                Return getOption("digits", [default]:=6).ParseInteger
            End Get
        End Property

        ''' <summary>
        ''' ``F`` or ``G``
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property f64Format As String
            Get
                Return getOption("f64.format", [default]:="G")
            End Get
        End Property

        Public ReadOnly Property HTTPUserAgent As String
            Get
                Return getOption("HTTPUserAgent", [default]:=Defaults.HTTPUserAgent)
            End Get
        End Property

        ''' <summary>
        ''' turn strict mode on?
        ''' </summary>
        ''' <returns>
        ''' option(strict = TRUE/FALSE);
        ''' </returns>
        Public ReadOnly Property [strict] As Boolean
            Get
                Return getOption("strict", [default]:="on").ParseBoolean
            End Get
        End Property

        ''' <summary>
        ''' 在R#环境之中调用命令行，输出的stdout字符串是使用多行数据返回还是整个文本返回
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property stdout_multipline As Boolean
            Get
                Return getOption("stdout.multipline", [default]:="on").ParseBoolean
            End Get
        End Property

        ''' <summary>
        ''' logical. Should R report extra information on progress? Set to TRUE by the command-line option ``--verbose``
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' debug模式下会默认将啰嗦模式打开
        ''' </remarks>
        Public ReadOnly Property verbose As Boolean
            Get
                Return getOption("verbose", [default]:="off").ParseBoolean
            End Get
        End Property

        Public ReadOnly Property log4vb_redirect As Boolean
            Get
                Return getOption("log4vb.redirect", [default]:="off").ParseBoolean
            End Get
        End Property

        Public ReadOnly Property memoryLoad As String
            Get
                Return getOption("memory.load", [default]:="light")
            End Get
        End Property

        Public ReadOnly Property python As String
            Get
                Return getOption("python.engine", [default]:="npy.dll")
            End Get
        End Property

        Public ReadOnly Property julia As String
            Get
                Return getOption("julia.engine", [default]:="njl.dll")
            End Get
        End Property

        Public ReadOnly Property typescript As String
            Get
                Return getOption("typescript.engine", [default]:="nts.dll")
            End Get
        End Property

        ''' <summary>
        ''' get default script environments
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property environments As String()
            Get
                Return getOption("environments", [default]:={python, julia, typescript}.GetJson).LoadJSON(Of String())
            End Get
        End Property

        ''' <summary>
        ''' file path for save the config file
        ''' </summary>
        ''' <returns></returns>
        Public Property localConfig As String Implements IFileReference.FilePath

        Public ReadOnly Property MimeType As ContentType() Implements IFileReference.MimeType
            Get
                Return {MIME.UnknownType}
            End Get
        End Property

        Dim saveConfig As Boolean = False

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="configs"></param>
        ''' <param name="saveConfig">
        ''' 为R解释器创建配置数据的时候，一般只是修改当前新创建的解释器的环境
        ''' 并不需要写入配置文件，这个参数总是会设置为FALSE
        ''' 
        ''' 当通过命令行修改默认参数配置的时候，才会将这个参数设置为TRUE
        ''' 保存通过命令行工具所配置的R环境选项
        ''' 新修改的配置文件将会作为默认配置作为后面启动的R脚本解释器的默认配置值
        ''' </param>
        Sub New(configs As String, saveConfig As Boolean)
            Me.New(ConfigFile.Load(configs), saveConfig)
            Me.localConfig = configs
        End Sub

        Sub New(file As ConfigFile, saveConfig As Boolean)
            Me.file = file
            Me.localConfig = ConfigFile.localConfigs
            Me.configValues = file.config _
                .ToDictionary(Function(cfg) cfg.name,
                              Function(cfg)
                                  Return cfg.text
                              End Function)
            Me.saveConfig = saveConfig

            For Each config In configValues
                Call App.JoinVariable(config.Key, config.Value)
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Function getAllConfigs() As Dictionary(Of String, String)
            Return New Dictionary(Of String, String)(configValues)
        End Function

        Public Overrides Function ToString() As String
            Return $"{configValues.Count} configuration values: {configValues.Keys.Take(5).JoinBy(", ")}..."
        End Function

        ''' <summary>
        ''' Get configuration value string, if the option is not exists in current configuration, 
        ''' then this function will create a new configuration value with use default value.
        ''' </summary>
        ''' <param name="opt"></param>
        ''' <param name="default$"></param>
        ''' <returns></returns>
        Public Function getOption(opt$, Optional default$ = Nothing, Optional env As REnvironment = Nothing) As String
            If configValues.ContainsKey(opt) Then
                Return configValues(opt)
            Else
                Return setOption(opt, [default], env)
            End If
        End Function

        ''' <summary>
        ''' Set configuration value and update the configuration database.
        ''' </summary>
        ''' <param name="opt$"></param>
        ''' <param name="value$"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' This method will also join/update a variable into the 
        ''' sciBASIC.NET framework runtime.
        ''' </remarks>
        Public Function setOption(opt$, value$, Optional env As REnvironment = Nothing) As Object
            If configValues.ContainsKey(opt) Then
                configValues(opt) = value
                file.config.First(Function(c) c.name = opt).text = value
            Else
                configValues.Add(opt, value)
                file.config.Add(New NamedValue With {.name = opt, .text = value})
            End If

            If Not env Is Nothing AndAlso opt = "strict" Then
                env.globalEnvironment.Rscript.strict = value.ParseBoolean
            ElseIf opt = "memory.load" Then
                If value = "max" Then
                    Call FrameworkInternal.ConfigMemory(MemoryLoads.Heavy)
                Else
                    Call FrameworkInternal.ConfigMemory(MemoryLoads.Light)
                End If
            End If

            Call flush()
            Call App.JoinVariable(opt, value)

            Return value
        End Function

        ''' <summary>
        ''' Save configuration file
        ''' </summary>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Sub flush()
            ' 20210603 在R之中，设置options好像并不会保存配置文件
            ' options设置仅在当前环境中有效
            If saveConfig Then
                Call file _
                    .GetXml _
                    .SaveTo(localConfig)
            End If
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
