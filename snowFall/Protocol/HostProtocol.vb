#Region "Microsoft.VisualBasic::5e96c67cc59ca03db9fa70c5117f4331, snowFall\Protocol\HostProtocol.vb"

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

    '     Class Host
    ' 
    '         Function: CreateProcessor, CreateSlave, SlaveTask
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.InteropService
Imports Parallel
Imports Rserver.RscriptCommandLine

Namespace Protocol

    Public Class Host

        ''' <summary>
        ''' Create a slave task factory
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function CreateSlave(Optional debugPort As Integer? = Nothing, Optional verbose As Boolean = False) As SlaveTask
            Return New SlaveTask(Host.CreateProcessor, AddressOf Host.SlaveTask, debugPort, verbose:=verbose)
        End Function

        Public Shared Function CreateProcessor() As Rscript
            Return Rscript.FromEnvironment(App.HOME)
        End Function

        Public Shared Function SlaveTask(processor As InteropService, port As Integer) As String
            Dim cli As String = DirectCast(processor, Rscript).GetparallelModeCommandLine(master:=port, [delegate]:="Parallel::snowFall")
            Return cli
        End Function
    End Class
End Namespace
