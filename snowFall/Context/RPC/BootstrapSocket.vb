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

        Dim [stop] As Boolean = False
        Dim status2 As String

        Sub New(uuid As Integer, master As Integer, closure As Byte(), Optional debugPort As Integer = -1)
            Me.masterPort = master
            Me.uuid = uuid
            Me.closure = closure

            is_debug = debugPort > 0
            socket = New TcpServicesSocket(If(debugPort > 0, debugPort, IPCSocket.GetFirstAvailablePort))
            socket.ResponseHandler = AddressOf New ProtocolHandler(Me).HandleRequest

            Call setStatus("initialized")
        End Sub

        Public Overrides Function ToString() As String
            Return $"[{uuid}] [{status2}] {socket}"
        End Function

        Private Sub setStatus(text As String)
            status2 = text

            If is_debug Then
                Call Console.WriteLine(status2)
            End If
        End Sub

        Public Function Run(process As RunSlavePipeline) As RunSlavePipeline
            Dim wait As Action = folk(process)

            Call New Thread(AddressOf socket.Run).Start()

            Do While Not [stop]
                Call Thread.Sleep(1)
            Loop

            Call socket.Dispose()
            Call wait()

            Return process
        End Function

        Private Function folk(process As RunSlavePipeline) As Action
            Dim thread As New Thread(
                Sub()
                    Call Thread.Sleep(500)
                    Call process.Run()
                End Sub)

            Call setStatus("folk and wait slave node initializing")
            Call thread.Start()

            Return Sub()
                       Call setStatus("wait task running")

                       Do While thread.ThreadState = ThreadState.Running
                           Call Thread.Sleep(1)
                       Loop

                       Call setStatus("job done!")
                   End Sub
        End Function

        <Protocol(Protocols.Stop)>
        Public Function stopSocket(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
            Me.stop = True
            Return New DataPipe("OK")
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