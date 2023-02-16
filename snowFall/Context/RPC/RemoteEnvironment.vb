#Region "Microsoft.VisualBasic::107786c5b60a3300f166778643e9e368, E:/GCModeller/src/R-sharp/snowFall//Context/RPC/RemoteEnvironment.vb"

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

    '   Total Lines: 131
    '    Code Lines: 79
    ' Comment Lines: 32
    '   Blank Lines: 20
    '     File Size: 4.81 KB


    '     Class RemoteEnvironment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: buffer404, FindSymbol, getRemoteSymbol, loadRemoteSymbol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Net
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Context.RPC

    ''' <summary>
    ''' a slow accessed R# runtime environment based on 
    ''' the tcp networking.
    ''' </summary>
    ''' <remarks>
    ''' this is the remote environment which is running 
    ''' on the slave node, slave parallel code access the
    ''' data on the master node via the <see cref="FindSymbol"/>
    ''' method in this executation environment model.
    ''' </remarks>
    Public Class RemoteEnvironment : Inherits Environment

        ReadOnly master As IPEndPoint
        ReadOnly uuid As Integer
        ''' <summary>
        ''' symbols list that which is missing from the master node
        ''' </summary>
        ReadOnly missing404 As New Index(Of String)
        ReadOnly verbose As Boolean = False

        Sub New(uuid As Integer, master As IPEndPoint, parent As Environment)
            Call MyBase.New(
                parent:=parent,
                stackName:=$"snowfall_prallel_RPC_slave_node%&H{uuid.ToHexString}@{master.ToString}",
                isInherits:=False
            )

            Me.uuid = uuid
            Me.master = master
        End Sub

        ''' <summary>
        ''' find symbol at local first and then find symbol 
        ''' via tcp connection from remote
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="[inherits]"></param>
        ''' <returns>
        ''' returns nothing if symbol not found on both local and master node environment
        ''' </returns>
        Public Overrides Function FindSymbol(name As String, Optional [inherits] As Boolean = True) As Symbol
            Dim local As Symbol = MyBase.FindSymbol(name, [inherits])
            Dim missingOnMaster As Boolean

            SyncLock missing404
                missingOnMaster = name Like missing404
            End SyncLock

            If local Is Nothing AndAlso Not missingOnMaster Then
                If verbose Then
                    Call Console.WriteLine($"try find symbol: {name}")
                    Call Console.WriteLine($"but not exists on local or cache...")
                    Call Console.WriteLine($"request symbol '{name}' from master!")
                End If

                Return getRemoteSymbol(name)
            Else
                Return local
            End If
        End Function

        Private Function getRemoteSymbol(name As String) As Symbol
            Dim msg As New GetSymbol With {.name = name, .uuid = uuid}
            Dim req As New RequestStream(MasterContext.Protocol, Protocols.GetSymbol, msg)
            Dim resp = New TcpRequest(master).SendMessage(req)

            If verbose Then
                Call Console.WriteLine($"[get_symbol] {msg.GetJson}")
            End If

            Using buffer As New MemoryStream(resp.ChunkBuffer)
                If buffer.Length = 0 OrElse resp.Protocol = 404 Then
                    Return buffer404(name)
                Else
                    Return loadRemoteSymbol(name, buffer)
                End If
            End Using
        End Function

        ''' <summary>
        ''' deserialize
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="buffer"></param>
        ''' <returns></returns>
        Private Function loadRemoteSymbol(name As String, buffer As MemoryStream) As Symbol
            Dim value As Symbol

            Try
                value = Serialization.GetValue(buffer)
            Catch ex As Exception
                Throw New InvalidProgramException($"error while get remote symbol: '{name}'", ex)
            End Try

            If verbose Then
                Call Console.WriteLine($"[symbol::{name}] {value.ToString}")
            End If

            ' and then push/cache to local environment
            Call symbols.Add(name, value)

            Return value
        End Function

        Private Function buffer404(name As String) As Symbol
            If verbose Then
                Call Console.WriteLine($"[404/NOT FOUND] target symbol is also not find on master environment!")
            End If

            ' this symbol will not query in the master
            ' environment any more
            SyncLock missing404
                Call missing404.Add(name)
            End SyncLock

            ' symbol not found
            Return Nothing
        End Function
    End Class
End Namespace
