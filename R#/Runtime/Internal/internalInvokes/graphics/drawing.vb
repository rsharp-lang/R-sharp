Imports System.Drawing
Imports System.Drawing.Imaging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging

Namespace Runtime.Internal.Invokes

    Module internalDrawing

        <ExportAPI("gdi_canvas")>
        Public Function new_bitmap(width As Integer, height As Integer) As IGraphics
            Return New Size(width, height).CreateGDIDevice(filled:=Color.Transparent)
        End Function

    End Module
End Namespace