#Region "Microsoft.VisualBasic::d22c0f3ce37234a753055522771d96e9, Library\R.graph\Visualize.vb"

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

    ' Module Visualize
    ' 
    '     Function: colorByTypeGroup, renderPlot, setNodeColors
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports stdNum = System.Math

''' <summary>
''' Rendering png or svg image from a given network graph model.
''' </summary>
<Package("igraph.render")>
Module Visualize

    ''' <summary>
    ''' Rendering png or svg image from a given network graph model.
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="canvasSize$"></param>
    ''' <returns></returns>
    <ExportAPI("render.Plot")>
    <RApiReturn(GetType(GraphicsData))>
    Public Function renderPlot(g As NetworkGraph,
                               <RRawVectorArgument>
                               Optional canvasSize As Object = "1024,768",
                               Optional padding$ = g.DefaultPadding,
                               Optional defaultColor$ = "skyblue",
                               Optional minNodeSize! = 10,
                               Optional minLinkWidth! = 2,
                               Optional nodeSize As Object = Nothing,
                               Optional labelerIterations% = 100,
                               Optional showLabelerProgress As Boolean = False,
                               Optional driver As Drivers = Drivers.GDI,
                               Optional env As Environment = Nothing) As Object

        Dim nodeRadius As [Variant](Of Func(Of Node, Single), Single) = Nothing

        If nodeSize Is Nothing Then
            nodeRadius = minNodeSize
        Else
            Select Case nodeSize.GetType
                Case GetType(list)
                    Dim list As list = nodeSize

                    nodeRadius = New Func(Of Node, Single)(
                        Function(node As Node) As Single
                            If list.slots.ContainsKey(node.label) Then
                                Return stdNum.Max(CSng(Val(list.slots(node.label))), minNodeSize)
                            Else
                                Return minNodeSize
                            End If
                        End Function
                    )
                Case GetType(Func(Of Node, Single))
                    nodeRadius = DirectCast(nodeSize, Func(Of Node, Single))
                Case GetType(Func(Of Node, Double))
                    With DirectCast(nodeSize, Func(Of Node, Double))
                        nodeRadius = New Func(Of Node, Single)(Function(n) .Invoke(n))
                    End With
                Case GetType(DeclareLambdaFunction)
                    With DirectCast(nodeSize, DeclareLambdaFunction)
                        Dim compute = .CreateLambda(Of String, Single)(env)

                        nodeRadius = New Func(Of Node, Single)(
                            Function(n)
                                Return compute(n.label)
                            End Function
                        )
                    End With
                Case Else
                    Return Internal.debug.stop(New NotImplementedException(nodeSize.GetType.FullName), env)
            End Select
        End If

        Return g.DrawImage(
            canvasSize:=InteropArgumentHelper.getSize(canvasSize),
            padding:=InteropArgumentHelper.getPadding(padding),
            labelerIterations:=labelerIterations,
            defaultColor:=InteropArgumentHelper.getColor(defaultColor),
            nodeRadius:=nodeRadius,
            labelTextStroke:=Nothing,
            driver:=driver,
            minLinkWidth:=minLinkWidth,
            showLabelerProgress:=showLabelerProgress,
            drawEdgeBends:=True
        )
    End Function

    <ExportAPI("color.group")>
    Public Function colorByTypeGroup(g As NetworkGraph, type$, color$) As NetworkGraph
        Dim colorBrush As Brush = color.GetBrush

        g.vertex _
            .Where(Function(n)
                       Return n.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = type
                   End Function) _
            .DoEach(Sub(n)
                        n.data.color = colorBrush
                    End Sub)

        Return g
    End Function

    <ExportAPI("node.colors")>
    <RApiReturn(GetType(NetworkGraph), GetType(list))>
    Public Function setNodeColors(g As NetworkGraph, Optional colors As list = Nothing) As Object
        If colors Is Nothing Then
            Return New list With {
                .slots = g.vertex _
                    .ToDictionary(Function(n) n.label,
                                  Function(n)
                                      Return CObj(DirectCast(n.data.color, SolidBrush).Color)
                                  End Function)
            }
        Else
            For Each node As Node In g.vertex
                If colors.slots.ContainsKey(node.label) Then
                    node.data.color = InteropArgumentHelper.getColor(colors.slots(node.label)).GetBrush
                End If
            Next

            Return g
        End If
    End Function

End Module
