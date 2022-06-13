Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Internal.Invokes

    Public Structure graphicsDevice

        Dim g As IGraphics
        Dim file As Stream
        Dim args As list
        Dim index As Integer

        Public ReadOnly Property isEmpty As Boolean
            Get
                Return g Is Nothing
            End Get
        End Property

        Public ReadOnly Property Background As Color
            Get
                Return g.Background
            End Get
        End Property

        Default Public ReadOnly Property Item(ref As String) As Object
            Get
                Return args.slots(ref)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{g.ToString}] {args.getNames.GetJson}"
        End Function

    End Structure
End Namespace