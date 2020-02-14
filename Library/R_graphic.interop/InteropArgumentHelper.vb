#Region "Microsoft.VisualBasic::51adbdf42b242cd79d133e5058723ac7, Library\R_graphic.interop\InteropArgumentHelper.vb"

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

' Module InteropArgumentHelper
' 
'     Function: getColor, getPadding, getSize
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D

''' <summary>
''' R# graphics argument scripting helper
''' </summary>
Public Module InteropArgumentHelper

    Public Function getVector3D(obj As Object) As Point3D
        If obj Is Nothing Then
            Return New Point3D
        ElseIf TypeOf obj Is Point3D Then
            Return DirectCast(obj, Point3D)
        ElseIf TypeOf obj Is Double() Then
            With DirectCast(obj, Double())
                If .Length >= 3 Then
                    Return New Point3D(.GetValue(0), .GetValue(1), .GetValue(2))
                ElseIf .Length = 2 Then
                    Return New Point3D(.GetValue(0), .GetValue(1), 0)
                Else
                    Return Nothing
                End If
            End With
        End If

        Return Nothing
    End Function

    Public Function getPadding(padding As Object, Optional default$ = g.DefaultPadding) As String
        If padding Is Nothing Then
            Return [default]
        End If

        Select Case padding.GetType
            Case GetType(String)
                Return padding
            Case Else
                Return [default]
        End Select
    End Function

    Public Function getSize(size As Object, Optional default$ = "2700,2000") As String
        If size Is Nothing Then
            Return [default]
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
                Return [default]
        End Select
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
            Case Else
                Return [default]
        End Select
    End Function
End Module
