Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

Public Module InteropArgumentHelper

    Public Function getSize(size As Object) As String
        If size Is Nothing Then
            Return Nothing
        End If

        Select Case size.GetType
            Case GetType(String)
                Return size
            Case GetType(String())
                Return DirectCast(size, String()).ElementAtOrDefault(Scan0)
            Case GetType(Size)
                With DirectCast(size, Size)
                    Return $"{ .Width},{ .Height}"
                End With
            Case GetType(SizeF)
                With DirectCast(size, SizeF)
                    Return $"{ .Width},{ .Height}"
                End With
            Case GetType(Integer()), GetType(Long()), GetType(Single()), GetType(Double()), GetType(Short())
                With DirectCast(size, Array)
                    Return $"{ .GetValue(0)},{ .GetValue(1)}"
                End With
            Case Else
                Return Nothing
        End Select
    End Function

    Public Function getColor(color As Object) As String
        If color Is Nothing Then
            Return Nothing
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
            Case Else
                Return Nothing
        End Select
    End Function
End Module
