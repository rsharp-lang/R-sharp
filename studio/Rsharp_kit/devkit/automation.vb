#Region "Microsoft.VisualBasic::44ce8fdeb7d3a75b2959e8b59e0a36f6, studio\Rsharp_kit\devkit\automation.vb"

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

    ' Module automationUtils
    ' 
    '     Function: CreateConfig, GetConfig
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Development.CommandLine
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' utils tools api for create automation pipeline script via R# scripting language.
''' </summary>
<Package("automation")>
Module automationUtils

    <RInitialize>
    Sub Main(env As Environment)
        Call ConfigJSON.LoadConfig(env)?.SetCommandLine(env)
    End Sub

    <ExportAPI("getConfig")>
    Public Function GetConfig(Optional env As Environment = Nothing) As list
        Return ConfigJSON.LoadConfig(env)?.getListConfig
    End Function

    ''' <summary>
    ''' create config.json data for given Rscript
    ''' </summary>
    ''' <param name="Rscript"></param>
    ''' <returns></returns>
    <ExportAPI("config.json")>
    Public Function CreateConfig(Rscript As ShellScript) As list
        Return ConfigJSON.BuildTemplate(Rscript).getListConfig
    End Function

End Module
