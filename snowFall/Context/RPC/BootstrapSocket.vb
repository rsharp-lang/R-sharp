#Region "Microsoft.VisualBasic::9562a1e5bb68f713015d45c1b4f4ba4b, D:/GCModeller/src/R-sharp/snowFall//Context/RPC/BootstrapSocket.vb"

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

    '   Total Lines: 247
    '    Code Lines: 142
    ' Comment Lines: 67
    '   Blank Lines: 38
    '     File Size: 9.00 KB


    '     Class BootstrapSocket
    ' 
    '         Properties: NotAvaiable, port
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: folk, NodeSetup, Run, stopSocket, ToString
    ' 
    '         Sub: (+2 Overloads) Dispose, setStatus, startAsync
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine.InteropService.Pipeline
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Parallel

Namespace Context.RPC

    <Protocol(GetType(Protocols))>
    Public Class BootstrapSocket : Implements IDisposable

        Public ReadOnly Property port As Integer
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return socket.LocalPort
            End Get
        End Property

        ReadOnly socket As TcpServicesSocket
        ReadOnly closure As Byte()
        ReadOnly uuid As Integer
        ReadOnly masterPort As Integer
        ReadOnly is_debug As Boolean = False
        ReadOnly slave_debug As Boolean = False

        Dim [stop] As Boolean = False
        Dim status2 As String

        Private disposedValue As Boolean

        ''' <summary>
        ''' error during start the socket, most error:
        ''' 
        ''' ```
        ''' System.Net.Sockets.SocketException (98): Address already in use
        ''' ```
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property NotAvaiable As Boolean = False

        ''' <summary>
        ''' Create a new socket and start the tcp socket services
        ''' </summary>
        ''' <param name="uuid"></param>
        ''' <param name="master"></param>
        ''' <param name="closure"></param>
        ''' <param name="debugPort">
        ''' [debug_only] the tcp port of this services socket.
        ''' </param>
        ''' <param name="debug"></param>
        ''' <remarks>
        ''' test is error or not on socket start via the property <see cref="NotAvaiable"/>.
        ''' </remarks>
        Sub New(uuid As Integer, master As Integer, closure As Byte(),
                Optional debugPort As Integer = -1,
                Optional debug As Boolean = False,
                Optional slave_debug As Boolean = False)

            Dim protocol As DataRequestHandler = AddressOf New ProtocolHandler(Me).HandleRequest
            Dim tcpPort As Integer

            Me.masterPort = master
            Me.uuid = uuid
            Me.closure = closure
            Me.slave_debug = slave_debug

            is_debug = debugPort > 0 OrElse debug

            ' loop until the socket run success?
            tcpPort = If(debugPort > 0, debugPort, IPCSocket.GetFirstAvailablePort)
            socket = New TcpServicesSocket(tcpPort)
            socket.ResponseHandler = protocol

            Call New Thread(AddressOf startAsync).Start()
            Call Thread.Sleep(300)
            Call setStatus("initialized")
        End Sub

        Private Sub startAsync()
            Try
                socket.Run()
                _NotAvaiable = False
            Catch ex As Exception
                _NotAvaiable = True
            End Try
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{uuid}] [{status2}] {socket}"
        End Function

        Private Sub setStatus(text As String)
            status2 = text

            If is_debug Then
                Call Console.WriteLine(ToString)
            End If
        End Sub

        Public Function Run(process As RunSlavePipeline) As RunSlavePipeline
            Dim task As (background As Thread, wait As Action, Task As Process) = folk(process)

            ' wait at this loop for wait slave node
            ' initialize execute context environment
            ' job done
            Do While Not [stop]
                Call Thread.Sleep(100)

                Try
                    ' 20221024
                    ' System.InvalidOperationException: No process is associated with this object.
                    If task.Task.HasExited Then
                        Exit Do
                    End If
                Catch ex As Exception
                    ' the task is already been exited
                    ' break the loop directly when this
                    ' error occurs
                    Exit Do
                End Try
            Loop

            ' If slave_debug Then
            ' Call App.Pause()
            ' End If

            Try
                ' Call socket.Dispose()
            Catch ex As Exception
            Finally
                Call Thread.Sleep(5000)
            End Try

            ' wait job done of running slave task
            Call task.wait()

            Return process
        End Function

        Private Function folk(process As RunSlavePipeline) As (background As Thread, wait As Action, Task As Process)
            Dim task As New Process With {
                .StartInfo = New ProcessStartInfo With {
                    .Arguments = process.Arguments,
                    .FileName = "dotnet",
                    .UseShellExecute = True
                }
            }
            Dim thread As New Thread(
                Sub()
                    ' If Not is_debug Then
                    Call Thread.Sleep(500)
                    ' Call process.Run()
                    ' Call Interaction.Shell(process.ToString, AppWinStyle.NormalFocus, Wait:=True)
                    Call task.Start()
                    Call task.WaitForExit()
                    ' End If
                End Sub)

            If is_debug Then
                Call Console.WriteLine($" -> [{process}]")
            End If

            Call setStatus("folk and wait slave node initializing")
            Call thread.Start()
            Call Threading.Thread.Sleep(1000)

            Dim wait As Action =
                Sub()
                    Call setStatus("wait task running")

                    Try
                        Do While Not task.HasExited
                            If Me.stop Then
                                Exit Do
                            Else
                                Call Thread.Sleep(100)
                            End If
                        Loop
                    Catch ex As Exception
                        ' 20221024
                        ' the process object is closed
                        ' so may throw exception
                        ' exit from here directly!
                    End Try

                    Call setStatus("job done!")
                End Sub

            Return (thread, wait, task)
        End Function

        Public Sub Kill()
            Call App.Pause()

            Me.stop = True

            Try
                Me.socket.Dispose()
            Catch ex As Exception

            End Try
        End Sub

        ''' <summary>
        ''' 2. and then send signal to notify this socket that 
        ''' slave node has been initialized, and then shutdown
        ''' this socket
        ''' 
        ''' 3. the slave node run task and request data from
        ''' the remote environment
        ''' </summary>
        ''' <param name="request"></param>
        ''' <param name="remoteAddress"></param>
        ''' <returns></returns>
        <Protocol(Protocols.Stop)>
        Public Function stopSocket(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
            Call Me.Kill()
            Return New DataPipe("OK")
        End Function

        ''' <summary>
        ''' 1. the node run environment setup and initialization 
        ''' at first via request this method
        ''' </summary>
        ''' <param name="request"></param>
        ''' <param name="remoteAddress"></param>
        ''' <returns></returns>
        <Protocol(Protocols.Initialize)>
        Public Function NodeSetup(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
            Dim payload As New List(Of Byte)

            Call payload.AddRange(BitConverter.GetBytes(uuid))
            Call payload.AddRange(BitConverter.GetBytes(masterPort))
            Call payload.AddRange(BitConverter.GetBytes(closure.Length))
            Call payload.AddRange(closure)

            Return New DataPipe(payload)
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call Me.Kill()
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
