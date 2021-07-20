﻿#Region "Microsoft.VisualBasic::b953121ea8ea9b552f82f02cb507d0af, Library\R_graphic.interop\InteropArgumentHelper.vb"

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
    '     Function: getColor, getColorSet, getFontCSS, getPadding, GetRawColor
    '               getSize, getStrokePenCSS, getVector2D, getVector3D, paddingFromNumbers
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.MIME.Html.CSS
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime

''' <summary>
''' R# graphics argument scripting helper
''' </summary>
Public Module InteropArgumentHelper

    Public Function getVector2D(obj As Object) As PointF
        If TypeOf obj Is vector Then
            obj = DirectCast(obj, vector).data
        End If

        If obj Is Nothing Then
            Return New PointF
        ElseIf TypeOf obj Is PointF Then
            Return DirectCast(obj, PointF)
        ElseIf TypeOf obj Is Point Then
            Return DirectCast(obj, Point).PointF
        ElseIf TypeOf obj Is Double() Then
            With DirectCast(obj, Double())
                If .Length >= 2 Then
                    Return New PointF(.GetValue(0), .GetValue(1))
                Else
                    Return Nothing
                End If
            End With
        End If

        Return Nothing
    End Function

    Public Function getVector3D(obj As Object) As Point3D
        If TypeOf obj Is vector Then
            obj = DirectCast(obj, vector).data
        End If

        If obj Is Nothing Then
            Return New Point3D
        ElseIf TypeOf obj Is Point3D Then
            Return DirectCast(obj, Point3D)
        ElseIf TypeOf obj Is Double() OrElse TypeOf obj Is Integer() OrElse TypeOf obj Is Long() Then
            With DirectCast(REnv.asVector(Of Double)(obj), Double())
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

        If TypeOf padding Is vector Then
            padding = DirectCast(padding, vector).data
        End If

        Select Case padding.GetType
            Case GetType(String)
                Return padding
            Case GetType(String())
                Dim strs As String() = DirectCast(padding, String())

                If strs.Length = 1 Then
                    Return strs(Scan0)
                ElseIf strs.Length = 4 AndAlso strs.Take(4).All(Function(val) val.IsNumeric) Then
                    Return strs.Take(4).Select(AddressOf Val).ToArray.paddingFromNumbers(default$)
                Else
                    Return [default]
                End If
            Case GetType(Long()), GetType(Integer())
                Return DirectCast(padding, Array).paddingFromNumbers(default$)
            Case GetType(vector)
                Return DirectCast(padding, vector).data.paddingFromNumbers(default$)
            Case Else
                Return [default]
        End Select
    End Function

    <Extension>
    Private Function paddingFromNumbers(data As Array, default$) As String
        If data.Length = 1 Then
            Dim x As String = data.GetValue(Scan0).ToString

            Return $"padding: {x}px {x}px {x}px {x}px;"
        ElseIf data.Length = 4 Then
            Return $"padding: {data.GetValue(0)}px {data.GetValue(1)}px {data.GetValue(2)}px {data.GetValue(3)}px;"
        Else
            Return [default]
        End If
    End Function

    Public Function getSize(size As Object, env As Environment, Optional default$ = "2700,2000") As String
        If size Is Nothing Then
            Return [default]
        ElseIf TypeOf size Is vector Then
            size = DirectCast(size, vector).data
        End If

        If size.GetType.IsArray Then
            ' cast object() to integer()/string(), etc
            size = REnv.TryCastGenericArray(size, env)
        End If

        Dim sizeType As Type = size.GetType

        Select Case sizeType
            Case GetType(String)
                Return size
            Case GetType(String())
                Dim strs As String() = DirectCast(size, String())

                If strs.Length = 1 Then
                    Return strs(Scan0)
                ElseIf strs(Scan0).IsNumeric AndAlso strs(1).IsNumeric Then
                    Return $"{strs(Scan0)},{strs(1)}"
                Else
                    Return [default]
                End If
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
                Call $"invalid data type for get [width,height]: {sizeType.FullName}".Warning

                Return [default]
        End Select
    End Function

    Public Function getFontCSS(font As Object, Optional default$ = CSSFont.Win7Large) As String
        If font Is Nothing Then
            Return [default]
        End If

        Select Case font.GetType
            Case GetType(String)
                Return font
            Case GetType(Font)
                Return New CSSFont(DirectCast(font, Font)).ToString
            Case GetType(CSSFont)
                Return DirectCast(font, CSSFont).ToString
            Case GetType(String())
                Return DirectCast(font, String()).GetValue(Scan0)
            Case Else
                Return [default]
        End Select
    End Function

    ''' <summary>
    ''' <see cref="Stroke"/>
    ''' </summary>
    ''' <param name="stroke"></param>
    ''' <param name="default"></param>
    ''' <returns></returns>
    Public Function getStrokePenCSS(stroke As Object, Optional default$ = Stroke.AxisStroke) As String
        If stroke Is Nothing Then
            Return [default]
        End If

        Select Case stroke.GetType
            Case GetType(String)
                Return stroke
            Case GetType(String())
                Return DirectCast(stroke, String()).GetValue(Scan0)
            Case GetType(Stroke)
                Return DirectCast(stroke, Stroke).ToString
            Case GetType(Pen)
                Return New Stroke(DirectCast(stroke, Pen)).ToString
            Case GetType(Single), GetType(Double), GetType(Integer)
                Return New Stroke(CDbl(stroke)).ToString
            Case Else
                Return [default]
        End Select
    End Function
End Module
