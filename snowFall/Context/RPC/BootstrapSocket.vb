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
            Get
                Return socket.LocalPort
            End Get
        End Property

        ReadOnly socket As TcpServicesSocket
        ReadOnly closure As Byte()
        ReadOnly uuid As Integer
        ReadOnly masterPort As Integer

        Sub New(uuid As Integer, master As Integer, closure As Byte())
            Me.masterPort = master
            Me.uuid = uuid
            Me.closure = closure

            socket = New TcpServicesSocket(IPCSocket.GetFirstAvailablePort)
            socket.ResponseHandler = AddressOf New ProtocolHandler(Me).HandleRequest
        End Sub

        Public Function Run(process As RunSlavePipeline) As RunSlavePipeline
            Dim wait = folk(process)

            Call socket.Run()
            Call wait()

            Return process
        End Function

        Private Function folk(process As RunSlavePipeline) As Action
            Dim thread As New Thread(Sub()
                                         Call Thread.Sleep(500)
                                         Call process.Run()
                                     End Sub)
            Call thread.Start()

            Return Sub()
                       Do While thread.ThreadState = ThreadState.Running
                           Call Thread.Sleep(1)
                       Loop
                   End Sub
        End Function

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