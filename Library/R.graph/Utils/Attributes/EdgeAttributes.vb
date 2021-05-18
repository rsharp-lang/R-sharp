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
        Return dashMaps.TryGetValue(LCase(any.ToString(value)), DashStyle.Solid)
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
