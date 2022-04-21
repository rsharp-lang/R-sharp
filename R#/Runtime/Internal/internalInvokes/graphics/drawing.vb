Imports System.Drawing
Imports System.Drawing.Imaging
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Runtime.Internal.Invokes

    Module drawing

        <ExportAPI("new_bitmap")>
        Public Function new_bitmap(width As Integer, height As Integer) As Bitmap
            Return New Bitmap(width, height, format:=PixelFormat.Format32bppArgb)
        End Function
    End Module
End Namespace