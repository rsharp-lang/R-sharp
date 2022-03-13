#Region "Microsoft.VisualBasic::5876ee021f0b61159c1157e6ffd279bc, snowFall\Context\RPC\MasterContext.vb"

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

'     Class MasterContext
' 
'         Properties: Protocol
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: GetSymbol, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Parallel
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

#If netcore5 = 1 Then
Imports Microsoft.VisualBasic.ComponentModel.Collection
#End If

Namespace Context.RPC

    ''' <summary>
    ''' the master node for <see cref="RemoteEnvironment"/>
    ''' </summary>
    ''' 
    <Protocol(GetType(Protocols))>
    Public Class MasterContext

        ''' <summary>
        ''' the listen port of the master node
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property port As Integer

        ''' <summary>
        ''' the R# context environment
        ''' </summary>
        ''' <remarks>
        ''' the master environment is readonly to 
        ''' the slave parallel node.
        ''' </remarks>
        ReadOnly env As Environment
        ReadOnly socket As TcpServicesSocket
        ReadOnly result As New Dictionary(Of String, Object)

        Public Shared ReadOnly Property Protocol As Long = New ProtocolAttribute(GetType(Protocols)).EntryPoint

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="env">
        ''' the R# context environment
        ''' </param>
        ''' <param name="port"></param>
        Sub New(env As Environment, Optional port As Integer = -1, Optional verbose As Boolean = False)
            Me.port = If(port <= 0, IPCSocket.GetFirstAvailablePort, port)
            Me.env = New Environment(env, "snowfall-parallel@master", isInherits:=False)
            Me.socket = New TcpServicesSocket(port, debug:=verbose OrElse port > 0)
            Me.socket.ResponseHandler = AddressOf New ProtocolHandler(Me).HandleRequest
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub push(name As String, value As Object)
            Call env.Push(name, value, [readonly]:=True)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function pop(uuid As Integer) As Object
            SyncLock result
                Return result.TryGetValue(uuid.ToString)
            End SyncLock
        End Function

        <Protocol(Protocols.PushResult)>
        Public Function PostResult(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
            Dim payload As New ResultPayload(request.ChunkBuffer)
            Dim value As Object = payload.value
            Dim uuid As Integer = payload.uuid

            SyncLock result
                result(uuid.ToString) = value
            End SyncLock

            Return New DataPipe("OK")
        End Function

        <Protocol(Protocols.GetSymbol)>
        Public Function GetSymbol(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
            Dim name As String = request.GetString(Encoding.ASCII)
            Dim target As Symbol = env.FindSymbol(name)
            Dim data As Byte()

            If Not target Is Nothing Then
                data = Serialization.GetBytes(target, env:=env)
            Else
                data = {}
            End If

            Return New DataPipe(data)
        End Function

        Public Overrides Function ToString() As String
            Return $"localhost::{port}"
        End Function

    End Class
End Namespace
