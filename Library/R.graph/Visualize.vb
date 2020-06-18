#Region "Microsoft.VisualBasic::e24bd1d10e0e70858d14864945b9b484, Library\R.graph\Visualize.vb"

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
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports stdNum = System.Math

''' <summary>
''' Rendering png or svg image from a given network graph model.
''' </summary>
<Package("igraph.render")>
Module Visualize

    Private Function getNodeSizeHandler(nodeSize As Object, minNodeSize!, env As Environment, ByRef err As Message) As Func(Of Node, Single)
        Select Case nodeSize.GetType
            Case GetType(list)
                Dim list As list = nodeSize

                Return New Func(Of Node, Single)(
                    Function(node As Node) As Single
                        If list.slots.ContainsKey(node.label) Then
                            Return stdNum.Max(CSng(Val(list.slots(node.label))), minNodeSize)
                        Else
                            Return minNodeSize
                        End If
                    End Function
                )
            Case GetType(Func(Of Node, Single))
                Return DirectCast(nodeSize, Func(Of Node, Single))
            Case GetType(Func(Of Node, Double))
                With DirectCast(nodeSize, Func(Of Node, Double))
                    Return New Func(Of Node, Single)(Function(n) .Invoke(n))
                End With
            Case GetType(DeclareLambdaFunction)
                With DirectCast(nodeSize, DeclareLambdaFunction)
                    Dim compute = .CreateLambda(Of String, Single)(env)

                    Return New Func(Of Node, Single)(
                        Function(n)
                            Return compute(n.label)
                        End Function
                    )
                End With
            Case GetType(String)
                Dim propName As String = nodeSize

                Return Function(n As Node)
                           Return stdNum.Max(Val(n.data(propName)), minNodeSize)
                       End Function
            Case Else
                err = Internal.debug.stop(New NotImplementedException(nodeSize.GetType.FullName), env)
        End Select

        Return Nothing
    End Function

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
                               Optional nodeLabel As Object = Nothing,
                               Optional labelerIterations% = 100,
                               Optional texture As Object = Nothing,
                               Optional widget As Object = Nothing,
                               Optional showLabelerProgress As Boolean = False,
                               Optional showUntexture As Boolean = True,
                               Optional defaultEdgeColor$ = "gray",
                               Optional defaultLabelColor$ = "black",
                               Optional drawEdgeDirection As Boolean = False,
                               Optional driver As Drivers = Drivers.GDI,
                               Optional env As Environment = Nothing) As Object

        Dim nodeWidget As Action(Of IGraphics, PointF, Double, Node) = Nothing
        Dim nodeRadius As [Variant](Of Func(Of Node, Single), Single) = Nothing
        Dim err As Message = Nothing

        If nodeSize Is Nothing Then
            nodeRadius = minNodeSize
        Else
            nodeRadius = getNodeSizeHandler(nodeSize, minNodeSize, env, err)

            If Not err Is Nothing Then
                Return err
            End If
        End If

        If Not widget Is Nothing Then
            Select Case widget.GetType
                Case GetType(DeclareNewFunction)
                    Dim func As DeclareNewFunction = widget

                    nodeWidget = Sub(canvas, center, radius, node)
                                     Call func.Invoke(env, InvokeParameter.CreateLiterals(canvas, center, radius, node.label))
                                 End Sub
            End Select
        End If

        Dim getTexture As Func(Of String, String) = Nothing

        If Not texture Is Nothing Then
            Select Case texture.GetType
                Case GetType(DeclareNewFunction)
                    Dim func As DeclareNewFunction = texture

                    getTexture = Function(label)
                                     Return getFirst(func.Invoke(env, InvokeParameter.CreateLiterals(label)))
                                 End Function
                Case Else
            End Select
        End If

        Dim drawNodeShape As DrawNodeShape = Nothing

        If Not getTexture Is Nothing Then
            drawNodeShape = Function(id, gr, colorBrush, r, center)
                                Dim value = getTexture(id)

                                If value Is Nothing Then
                                    If showUntexture Then
                                        Call gr.DrawCircle(center, r, colorBrush)
                                    End If
                                Else
                                    Dim textureBrush As Brush = value.GetBrush

                                    If TypeOf textureBrush Is SolidBrush Then
                                        Call gr.DrawCircle(center, r, textureBrush)
                                    Else
                                        Dim res = value.LoadImage.ColorReplace(Color.White, Color.Transparent)
                                        Dim size = res.Size
                                        Dim maxR = r * 2.5
                                        Dim scale = stdNum.Max(size.Width, size.Height) / maxR
                                        Dim w! = size.Width / scale
                                        Dim h! = size.Height / scale

                                        Call gr.DrawImage(value.LoadImage, center.X - w / 2, center.Y - h / 2, w, h)
                                    End If
                                End If
                            End Function
        End If

        Dim getNodeLabel As Func(Of Node, String) = Nothing

        If Not nodeLabel Is Nothing Then
            Select Case nodeLabel.GetType
                Case GetType(DeclareLambdaFunction)
                    Dim compute = DirectCast(nodeLabel, DeclareLambdaFunction).CreateLambda(Of String, String)(env)

                    getNodeLabel = Function(node) compute(node.label)
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
            drawEdgeBends:=True,
            throwEx:=env.globalEnvironment.Rscript.debug,
            drawNodeShape:=drawNodeShape，
            edgeDashTypes:=DashStyle.Solid,
            defaultEdgeColor:=defaultEdgeColor,
            defaultLabelColor:=defaultLabelColor,
            getNodeLabel:=getNodeLabel,
            drawEdgeDirection:=drawEdgeDirection,
            nodeWidget:=nodeWidget
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
