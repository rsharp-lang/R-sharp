#Region "Microsoft.VisualBasic::43e366a08b8da6d1aac5e2b881c36a20, G:/GCModeller/src/R-sharp/snowFall//Context/RPC/Protocols.vb"

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

    '   Total Lines: 71
    '    Code Lines: 49
    ' Comment Lines: 6
    '   Blank Lines: 16
    '     File Size: 2.03 KB


    '     Enum Protocols
    ' 
    '         [Stop], GetSymbol, Initialize, PushResult
    ' 
    '  
    ' 
    ' 
    ' 
    '     Class GetSymbol
    ' 
    '         Properties: name, uuid
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: ToString
    ' 
    '         Sub: Serialize
    ' 
    '     Class ResultPayload
    ' 
    '         Properties: uuid, value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Sub: Serialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Serialization
Imports SMRUCC.Rsharp.Runtime

Namespace Context.RPC

    Public Enum Protocols As Long
        Initialize
        ''' <summary>
        ''' get symbol value
        ''' </summary>
        GetSymbol

        ''' <summary>
        ''' the slave node push the result back to the master node
        ''' </summary>
        PushResult
        [Stop]
    End Enum

    Public Class GetSymbol : Inherits RawStream

        Public Property uuid As Integer
        Public Property name As String

        Sub New()
        End Sub

        Sub New(payload As Byte())
            uuid = BitConverter.ToInt32(payload, Scan0)
            name = Encoding.UTF8.GetString(payload, 4, payload.Length - 4)
        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Call buffer.Write(BitConverter.GetBytes(uuid), Scan0, 4)
            Call buffer.Write(name, Encoding.UTF8)
            Call buffer.Flush()
        End Sub

        Public Overrides Function ToString() As String
            Return $"&Hx00{uuid} get symbol '{name}'"
        End Function
    End Class

    Public Class ResultPayload : Inherits RawStream

        Public Property uuid As Integer
        Public Property value As Object

        Dim env As Environment

        Sub New(env As Environment)
            Me.env = env
        End Sub

        Sub New(payload As Byte(), env As Environment)
            Me.uuid = BitConverter.ToInt32(payload, Scan0)
            Me.value = Serialization.ParseBuffer(payload.Skip(4).ToArray)
            Me.env = env
        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Dim data As Byte() = Serialization.GetBuffer(value, env)

            Call buffer.Write(BitConverter.GetBytes(uuid), Scan0, 4)
            Call buffer.Write(data, Scan0, data.Length)
            Call buffer.Flush()
        End Sub
    End Class
End Namespace
