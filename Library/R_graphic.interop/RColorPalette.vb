Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Module RColorPalette

    ''' <summary>
    ''' 因为html颜色不支持透明度，所以这个函数是为了解决透明度丢失的问题而编写的
    ''' </summary>
    ''' <param name="color"></param>
    ''' <param name="default$"></param>
    ''' <returns></returns>
    Public Function GetRawColor(color As Object, Optional default$ = "black") As Color
        If color Is Nothing Then
            Return [default].TranslateColor
        End If

        Select Case color.GetType
            Case GetType(String)
                Return DirectCast(color, String).TranslateColor
            Case GetType(String())
                Return DirectCast(DirectCast(color, String()).GetValue(Scan0), String).TranslateColor
            Case GetType(Color)
                Return DirectCast(color, Color)
            Case GetType(Integer), GetType(Long), GetType(Short)
                Return color.ToString.TranslateColor
            Case GetType(Integer()), GetType(Long()), GetType(Short())
                Return DirectCast(color, Array).GetValue(Scan0).ToString.TranslateColor
            Case GetType(SolidBrush)
                Return DirectCast(color, SolidBrush).Color
            Case Else
                Return [default].TranslateColor
        End Select
    End Function

    Public Function getColorSet(colorSet As Object, Optional default$ = "Set1:c12") As String
        If colorSet Is Nothing Then
            Return [default]
        End If

        Dim type As Type = colorSet.GetType

        If type.IsArray Then
            If type.GetElementType Is GetType(String) Then
                Return DirectCast(colorSet, String()).JoinBy(",")
            ElseIf type.GetElementType Is GetType(Color) Then
                Return DirectCast(colorSet, Color()).Select(Function(c) c.ToHtmlColor).JoinBy(",")
            Else
                Return [default]
            End If
        ElseIf type Is GetType(String) Then
            Return DirectCast(colorSet, String)
        Else
            Return [default]
        End If
    End Function

    Public Function getColors(colorSet As Object, levels As Integer, Optional default$ = "Set1:c12") As String()
        If colorSet Is Nothing Then
            Return getColorSequence([default], levels)
        End If

        If TypeOf colorSet Is vector Then
            colorSet = DirectCast(colorSet, vector).data
        End If

        Dim type As Type = colorSet.GetType

        If type.IsArray Then
            If type.GetElementType Is GetType(String) Then
                Dim array As String() = DirectCast(colorSet, String())

                If array.Length >= levels Then
                    Return array
                Else
                    Return getColorSequence(DirectCast(colorSet, String()).JoinBy(","), levels)
                End If
            ElseIf type.GetElementType Is GetType(Color) Then
                Dim colors As Color() = DirectCast(colorSet, Color())

                If colors.Length >= levels Then
                    Return colors _
                        .Select(Function(c) c.ToHtmlColor) _
                        .ToArray
                Else
                    Return getColorSequence(colors.Select(Function(c) c.ToHtmlColor).JoinBy(","), levels)
                End If
            Else
                Return getColorSequence([default], levels)
            End If
        ElseIf type Is GetType(String) Then
            Return getColorSequence(DirectCast(colorSet, String), levels)
        Else
            Return getColorSequence([default], levels)
        End If
    End Function

    Private Function getColorSequence(colorSet As String, levels As Integer) As String()
        Return Designer _
            .GetColors(DirectCast(colorSet, String), levels) _
            .Select(Function(c) c.ToHtmlColor) _
            .ToArray
    End Function

    Public Function getColor(color As Object, Optional default$ = "black") As String
        If color Is Nothing Then
            Return [default]
        End If

        Select Case color.GetType
            Case GetType(String)
                Return color
            Case GetType(String())
                Return DirectCast(color, String()).GetValue(Scan0)
            Case GetType(Color)
                Return DirectCast(color, Color).ToHtmlColor
            Case GetType(Integer), GetType(Long), GetType(Short)
                Return color.ToString
            Case GetType(Integer()), GetType(Long()), GetType(Short())
                Return DirectCast(color, Array).GetValue(Scan0).ToString
            Case GetType(SolidBrush)
                Return DirectCast(color, SolidBrush).Color.ToHtmlColor
            Case Else
                Return [default]
        End Select
    End Function
End Module
