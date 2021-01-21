#Region "Microsoft.VisualBasic::d5c0bc2092065c59184e7491186b8eff, studio\Rsharp_kit\webKit\HttpServer.vb"

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

    ' Module HttpServer
    ' 
    '     Function: serve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If netcore5= 0 Then 

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.WebCloud.HTTPInternal.Core

<Package("http.socket", Category:=APICategories.UtilityTools)>
Public Module HttpServer

    <ExportAPI("serve")>
    Public Function serve(content$, Optional port% = -1) As HttpSocket
        Dim socket As New HttpSocket(Sub(req, rep) Call rep.WriteHTML(content), Rnd() * 30000, threads:=1)
        Dim localUrl$ = socket.localhost

        Call socket.DriverRun
        Call Process.Start(localUrl)

        Return socket
    End Function
End Module
#end if
