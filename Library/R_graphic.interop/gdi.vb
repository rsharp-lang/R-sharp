#Region "Microsoft.VisualBasic::159b38a5ef90e11b46e4d84e396c2d4f, Library\R_graphic.interop\gdi.vb"

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

    ' Module gdi
    ' 
    '     Function: getBrush, getSolidBrush
    ' 
    ' /********************************************************************************/

#End Region

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

    Public Function getSolidBrush(color As Object, Optional default$ = "black") As SolidBrush
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
