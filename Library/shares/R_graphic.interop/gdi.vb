﻿#Region "Microsoft.VisualBasic::159b38a5ef90e11b46e4d84e396c2d4f, R-sharp\Library\R_graphic.interop\gdi.vb"

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


    ' Code Statistics:

    '   Total Lines: 43
    '    Code Lines: 38
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 1.47 KB


    ' Module gdi
    ' 
    '     Function: getBrush, getSolidBrush
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging

#If NET48 Then
Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports LineCap = System.Drawing.Drawing2D.LineCap
Imports TextureBrush = System.Drawing.TextureBrush
#Else
Imports Pen = Microsoft.VisualBasic.Imaging.Pen
Imports Pens = Microsoft.VisualBasic.Imaging.Pens
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports Brushes = Microsoft.VisualBasic.Imaging.Brushes
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports DashStyle = Microsoft.VisualBasic.Imaging.DashStyle
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports GraphicsPath = Microsoft.VisualBasic.Imaging.GraphicsPath
Imports LineCap = Microsoft.VisualBasic.Imaging.LineCap
Imports TextureBrush = Microsoft.VisualBasic.Imaging.TextureBrush
#End If

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
