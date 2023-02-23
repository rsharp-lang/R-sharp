#Region "Microsoft.VisualBasic::f85a2d666f84645346e8f10ad0e0d625, D:/GCModeller/src/R-sharp/snowFall//Context/RPC/MasterContext.vb"

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

    '   Total Lines: 162
    '    Code Lines: 96
    ' Comment Lines: 40
    '   Blank Lines: 26
    '     File Size: 6.17 KB


    '     Class MasterContext
    ' 
    '         Properties: port, Protocol
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetSymbol, pop, PostResult, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, push, Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Parallel
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

#If netcore5 = 1 Then
Imports Microsoft.VisualBasic.ComponentModel.Collection
#End If

Namespace Context.RPC

    ''' <summary>
    ''' the master node for <see cref="RemoteEnvironment"/>
    ''' </summary>
    ''' 
    <Protocol(GetType(Protocols))>
    Public Class MasterContext : Implements IDisposable

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
        Friend ReadOnly env As Environment

        ReadOnly socket As TcpServicesSocket
        ReadOnly result As New Dictionary(Of String, Object)
        ReadOnly bootstrap As New Dictionary(Of String, BootstrapSocket)

        Dim getLoopSymbol As Func(Of GetSymbol, (hit As Boolean, val As Object))
        Dim disposedValue As Boolean

        Public Shared ReadOnly Property Protocol As Long = New ProtocolAttribute(GetType(Protocols)).EntryPoint

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="env">
        ''' the R# context environment
        ''' </param>
        ''' <param name="port">
        ''' the master tcp port
        ''' </param>
        Sub New(env As Environment, Optional port As Integer = -1, Optional verbose As Boolean = False)
            Me.port = If(port <= 0, IPCSocket.GetFirstAvailablePort, port)
            Me.env = New Environment(env, "snowfall-parallel@master", isInherits:=False)
            Me.socket = New TcpServicesSocket(Me.port, debug:=verbose OrElse port > 0)
            Me.socket.ResponseHandler = AddressOf New ProtocolHandler(Me).HandleRequest
        End Sub

        Public Sub Run(getLoopSymbol As Func(Of GetSymbol, (hit As Boolean, val As Object)))
            Me.getLoopSymbol = getLoopSymbol
            Me.socket.Run()
        End Sub

        Public Sub Register(uuid As Integer, slave As BootstrapSocket)
            SyncLock bootstrap
                bootstrap(uuid.ToString) = slave
            End SyncLock
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
            Dim payload As New ResultPayload(request.ChunkBuffer, env)
            Dim value As Object = payload.value
            Dim uuid As Integer = payload.uuid
            Dim uuidKey As String = uuid.ToString

            SyncLock result
                result(uuidKey) = value
            End SyncLock
            SyncLock bootstrap
                If bootstrap.ContainsKey(uuidKey) Then
                    Try
                        Call bootstrap(uuidKey).Kill()
                    Catch ex As Exception

                    End Try
                End If
            End SyncLock

            Return New DataPipe("OK")
        End Function

        <Protocol(Protocols.GetSymbol)>
        Public Function GetSymbol(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
            Dim payload As New GetSymbol(request.ChunkBuffer)
            Dim name As String = payload.name
            Dim target As Symbol = env.FindSymbol(name)
            Dim data As Byte()

            ' 
            If Not target Is Nothing Then
                If TypeOf target.value Is RMethodInfo Then
                    data = {}
                Else
                    'Dim vec As Object = target.ToVector.GetValue(payload.uuid)

                    'vec = REnv.TryCastGenericArray({vec}, env)
                    'target = New Symbol(name, vec, target.constraint, target.readonly) With {
                    '    .stacktrace = target.stacktrace
                    '}
                    data = Serialization.GetBytes(target, env:=env)
                End If
            Else
                Dim [loop] = getLoopSymbol(payload)

                If [loop].hit Then
                    Dim type = RType.GetRSharpType([loop].val.GetType)
                    Dim isArray As Boolean = Not [loop].val Is Nothing AndAlso [loop].val.GetType.IsArray
                    Dim valLoop As Array

                    If isArray Then
                        valLoop = [loop].val
                    Else
                        valLoop = {[loop].val}
                    End If

                    Dim vec As New vector(valLoop, type)

                    target = New Symbol(name, vec, TypeCodes.generic, [readonly]:=True) With {
                        .stacktrace = env.stackTrace
                    }
                    data = Serialization.GetBytes(target, env:=env)
                Else
                    data = {}
                End If
            End If

            Return New DataPipe(data)
        End Function

        Public Overrides Function ToString() As String
            Return $"localhost::{port}"
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call socket.Dispose()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace
