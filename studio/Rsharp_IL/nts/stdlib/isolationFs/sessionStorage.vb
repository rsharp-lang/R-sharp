#Region "Microsoft.VisualBasic::c0eaf986ef612d92ad2bd96bba0860c9, F:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//stdlib/isolationFs/sessionStorage.vb"

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

    '   Total Lines: 41
    '    Code Lines: 32
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.39 KB


    '     Module sessionStorage
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: clear, getItem, removeItem, setItem
    ' 
    ' 
    ' /********************************************************************************/

#End Region


Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace jsstd.isolationFs

    <Package("sessionStorage")>
    Public Module sessionStorage

        ReadOnly fs As Storage

        Sub New()
            fs = New Storage(TempFileSystem.GetAppSysTempFile("_session", sessionID:=App.PID.ToHexString, prefix:="sessionStorage_"))
            App.JoinVariable("js.sessionStorage", fs.ToString)
        End Sub

        <ExportAPI("clear")>
        Public Function clear() As Object
            Return fs.clear
        End Function

        <ExportAPI("removeItem")>
        Public Function removeItem(keyname As String) As Object
            Return fs.removeItem(keyname)
        End Function

        <ExportAPI("setItem")>
        Public Function setItem(key As String, <RRawVectorArgument> value As Object,
                                Optional env As Environment = Nothing) As Object
            Return fs.setItem(key, value, env)
        End Function

        <ExportAPI("getItem")>
        Public Function getItem(key As String, Optional env As Environment = Nothing) As Object
            Return fs.getItem(key, env)
        End Function
    End Module
End Namespace
