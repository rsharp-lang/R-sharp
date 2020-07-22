#Region "Microsoft.VisualBasic::984338578db995a1c4eef8ccee313398, Library\devkit\devkit.vb"

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
'     Function: AssemblyInfo, gitLog, svnLog
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.IL
Imports Microsoft.VisualBasic.ApplicationServices.Development.VisualStudio.VersionControl
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' VisualBasic.NET application development kit
''' </summary>
<Package("VisualStudio", Category:=APICategories.UtilityTools, Publisher:="xie.guigang@live.com")>
Module devkit

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
    ''' <param name="api"></param>
    ''' <returns></returns>
    <ExportAPI("il")>
    Public Function showIL(api As RMethodInfo) As String()
        Dim il As New IL.MethodBodyReader(api.GetRawDeclares)
        Dim msil As ILInstruction() = il.ToArray

        Return msil.Select(Function(a) a.GetCode).ToArray
    End Function
End Module
