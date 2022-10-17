#Region "Microsoft.VisualBasic::afc444f4c1562c1d4c3cd1ea7370acac, R-sharp\Library\igraph\Utils\Attributes\NodeAttributes.vb"

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

    '   Total Lines: 99
    '    Code Lines: 87
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 3.82 KB


    ' Module NodeAttributes
    ' 
    '     Function: GetNodeAttributes, SetNodeAttributeInVector, SetNodeAttributesInList
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports node = Microsoft.VisualBasic.Data.visualize.Network.Graph.Node
Imports REnv = SMRUCC.Rsharp.Runtime

Module NodeAttributes

    <Extension>
    Public Function GetNodeAttributes(elements As node(), name$) As Object
        Return elements _
            .Select(Function(a)
                        If name = "color" Then
                            If a.data.color Is Nothing OrElse Not TypeOf a.data.color Is SolidBrush Then
                                Return "black"
                            Else
                                Return DirectCast(a.data.color, SolidBrush).Color.ToHtmlColor
                            End If
                        Else
                            Return If(a.data(name), "")
                        End If
                    End Function) _
            .ToArray
    End Function

    <Extension>
    Public Function SetNodeAttributeInVector(elements As node(), name$, values As Object) As Object
        Dim valArray As New GetVectorElement(REnv.asVector(Of Object)(values))
        Dim value As Object

        If name = "color" Then
            For i As Integer = 0 To elements.Length - 1
                value = valArray(i)

                If TypeOf value Is Brush Then
                    elements(i).data.color = value
                ElseIf TypeOf value Is Color Then
                    elements(i).data.color = New SolidBrush(DirectCast(value, Color))
                Else
                    elements(i).data.color = any.ToString(value).GetBrush
                End If
            Next
        ElseIf name = "label" Then
            For i As Integer = 0 To elements.Length - 1
                elements(i).data.label = any.ToString(valArray(i))
            Next
        ElseIf name = "size" Then
            For i As Integer = 0 To elements.Length - 1
                elements(i).data.size = {CDbl(valArray(i))}
            Next
        Else
            For i As Integer = 0 To elements.Length - 1
                elements(i).data(name) = any.ToString(valArray(i))
            Next
        End If

        Return Nothing
    End Function

    <Extension>
    Public Function SetNodeAttributesInList(elements As node(), name$, valList As list) As Object
        Dim value As Object
        Dim element As node
        Dim hash As Dictionary(Of String, node) = elements.ToDictionary(Function(e) e.label)

        For Each vName As String In valList.slots.Keys
            value = REnv.single(valList.slots(vName))
            element = hash.TryGetValue(vName)

            If element Is Nothing Then
                Continue For
            End If

            If name = "color" Then
                If TypeOf value Is Brush Then
                    element.data.color = value
                ElseIf TypeOf value Is Color Then
                    element.data.color = New SolidBrush(DirectCast(value, Color))
                Else
                    element.data.color = any.ToString(value).GetBrush
                End If
            ElseIf name = "layout" Then
                Dim points As Double() = REnv.asVector(Of Double)(value)

                element.data.initialPostion = New FDGVector2(points(0), points(1))
            Else
                element.data(name) = any.ToString(value)
            End If
        Next

        Return Nothing
    End Function
End Module
