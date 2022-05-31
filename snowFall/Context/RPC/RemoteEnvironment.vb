#Region "Microsoft.VisualBasic::d9e1df1a6b29f173ad86ff27ad1dbc43, R-sharp\snowFall\Context\RPC\RemoteEnvironment.vb"

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

'   Total Lines: 73
'    Code Lines: 43
' Comment Lines: 20
'   Blank Lines: 10
'     File Size: 2.64 KB


'     Class RemoteEnvironment
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: FindSymbol, getRemoteSymbol
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
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

        Sub New(uuid As Integer, master As IPEndPoint, parent As Environment)
            Call MyBase.New(
                parent:=parent,
                stackName:=$"&H{uuid.ToHexString}@{master.ToString}",
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
        ''' <returns></returns>
        Public Overrides Function FindSymbol(name As String, Optional [inherits] As Boolean = True) As Symbol
            Dim local As Symbol = MyBase.FindSymbol(name, [inherits])

            Call Console.WriteLine($"try find symbol: {name}")

            If local Is Nothing Then
                Call Console.WriteLine($"but not exists on local or cache...")
                Call Console.WriteLine($"request symbol '{name}' from master!")

                Return getRemoteSymbol(name)
            Else
                Return local
            End If
        End Function

        Private Function getRemoteSymbol(name As String) As Symbol
            Dim msg As New GetSymbol With {.name = name, .uuid = uuid}
            Dim req As New RequestStream(MasterContext.Protocol, Protocols.GetSymbol, msg)
            Dim resp = New TcpRequest(master).SendMessage(req)

            Call Console.WriteLine($"[get_symbol] {msg.GetJson}")

            Using buffer As New MemoryStream(resp.ChunkBuffer)
                If buffer.Length = 0 OrElse resp.Protocol = 404 Then
                    Call Console.WriteLine($"[404/NOT FOUND] target symbol is also not find on master environment!")
                    ' symbol not found
                    Return Nothing
                Else
                    ' deserialize
                    Dim value As Symbol = Serialization.GetValue(buffer)
                    ' and then push/cache to local environment
                    Call Console.WriteLine($"[symbol::{name}] {value.ToString}")
                    Call symbols.Add(name, value)
                    Return value
                End If
            End Using
        End Function

    End Class
End Namespace
