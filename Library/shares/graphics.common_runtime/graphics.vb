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
        Return devlist.Pop
    End Function

    Public Sub RemoveAt(offset As Integer)
        Call devlist.RemoveAt(offset)
    End Sub

    Public Sub PushNewDevice(dev As graphicsDevice)
        Call devlist.Add(dev)
    End Sub

    Public Sub SwapDevice(a As Integer, b As Integer)
        Call devlist.Swap(a, b)
    End Sub

    ''' <summary>
    ''' a common method for create new graphics device
    ''' </summary>
    ''' <param name="dev"></param>
    ''' <param name="buffer"></param>
    ''' <param name="args"></param>
    Public Sub openNew(dev As IGraphics, buffer As Stream, args As list)
        Dim leaveOpen As Boolean() = CLRVector.asLogical(args.getBySynonyms("leaveOpen", "leave.open"))
        Dim autoCloseFile As Boolean = If(leaveOpen.IsNullOrEmpty, True, Not leaveOpen(0))
        Dim curDev = New graphicsDevice With {
            .g = dev,
            .file = buffer,
            .args = args,
            .index = devlist.Count,
            .leaveOpen = Not autoCloseFile
        }

        Call devlist.Add(curDev)
    End Sub
End Module
