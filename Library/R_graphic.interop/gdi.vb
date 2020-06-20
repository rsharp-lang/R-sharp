Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Module gdi

    Public Function getBrush(color As Object, Optional default$ = "black") As Brush
        If color Is Nothing Then
            Return New SolidBrush(default$.TranslateColor)
        End If

        Select Case color.GetType
            Case GetType(Color)
                Return New SolidBrush(DirectCast(color, Color))
            Case GetType(String)
                Return DirectCast(color, String).GetBrush
            Case GetType(Brush)
                Return color
            Case GetType(SolidBrush)
                Return color
            Case GetType(TextureBrush)
                Return color
            Case Else
                Return New SolidBrush([default].TranslateColor)
        End Select
    End Function

    Public Function getSolidbrush(color As Object, Optional default$ = "black") As SolidBrush
        If color Is Nothing Then
            Return New SolidBrush(default$.TranslateColor)
        End If

        Select Case color.GetType
            Case GetType(Color)
                Return New SolidBrush(DirectCast(color, Color))
            Case GetType(String)
                Return DirectCast(color, String).GetBrush
            Case GetType(SolidBrush)
                Return color
            Case Else
                Return New SolidBrush([default].TranslateColor)
        End Select
    End Function
End Module
