Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
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

            If Not element Is Nothing Then
                If name = "color" Then
                    If TypeOf value Is Brush Then
                        element.data.color = value
                    ElseIf TypeOf value Is Color Then
                        element.data.color = New SolidBrush(DirectCast(value, Color))
                    Else
                        element.data.color = any.ToString(value).GetBrush
                    End If
                Else
                    element.data(name) = any.ToString(value)
                End If
            End If
        Next

        Return Nothing
    End Function
End Module
