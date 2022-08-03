#Region "Microsoft.VisualBasic::f1b75fac423f0a004d355e6a7db16bfd, R-sharp\Library\igraph\Utils\Attributes\EdgeAttributes.vb"

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

    '   Total Lines: 73
    '    Code Lines: 61
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 2.69 KB


    ' Module EdgeAttributes
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: getDash, SetEdgeAttributesInList
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting
Imports REnv = SMRUCC.Rsharp.Runtime

Module EdgeAttributes

    ReadOnly dashMaps As Dictionary(Of String, DashStyle)

    Sub New()
        dashMaps = Enums(Of DashStyle).ToDictionary(Function(a) a.ToString.ToLower)
    End Sub

    Private Function getDash(value As Object) As DashStyle
        Return dashMaps.TryGetValue(LCase(any.ToString(value)), default:=DashStyle.Solid)
    End Function

    <Extension>
    Public Function SetEdgeAttributesInList(elements As Edge(), name$, valList As list) As Object
        Dim value As Object
        Dim element As Edge
        Dim hash As Dictionary(Of String, Edge) = elements.ToDictionary(Function(n) n.ID)

        For Each eName As String In valList.slots.Keys
            value = REnv.single(valList.slots(eName))
            element = hash.TryGetValue(eName)

            If element Is Nothing Then
                Continue For
            ElseIf element.data.style Is Nothing Then
                element.data.style = New Pen(Brushes.Black)
            End If

            If name = "color" Then
                Dim colorBrush As SolidBrush

                If TypeOf value Is Brush Then
                    colorBrush = value
                ElseIf TypeOf value Is Color Then
                    colorBrush = New SolidBrush(DirectCast(value, Color))
                Else
                    colorBrush = any.ToString(value).GetBrush
                End If

                element.data.style = New Pen(colorBrush, element.data.style.Width) With {
                    .DashStyle = element.data.style.DashStyle
                }
            ElseIf name = "width" Then
                element.data.style = New Pen(element.data.style.Color, Val(value)) With {
                    .DashStyle = element.data.style.DashStyle
                }
            ElseIf name = "dash" Then
                Dim dash As DashStyle = getDash(value)

                element.data.style = New Pen(
                    color:=element.data.style.Color,
                    width:=element.data.style.Width
                ) With {
                    .DashStyle = dash
                }
            Else
                element.data(name) = any.ToString(value)
            End If
        Next

        Return Nothing
    End Function
End Module
