Imports Parallel
Imports SMRUCC.Rsharp.Runtime
Imports System.IO
Imports System.Text
Imports System.Threading
Imports Microsoft.VisualBasic.ComponentModel
#If netcore5 = 1 Then
Imports Microsoft.VisualBasic.ComponentModel.Collection
#End If
Imports Microsoft.VisualBasic.Net.Protocols.Reflection
Imports Microsoft.VisualBasic.Net.Tcp
Imports Microsoft.VisualBasic.Parallel
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Parallel.IpcStream
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Context.RPC

    ''' <summary>
    ''' the master node for <see cref="RemoteEnvironment"/>
    ''' </summary>
    ''' 
    <Protocol(GetType(Protocols))>
    Public Class MasterContext

        ReadOnly port As Integer

        ''' <summary>
        ''' the R# context environment
        ''' </summary>
        ReadOnly env As Environment
        ReadOnly socket As TcpServicesSocket

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
            Me.env = env
            Me.socket = New TcpServicesSocket(port, debug:=verbose OrElse port > 0)
            Me.socket.ResponseHandler = AddressOf New ProtocolHandler(Me).HandleRequest
        End Sub

        <Protocol(Protocols.GetSymbol)>
        Public Function GetSymbol(request As RequestStream, remoteAddress As System.Net.IPEndPoint) As BufferPipe
            Dim name As String = request.GetString(Encoding.ASCII)
            Dim target As Symbol = env.FindSymbol(name)
            Dim data As Byte()

            If Not target Is Nothing Then
                data = Serialization.GetBytes(target)
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