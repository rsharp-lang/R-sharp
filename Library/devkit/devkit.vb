#Region "Microsoft.VisualBasic::525c8de5d59111087ab4633f4a4a4fdd, Library\devkit\devkit.vb"

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
    '     Function: AssemblyInfo
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

''' <summary>
''' VisualBasic.NET application development kit
''' </summary>
<Package("VisualStudio.devkit")>
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


End Module
