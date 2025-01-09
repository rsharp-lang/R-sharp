#Region "Microsoft.VisualBasic::243e2416975a27186fce8f74dfc8002a, Library\shares\graphics.common_runtime\graphics.vb"

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

    '   Total Lines: 62
    '    Code Lines: 42 (67.74%)
    ' Comment Lines: 9 (14.52%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 11 (17.74%)
    '     File Size: 1.83 KB


    ' Module graphics
    ' 
    '     Properties: curDev, Devices
    ' 
    '     Function: PopLastDevice
    ' 
    '     Sub: openNew, PushNewDevice, RemoveAt, SwapDevice
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Imaging
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization

Public Module graphics


    ReadOnly devlist As New List(Of graphicsDevice)

    ''' <summary>
    ''' the current actived graphics device
    ''' </summary>
    Public ReadOnly Property curDev As graphicsDevice
        Get
            Return devlist.LastOrDefault
        End Get
    End Property

    Public ReadOnly Property Devices As IEnumerable(Of graphicsDevice)
        Get
            Return devlist
        End Get
    End Property

    Public Function PopLastDevice() As graphicsDevice
        If Env.debugOpt Then
            Call VBDebugger.EchoLine($"pop out the last graphics device [{devlist.LastOrDefault}].")
        End If

        Return devlist.Pop
    End Function

    Public Sub RemoveAt(offset As Integer)
        If Env.debugOpt Then
            Call VBDebugger.EchoLine($"remove a specifc graphics device at index {offset} [{devlist.ElementAtOrDefault(offset)}].")
        End If

        Call devlist.RemoveAt(offset)
    End Sub

    Public Sub PushNewDevice(dev As graphicsDevice)
        If Env.debugOpt Then
            Call VBDebugger.EchoLine($"push a new graphics device [{dev}].")
        End If

        Call devlist.Add(dev)
    End Sub

    Public Sub SwapDevice(a As Integer, b As Integer)
        If Env.debugOpt Then
            Call VBDebugger.EchoLine($"swap two graphics device of {a} ~ {b} [{devlist(a)}] ~ [{devlist(b)}].")
        End If

        Call devlist.Swap(a, b)
    End Sub

    ''' <summary>
    ''' a common method for create new graphics device
    ''' </summary>
    ''' <param name="dev"></param>
    ''' <param name="buffer"></param>
    ''' <param name="args"></param>
    Public Sub openNew(dev As IGraphics, buffer As Stream, args As list, [function] As String)
        Dim leaveOpen As Boolean() = CLRVector.asLogical(args.getBySynonyms("leaveOpen", "leave.open"))
        Dim autoCloseFile As Boolean = If(leaveOpen.IsNullOrEmpty, True, Not leaveOpen(0))
        Dim curDev = New graphicsDevice With {
            .g = dev,
            .file = buffer,
            .args = args,
            .index = devlist.Count,
            .leaveOpen = Not autoCloseFile,
            .dev = [function]
        }

        Call PushNewDevice(curDev)
    End Sub
End Module
