#Region "Microsoft.VisualBasic::f926887df8821e24631f43507442caae, Library\R.graph\Visualize.vb"

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
    '     Function: colorByTypeGroup, getNodeSizeHandler, renderPlot, setEdgeColors, setNodeColors
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
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
    ''' <param name="canvasSize"></param>
    ''' <param name="texture">
    ''' get texture brush or color brush descriptor string.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("render.Plot")>
    <RApiReturn(GetType(GraphicsData))>
    Public Function renderPlot(g As NetworkGraph,
                               <RRawVectorArgument>
                               Optional canvasSize As Object = "1024,768",
                               <RRawVectorArgument>
                               Optional padding As Object = g.DefaultPadding,
                               Optional defaultColor$ = "skyblue",
                               Optional minNodeSize! = 10,
                               Optional minLinkWidth! = 2,
                               Optional nodeSize As Object = Nothing,
                               Optional nodeLabel As Object = Nothing,
                               Optional nodeStroke As Object = Stroke.ScatterLineStroke,
                               Optional nodeVisual As Object = Nothing,
                               Optional hullPolygonGroups As Object = Nothing,
                               Optional labelFontSize As Object = 20,
                               Optional labelerIterations% = 100,
                               Optional labelColor As Object = Nothing,
                               Optional labelWordWrapWidth As Integer = -1,
                               Optional texture As Object = Nothing,
                               Optional widget As Object = Nothing,
                               Optional showLabelerProgress As Boolean = False,
                               Optional showUntexture As Boolean = True,
                               Optional defaultEdgeColor$ = "lightgray",
                               Optional defaultLabelColor$ = "black",
                               Optional drawEdgeDirection As Boolean = False,
                               Optional driver As Drivers = Drivers.GDI,
                               Optional env As Environment = Nothing) As Object

        Dim nodeWidget As Func(Of IGraphics, PointF, Double, Node, RectangleF) = Nothing
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

                    nodeWidget = Function(canvas, center, radius, node)
                                     Dim layout = func.Invoke(env, InvokeParameter.CreateLiterals(canvas, center, radius, node.label))

                                     If Not layout Is Nothing Then
                                         layout = REnv.asVector(Of Object)(layout).GetValue(Scan0)
                                     End If

                                     Return layout
                                 End Function
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
        ElseIf Not nodeVisual Is Nothing Then
            If TypeOf nodeVisual Is DeclareNewFunction Then
                Dim drawNode As DeclareNewFunction = DirectCast(nodeVisual, DeclareNewFunction)

                drawNodeShape = Function(id, gr, color, r, center)
                                    Return drawNode.Invoke(env, InvokeParameter.CreateLiterals(gr, id, color, center, r))
                                End Function
            Else
                Return Internal.debug.stop(Message.InCompatibleType(GetType(DeclareNewFunction), nodeVisual.GetType, env,, NameOf(nodeVisual)), env)
            End If
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

        Dim getLabelColor As Func(Of Node, Color) = Nothing

        If Not labelColor Is Nothing Then
            If TypeOf labelColor Is DeclareNewFunction Then
                getLabelColor = Function(node)
                                    Dim vals = DirectCast(labelColor, DeclareNewFunction).Invoke(env, InvokeParameter.CreateLiterals(node.label))
                                    vals = REnv.getFirst(vals)
                                    Return InteropArgumentHelper.getColor(vals).TranslateColor
                                End Function
            Else
                Return Internal.debug.stop(Message.InCompatibleType(GetType(DeclareNewFunction), labelColor.GetType, env,, NameOf(labelColor)), env)
            End If
        End If

        If Not hullPolygonGroups Is Nothing Then
            hullPolygonGroups = GetNamedValueTuple(Of String)(hullPolygonGroups, env)

            If Not hullPolygonGroups Is Nothing Then
                With DirectCast(hullPolygonGroups, [Variant](Of NamedValue(Of String), Message))
                    If .GetUnderlyingType Is GetType(Message) Then
                        Return .VB
                    Else
                        hullPolygonGroups = .TryCast(Of NamedValue(Of String))
                    End If
                End With
            End If
        End If

        Return g.DrawImage(
            canvasSize:=InteropArgumentHelper.getSize(canvasSize),
            padding:=InteropArgumentHelper.getPadding(padding),
            labelerIterations:=labelerIterations,
            defaultColor:=InteropArgumentHelper.getColor(defaultColor),
            nodeRadius:=nodeRadius,
            labelTextStroke:=Nothing,
            labelWordWrapWidth:=labelWordWrapWidth,
            labelColorAsNodeColor:=False,
            getLabelColor:=getLabelColor,
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
            nodeWidget:=nodeWidget,
            fontSize:=CSng(labelFontSize),
            nodeStroke:=InteropArgumentHelper.getStrokePenCSS(nodeStroke, Nothing),
            hullPolygonGroups:=hullPolygonGroups,
            convexHullCurveDegree:=1
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

    <ExportAPI("edge.color")>
    Public Function setEdgeColors(g As NetworkGraph,
                                  <RRawVectorArgument, RByRefValueAssign>
                                  Optional colors As Object = Nothing,
                                  Optional env As Environment = Nothing) As Object
        If colors Is Nothing Then
            Return g.graphEdges _
                .Select(Function(a)
                            If a.data.color Is Nothing Then
                                Return Color.Black
                            Else
                                Return a.data.color.Color
                            End If
                        End Function) _
                .ToArray
        Else
            Dim values = REnv.asVector(Of Object)(colors)
            Dim unify As SolidBrush

            If values.Length = 1 Then
                Dim first = values.GetValue(Scan0)

                Select Case first.GetType
                    Case GetType(String)
                        unify = DirectCast(first, String).TranslateColor.DoCall(Function(c) New SolidBrush(c))
                    Case GetType(Color)
                        unify = New SolidBrush(DirectCast(first, Color))
                    Case GetType(SolidBrush)
                        unify = first
                    Case Else
                        Return Internal.debug.stop(Message.InCompatibleType(GetType(Color), first.GetType, env,, NameOf(colors)), env)
                End Select

                For Each edge As Edge In g.graphEdges
                    edge.data.color = unify
                Next
            ElseIf values.Length <> g.graphEdges.Count Then
                Return Internal.debug.stop("the color length is not equals to the edge size!", env)
            Else
                For Each edge As SeqValue(Of Edge) In g.graphEdges.SeqIterator
                    Dim first = values.GetValue(CInt(edge))

                    Select Case first.GetType
                        Case GetType(String)
                            unify = DirectCast(first, String).TranslateColor.DoCall(Function(c) New SolidBrush(c))
                        Case GetType(Color)
                            unify = New SolidBrush(DirectCast(first, Color))
                        Case GetType(SolidBrush)
                            unify = first
                        Case Else
                            Return Internal.debug.stop(Message.InCompatibleType(GetType(Color), first.GetType, env,, NameOf(colors)), env)
                    End Select

                    edge.value.data.color = unify
                Next
            End If

            Return g
        End If
    End Function

    <ExportAPI("node.colors")>
    <RApiReturn(GetType(NetworkGraph), GetType(list))>
    Public Function setNodeColors(g As NetworkGraph, <RByRefValueAssign> Optional colors As list = Nothing) As Object
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
