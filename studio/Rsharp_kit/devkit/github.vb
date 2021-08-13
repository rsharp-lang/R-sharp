﻿#Region "Microsoft.VisualBasic::a77eca48134caf61fd0f18e0fe48c84b, studio\Rsharp_kit\devkit\github.vb"

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

' Module github
' 
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

<Package("github")>
Module github

    ''' <summary>
    ''' install package from github repository
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("install")>
    Public Function install(source As String, Optional env As Environment = Nothing) As Object

    End Function

    <ExportAPI("hotLoad")>
    Public Function hotLoad(source As String, Optional env As Environment = Nothing) As Object

    End Function
End Module
