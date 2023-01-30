#Region "Microsoft.VisualBasic::7bfafd8ba0bd18b9008ba33d51051e75, R-sharp\Library\R_graphic.interop\RColorPalette.vb"

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

    '   Total Lines: 168
    '    Code Lines: 134
    ' Comment Lines: 15
    '   Blank Lines: 19
    '     File Size: 6.69 KB


    ' Module RColorPalette
    ' 
    '     Function: CreateColorMaps, getColor, getColors, getColorSequence, getColorSet
    '               GetRawColor
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Module RColorPalette

    ''' <summary>
    ''' 因为html颜色不支持透明度，所以这个函数是为了解决透明度丢失的问题而编写的
    ''' </summary>
    ''' <param name="color"></param>
    ''' <param name="default$"></param>
    ''' <returns></returns>
    Public Function GetRawColor(color As Object, Optional default$ = "black") As Color
        Return graphicsPipeline.GetRawColor(color, [default])
    End Function

    Public Function getColorSet(colorSet As Object, Optional default$ = "Set1:c8") As String
        If colorSet Is Nothing Then
            Return [default]
        ElseIf TypeOf colorSet Is vector Then
            colorSet = DirectCast(colorSet, vector).data
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

    <Extension>
    Public Function CreateColorMaps(uniqClass As String(),
                                    colorArguments As Object,
                                    env As Environment,
                                    Optional [default] As String = "Clusters") As Dictionary(Of String, Color)

        If TypeOf colorArguments Is list Then
            ' key -> color mapping
            Dim colorList As list = DirectCast(colorArguments, list)
            Dim colors As New Dictionary(Of String, Color)

            For Each classLabel As String In uniqClass
                colors(classLabel) = RColorPalette.GetRawColor(colorList.getByName(classLabel))
            Next

            Return colors
        Else
            ' string vector
            ' mapping in orders
            Dim colorSet As String() = RColorPalette.getColors(colorArguments, uniqClass.Length, [default])
            Dim colors As Dictionary(Of String, Color) = uniqClass.CreateColorMaps(colorSet)

            Return colors
        End If
    End Function

    <Extension>
    Public Function CreateColorMaps(uniqClass As String(),
                                    colorSet As String(),
                                    Optional [default] As String = "black") As Dictionary(Of String, Color)

        If colorSet.All(Function(d) d.IndexOf("="c) > -1) Then
            ' a=b[] color mapping
            Dim allMaps = colorSet _
                .Select(Function(d) d.GetTagValue("=")) _
                .ToDictionary(Function(d) d.Name,
                              Function(d)
                                  Return d.Value
                              End Function)
            Dim defaultColor As Color = [default].TranslateColor

            Return uniqClass _
                .ToDictionary(Function(tag)
                                  Return tag
                              End Function,
                              Function(tag)
                                  Return If(allMaps.ContainsKey(tag), allMaps(tag).TranslateColor, defaultColor)
                              End Function)
        Else
            Return colorSet _
                .Take(uniqClass.Length) _
                .SeqIterator _
                .ToDictionary(Function(i) uniqClass(i),
                              Function(i)
                                  Return i.value.TranslateColor
                              End Function)
        End If
    End Function

    ''' <summary>
    ''' create color mapping
    ''' </summary>
    ''' <param name="colorSet"></param>
    ''' <param name="levels"></param>
    ''' <param name="default"></param>
    ''' <returns></returns>
    Public Function getColors(colorSet As Object, levels As Integer, Optional default$ = "Set1:c8") As String()
        If colorSet Is Nothing Then
            Return getColorSequence([default], levels)
        ElseIf TypeOf colorSet Is list Then
            colorSet = DirectCast(colorSet, list).slots
        End If

        If TypeOf colorSet Is vector Then
            colorSet = DirectCast(colorSet, vector).data
        End If

        Dim type As Type = colorSet.GetType

        If type Is GetType(Dictionary(Of String, Object)) Then
            ' a=b[] mapping
            Return DirectCast(colorSet, Dictionary(Of String, Object)) _
                .Select(Function(map)
                            Return $"""{map.Key}""={RColorPalette.getColor(map.Value)}"
                        End Function) _
                .ToArray

        ElseIf type.IsArray Then
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

    ''' <summary>
    ''' get a single color literal string
    ''' </summary>
    ''' <param name="color"></param>
    ''' <param name="default$"></param>
    ''' <returns></returns>
    Public Function getColor(color As Object, Optional default$ = "black") As String
        If color Is Nothing Then
            Return [default]
        ElseIf TypeOf color Is vector Then
            color = DirectCast(color, vector).data
        End If

        If color.GetType.IsArray AndAlso DirectCast(color, Array).Length = 1 Then
            color = DirectCast(color, Array).GetValue(Scan0)
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
#Disable Warning
                Return DirectCast(color, SolidBrush).Color.ToHtmlColor
#Enable Warning
            Case Else
                Return [default]
        End Select
    End Function
End Module
