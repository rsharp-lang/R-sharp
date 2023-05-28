#Region "Microsoft.VisualBasic::238e96871b7c1a058856345798d9d1ca, F:/GCModeller/src/R-sharp/Library/igraph//Styler.vb"

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

    '   Total Lines: 237
    '    Code Lines: 189
    ' Comment Lines: 21
    '   Blank Lines: 27
    '     File Size: 9.25 KB


    ' Module Styler
    ' 
    '     Function: elementColor, size, width
    ' 
    '     Sub: SetPenColor, SetPenWidth
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("styler")>
Module Styler

    ''' <summary>
    ''' set or get edge pen width
    ''' </summary>
    ''' <param name="e"></param>
    ''' <param name="val"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("width")>
    Public Function width(e As E,
                          <RRawVectorArgument>
                          <RByRefValueAssign>
                          Optional val As Object = Nothing,
                          Optional env As Environment = Nothing) As Object

        If val Is Nothing Then
            Return New list(RType.GetRSharpType(GetType(Double))) With {
                .slots = e.edges _
                    .ToDictionary(Function(d) d.ID,
                                  Function(d)
                                      Return CObj(CSng(d.data.style?.Width))
                                  End Function)
            }
        End If

        Dim valType As RType = RType.GetRSharpType(val.GetType)

        If valType.mode = TypeCodes.double OrElse valType.mode = TypeCodes.integer Then
            Dim vec As Double() = CLRVector.asNumeric(val)

            If vec.Length = 1 Then
                Dim w As Single = vec(Scan0)

                For Each link As Edge In e.edges
                    Call link.SetPenWidth(w)
                Next
            ElseIf vec.Length <> e.size Then
                Return Internal.debug.stop($"the size of the data vector is not equals to the size of the edge list!", env)
            Else
                For i As Integer = 0 To vec.Length - 1
                    e.edges(i).SetPenWidth(vec(i))
                Next
            End If
        ElseIf valType Is RType.list Then
            For Each attr In DirectCast(val, list).slots
                Call e(attr.Key).SetPenWidth(REnv.single(CLRVector.asFloat(attr.Value)))
            Next
        End If

        Return e
    End Function

    <Extension>
    Private Sub SetPenColor(e As Edge, c As Color)
        If e.data.style Is Nothing Then
            e.data.style = New Pen(New SolidBrush(c), 1)
        Else
            e.data.style.Color = c
        End If
    End Sub

    <Extension>
    Private Sub SetPenWidth(e As Edge, w As Single)
        If e.data.style Is Nothing Then
            e.data.style = New Pen(Brushes.Black, w)
        Else
            e.data.style.Width = w
        End If
    End Sub

    ''' <summary>
    ''' get/set node or edge color style
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="val"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("color")>
    Public Function elementColor(g As Object,
                                 <RRawVectorArgument>
                                 <RByRefValueAssign>
                                 Optional val As Object = Nothing,
                                 Optional env As Environment = Nothing) As Object

        If val Is Nothing Then
            If TypeOf g Is E Then
                Return New list(GetType(Color)) With {
                    .slots = DirectCast(g, E).edges _
                        .ToDictionary(Function(e) e.ID,
                                      Function(e)
                                          If Not e.data.style Is Nothing Then
                                              Return CObj(e.data.style.Color)
                                          Else
                                              Return CObj(DirectCast(Brushes.Black, SolidBrush).Color)
                                          End If
                                      End Function)
                }
            Else
                Return New list(GetType(String)) With {
                   .slots = DirectCast(g, V).vertex _
                        .ToDictionary(Function(v) v.label,
                                      Function(v)
                                          If v.data.color Is Nothing Then
                                              Return CObj(DirectCast(Brushes.Black, SolidBrush).Color)
                                          Else
                                              Return CObj(DirectCast(v.data.color, SolidBrush).Color)
                                          End If
                                      End Function)
                }
            End If
        End If

        Dim valType As RType = RType.GetRSharpType(val.GetType)

        If valType.mode = TypeCodes.string Then
            Dim vec As String() = CLRVector.asCharacter(val)

            If vec.Length = 1 Then
                Dim w As Color = vec(Scan0).TranslateColor

                If TypeOf g Is E Then
                    For Each link As Edge In DirectCast(g, E).edges
                        Call link.SetPenColor(w)
                    Next
                Else
                    Dim b As New SolidBrush(w)

                    For Each vex As Node In DirectCast(g, V).vertex
                        vex.data.color = b
                    Next
                End If
            ElseIf vec.Length <> DirectCast(g, RIndex).length Then
                Return Internal.debug.stop($"the size of the data vector is not equals to the size of the target graph element list!", env)
            Else
                If TypeOf g Is E Then
                    For i As Integer = 0 To vec.Length - 1
                        DirectCast(g, E).edges(i).SetPenColor(vec(i).TranslateColor)
                    Next
                Else
                    For i As Integer = 0 To vec.Length - 1
                        DirectCast(g, V).vertex(i).data.color = vec(i).GetBrush
                    Next
                End If
            End If
        ElseIf valType Is RType.list Then
            If TypeOf g Is E Then
                For Each attr In DirectCast(val, list).slots
                    Call DirectCast(g, E)(attr.Key).SetPenColor(CStr(REnv.single(CLRVector.asCharacter(attr.Value))).TranslateColor)
                Next
            Else
                For Each attr In DirectCast(val, list).slots
                    Dim vex As Node = DirectCast(g, V)(attr.Key)

                    If Not vex Is Nothing Then
                        vex.data.color = CStr(REnv.single(CLRVector.asCharacter(attr.Value))).GetBrush
                    End If
                Next
            End If
        End If

        Return g
    End Function

    ''' <summary>
    ''' set or get node size data
    ''' </summary>
    ''' <param name="v"></param>
    ''' <returns></returns>
    <ExportAPI("size")>
    <RApiReturn(GetType(V))>
    Public Function size(v As V,
                         <RRawVectorArgument>
                         <RByRefValueAssign>
                         Optional val As Object = Nothing,
                         Optional env As Environment = Nothing) As Object

        If val Is Nothing Then
            Return New list(RType.GetRSharpType(GetType(Double))) With {
               .slots = v.vertex _
                   .ToDictionary(Function(d) d.label,
                                 Function(d)
                                     Return CObj(d.data.size.ElementAtOrDefault(0))
                                 End Function)
            }
        End If

        Dim valType As RType = RType.GetRSharpType(val.GetType)

        If valType.mode = TypeCodes.double OrElse valType.mode = TypeCodes.integer Then
            Dim vec As Double() = CLRVector.asNumeric(val)

            If vec.Length = 1 Then
                ' set unify size
                Dim sz As Double = vec(Scan0)

                For Each vex As Node In v.vertex
                    vex.data.size = {sz}
                Next
            Else
                ' set size one by one
                If vec.Length <> v.size Then
                    Return Internal.debug.stop($"the size of the data vector is not equals to the size of the vertex list!", env)
                Else
                    For i As Integer = 0 To vec.Length - 1
                        v.vertex(i).data.size = {vec(i)}
                    Next
                End If
            End If
        ElseIf valType Is RType.list Then
            For Each attr In DirectCast(val, list).slots
                Dim vex As Node = v(attr.Key)

                If Not vex Is Nothing Then
                    vex.data.size = CLRVector.asNumeric(attr.Value)
                End If
            Next
        End If

        Return v
    End Function
End Module
