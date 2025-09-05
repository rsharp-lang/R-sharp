#Region "Microsoft.VisualBasic::22c647bc5b8a88c994e2ee39d7f9608b, snowFall\Protocol\HostProtocol.vb"

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

    '   Total Lines: 26
    '    Code Lines: 17 (65.38%)
    ' Comment Lines: 4 (15.38%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 5 (19.23%)
    '     File Size: 957 B


    '     Class Host
    ' 
    '         Function: CreateProcessor, CreateSlave, SlaveTask
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Darwinism.HPC.Parallel
Imports Microsoft.VisualBasic.CommandLine.InteropService
Imports R = snowFall.CLI.Rscript

Namespace Protocol

    Public Class Host

        ''' <summary>
        ''' Create a slave task factory
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function CreateSlave(Optional debugPort As Integer? = Nothing) As SlaveTask
            Return New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask, debugPort)
        End Function

        Public Shared Function CreateProcessor() As R
            Return R.FromEnvironment(App.HOME)
        End Function

        Public Shared Function SlaveTask(processor As InteropService, port As Integer) As String
            Dim cli As String = DirectCast(processor, R).GetparallelModeCommandLine(master:=port, [delegate]:="Parallel::snowFall")
            Return cli
        End Function
    End Class
End Namespace
