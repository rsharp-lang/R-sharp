#Region "Microsoft.VisualBasic::b23bae4ec8129b2decefe5a15eafeea3, F:/GCModeller/src/R-sharp/snowFall//Context/RPC/MasterContext.vb"

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

    '   Total Lines: 249
    '    Code Lines: 158
    ' Comment Lines: 51
    '   Blank Lines: 40
    '     File Size: 9.43 KB


    '     Class MasterContext
    ' 
    '         Properties: port, Protocol
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: GetSymbol, pop, popLoopSymbol, (+2 Overloads) popSymbol, PostResult
    '                   SetSymbolLogTempDir, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, push, Register, Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Net.HTTP
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Parallel
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

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
        Dim log_getsymbol_temp As String = Nothing

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

        Public Function SetSymbolLogTempDir(dir As String) As MasterContext
            log_getsymbol_temp = dir
            Return Me
        End Function

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

        ''' <summary>
        ''' get context symbol that not from the loop
        ''' </summary>
        ''' <param name="target"></param>
        ''' <returns></returns>
        Private Function popSymbol(target As Symbol) As Byte()
            'Dim vec As Object = target.ToVector.GetValue(payload.uuid)

            'vec = REnv.TryCastGenericArray({vec}, env)
            'target = New Symbol(name, vec, target.constraint, target.readonly) With {
            '    .stacktrace = target.stacktrace
            '}
            Return Serialization.GetBytes(target, env:=env)
        End Function

        Private Function popLoopSymbol(name As String, loopVal As Object) As Byte()
            Dim type = RType.GetRSharpType([loopVal].GetType)
            Dim isArray As Boolean = Not [loopVal] Is Nothing AndAlso [loopVal].GetType.IsArray
            Dim valLoop As Array

            If isArray Then
                valLoop = [loopVal]
            Else
                valLoop = {loopVal}
            End If

            valLoop = REnv.TryCastGenericArray(valLoop, env)
            type = RType.GetRSharpType(valLoop.GetType)

            Dim vec As New vector(valLoop, type)
            Dim target As New Symbol(name, vec, type.mode, [readonly]:=True) With {
                .stacktrace = env.stackTrace
            }

            Return Serialization.GetBytes(target, env:=env)
        End Function

        ''' <summary>
        ''' get loop symbol
        ''' </summary>
        ''' <param name="payload"></param>
        ''' <returns></returns>
        Private Function popSymbol(payload As GetSymbol) As Byte()
            Dim [loop] = getLoopSymbol(payload)

            If [loop].hit Then
                Return popLoopSymbol(payload.name, [loop].val)
            Else
                Return New Byte() {}
            End If
        End Function

        <Protocol(Protocols.GetSymbol)>
        Public Function GetSymbol(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
            Dim log As New List(Of String)
            Dim payload As New GetSymbol(request.ChunkBuffer)
            Dim name As String = payload.name
            Dim target As Symbol = env.FindSymbol(name)
            Dim data As Byte()

            Call log.Add(payload.ToString)
            Call log.Add("try to find in runtime environment...")

            ' target symbol is not a loop symbol
            If Not target Is Nothing Then
                Call log.Add("found object!")
                data = popSymbol(target)
            Else
                Call log.Add("is a symbol from the loop sequence probably?")
                data = popSymbol(payload)
            End If

            ' save data for run debug of the slave node
            If Not log_getsymbol_temp.StringEmpty Then
                Dim dmpfile As String = $"{log_getsymbol_temp}/{payload.uuid}/{payload.name}.cache".GetFullPath

                Call log.Add($"dump result at '{dmpfile}'")
                Call data.FlushStream(dmpfile)
            End If

            If data.IsNullOrEmpty Then
                If Not log_getsymbol_temp.StringEmpty Then
                    Call log.Add("is empty?")
                    Call log.Add("404 not found!")
                    Call log.FlushAllLines($"{log_getsymbol_temp}/{payload.uuid}/{payload.name}.log")
                End If

                Return New DataPipe(NetResponse.RFC_NOT_FOUND)
            Else
                Return New DataPipe(data)
            End If
        End Function

        Public Overrides Function ToString() As String
            If log_getsymbol_temp.StringEmpty Then
                Return $"localhost::{port}"
            Else
                Return $"localhost::{port} [{log_getsymbol_temp}]"
            End If
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call socket.Dispose()

                    For Each worker As BootstrapSocket In bootstrap.Values
                        Try
                            Call worker.Dispose()
                        Catch ex As Exception

                        End Try
                    Next
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
