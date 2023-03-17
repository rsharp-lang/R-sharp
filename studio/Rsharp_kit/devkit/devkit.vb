#Region "Microsoft.VisualBasic::17a3d8a69649598d2969468212f2d39c, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/devkit//devkit.vb"

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


    ' Code Statistics:

    '   Total Lines: 150
    '    Code Lines: 104
    ' Comment Lines: 26
    '   Blank Lines: 20
    '     File Size: 5.60 KB


    ' Module devkit
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: AssemblyInfo, decodeSourceMap, encodeSourceMap, getSourceFiles, gitLog
    '               inspect, printProject, readBannerData, readVbProject, showIL
    '               svnLog, writeCodeBanner
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.CodeSign
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.SourceMap
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.vbproj
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.vbproj.Xml
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VersionControl
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' VisualBasic.NET application development kit
''' </summary>
<Package("VisualStudio", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@live.com")>
Module devkit

    Sub New()
        Internal.ConsolePrinter.AttachConsoleFormatter(Of ILInstruction)(Function(a) DirectCast(a, ILInstruction).GetCode)
        Internal.ConsolePrinter.AttachConsoleFormatter(Of Project)(AddressOf printProject)
        Internal.ConsolePrinter.AttachConsoleFormatter(Of LicenseInfo)(Function(a) a.ToString)
    End Sub

    Private Function printProject(vbproj As Project) As String
        Return vbproj.ToString
    End Function

    ''' <summary>
    ''' Get .NET library module assembly information data.
    ''' </summary>
    ''' <param name="dllfile"></param>
    ''' <returns></returns>
    <ExportAPI("AssemblyInfo")>
    Public Function AssemblyInfo(dllfile As String) As AssemblyInfo
        Return Assembly.UnsafeLoadFrom(dllfile.GetFullPath).FromAssembly
    End Function

    <ExportAPI("sourceFiles")>
    Public Function getSourceFiles(vbproj As Project) As String()
        Return vbproj _
            .EnumerateSourceFiles(skipAssmInfo:=True, fullName:=True) _
            .ToArray
    End Function

    <ExportAPI("read.vbproj")>
    Public Function readVbProject(file As String, Optional legacy As Boolean = False) As Project
        Try
            Dim vbproj As Project = Project.Load(file)

            If Not legacy Then
                If Not vbproj.IsDotNetCoreSDK Then
                    Return Nothing
                End If
            End If

            Return vbproj
        Catch ex As Exception
            Return Nothing
        End Try
    End Function

    <ExportAPI("read.banner")>
    Public Function readBannerData(file As String) As LicenseInfo
        Return file.LoadXml(Of LicenseInfo)
    End Function

    <ExportAPI("write.code_banner")>
    Public Function writeCodeBanner(file As String, banner As LicenseInfo, Optional rootDir As String = Nothing) As CodeStatics
        If rootDir.StringEmpty Then
            rootDir = file.ParentPath
        End If

        Dim stat As CodeStatics = Nothing
        Call LicenseMgr.Insert(file, banner, rootDir, stat)
        Return stat
    End Function

    <ExportAPI("svn.log")>
    Public Function svnLog(file As String) As log()
        Return log.ParseSvnLogText(svn.getLogText(file)).ToArray
    End Function

    <ExportAPI("git.log")>
    Public Function gitLog(file As String) As log()
        Return log.ParseGitLogText(file.SolveStream).ToArray
    End Function

    ''' <summary>
    ''' show IL assembly code of the given R# api
    ''' </summary>
    ''' <param name="api">
    ''' a R# api function symbol object
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("il")>
    <RApiReturn(GetType(ILInstruction))>
    Public Function showIL(api As RMethodInfo) As Object
        Dim il As New IL.MethodBodyReader(api.GetNetCoreCLRDeclaration)
        Dim msil As ILInstruction() = il.ToArray

        Return msil
    End Function

    ''' <summary>
    ''' parse source map
    ''' </summary>
    ''' <param name="sourceMap"></param>
    ''' <returns></returns>
    <ExportAPI("sourceMap")>
    Public Function decodeSourceMap(sourceMap As String) As mappingLine()
        Return sourceMap.LoadJSON(Of sourceMap).decodeMappings.ToArray
    End Function

    <ExportAPI("sourceMap_encode")>
    Public Function encodeSourceMap(symbols As StackFrame(), file$) As sourceMap
        Return symbols.encode(file)
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="script">the file path of the target script</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("inspect")>
    Public Function inspect(script As String, Optional env As Environment = Nothing) As Object
        Dim globalEnv As GlobalEnvironment = env.globalEnvironment

        If globalEnv.polyglot.CanHandle(script) Then
            Dim parsed = globalEnv _
                .polyglot _
                .ParseScript(script, globalEnv)

            If parsed Like GetType(Message) Then
                Return parsed
            Else
                Call Console.WriteLine(parsed.ToString)
                Return Nothing
            End If
        Else
            Return Internal.debug.stop({$"unsupported script file type(*.{script.ExtensionSuffix})!"}, globalEnv)
        End If
    End Function
End Module
