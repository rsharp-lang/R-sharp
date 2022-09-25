#Region "Microsoft.VisualBasic::3386b53b685772159abb59393bba0c11, R-sharp\snowFall\Context\RPC\BootstrapSocket.vb"

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

'   Total Lines: 117
'    Code Lines: 90
' Comment Lines: 0
'   Blank Lines: 27
'     File Size: 3.87 KB


'     Class BootstrapSocket
' 
'         Properties: port
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: folk, NodeSetup, Run, stopSocket, ToString
' 
'         Sub: setStatus
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
    Public Class BootstrapSocket

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

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="uuid"></param>
        ''' <param name="master"></param>
        ''' <param name="closure"></param>
        ''' <param name="debugPort">
        ''' [debug_only] the tcp port of this services socket.
        ''' </param>
        ''' <param name="debug"></param>
        Sub New(uuid As Integer, master As Integer, closure As Byte(),
                Optional debugPort As Integer = -1,
                Optional debug As Boolean = False,
                Optional slave_debug As Boolean = False)

            Dim protocol As DataRequestHandler = AddressOf New ProtocolHandler(Me).HandleRequest

            Me.masterPort = master
            Me.uuid = uuid
            Me.closure = closure
            Me.slave_debug = slave_debug

            is_debug = debugPort > 0 OrElse debug

            ' loop until the socket run success?
            socket = New TcpServicesSocket(If(debugPort > 0, debugPort, IPCSocket.GetFirstAvailablePort))
            socket.ResponseHandler = protocol

            Call New Thread(AddressOf socket.Run).Start()
            Call Thread.Sleep(300)
            Call setStatus("initialized")
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
            Dim task As (background As Thread, wait As Action) = folk(process)

            ' wait at this loop for wait slave node
            ' initialize execute context environment
            ' job done
            Do While Not [stop]
                Call Thread.Sleep(1)

                If task.background.ThreadState <> ThreadState.Running Then
                    Exit Do
                End If
            Loop

            If slave_debug Then
                Call App.Pause()
            End If

            ' wait job done of running slave task
            Call socket.Dispose()
            Call task.wait()

            Return process
        End Function

        Private Function folk(process As RunSlavePipeline) As (background As Thread, wait As Action)
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

                    Do While thread.ThreadState = ThreadState.Running
                        Call Thread.Sleep(1)
                    Loop

                    Call setStatus("job done!")
                End Sub

            Return (thread, wait)
        End Function

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
            Me.stop = True
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
    End Class
End Namespace
