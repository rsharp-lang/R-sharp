#Region "Microsoft.VisualBasic::4315278d68986244476948b7011e77cf, studio\Rsharp_kit\devkit\devkit.vb"

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

' Module devkit
' 
'     Constructor: (+1 Overloads) Sub New
'     Function: AssemblyInfo, gitLog, showIL, svnLog
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.SourceMap
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VersionControl
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' VisualBasic.NET application development kit
''' </summary>
<Package("VisualStudio", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@live.com")>
Module devkit

    Sub New()
        Internal.ConsolePrinter.AttachConsoleFormatter(Of ILInstruction)(Function(a) DirectCast(a, ILInstruction).GetCode)
    End Sub

    ''' <summary>
    ''' Get .NET library module assembly information data.
    ''' </summary>
    ''' <param name="dllfile"></param>
    ''' <returns></returns>
    <ExportAPI("AssemblyInfo")>
    Public Function AssemblyInfo(dllfile As String) As AssemblyInfo
        Return Assembly.UnsafeLoadFrom(dllfile.GetFullPath).FromAssembly
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
        Dim il As New IL.MethodBodyReader(api.GetRawDeclares)
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
End Module
