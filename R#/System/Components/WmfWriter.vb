Imports System.Drawing.Imaging
Imports System.IO
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Namespace Development.Components

    Public Class WmfWriter : Implements SaveGdiBitmap

        Public Function Save(stream As Stream, format As ImageFormat) As Boolean Implements SaveGdiBitmap.Save
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace