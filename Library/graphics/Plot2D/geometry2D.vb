#Region "Microsoft.VisualBasic::6a351a07c9a0f8f740edbea0f7dadb59, Library\graphics\Plot2D\geometry2D.vb"

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

    '   Total Lines: 256
    '    Code Lines: 193 (75.39%)
    ' Comment Lines: 34 (13.28%)
    '    - Xml Docs: 97.06%
    ' 
    '   Blank Lines: 29 (11.33%)
    '     File Size: 10.92 KB


    ' Module geometry2D
    ' 
    '     Function: ConcaveHull, density2D, fillPolygonGroups, fillPolygons, Kdtest
    '               transform
    ' 
    '     Sub: Main
    ' 
    ' Class PointAccess
    ' 
    '     Function: activate, getByDimension, GetDimensions, metric, nodeIs
    ' 
    '     Sub: setByDimensin
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.DataMining.DensityQuery
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConcaveHull
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus.ODESolver
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

<Package("geometry2D")>
<RTypeExport("polygon_group", GetType(PolygonGroup))>
<RTypeExport("geo_transform", GetType(Transform))>
Module geometry2D

    Public Sub Main()
        Call RInternal.generic.add("plot", GetType(Polygon2D), Function(polygon, args, env) fillPolygons({DirectCast(polygon, Polygon2D)}, args, env))
        Call RInternal.generic.add("plot", GetType(Polygon2D()), AddressOf fillPolygons)
        Call RInternal.generic.add("plot", GetType(PolygonGroup()), AddressOf fillPolygonGroups)
    End Sub

    Private Function fillPolygonGroups(polygons As PolygonGroup(), args As list, env As Environment) As Object
        Dim colors = RColorPalette.getColorSet(args.getBySynonyms("colors", "colorset", "colorSet"), "paper")
        Dim size = InteropArgumentHelper.getSize(args.getBySynonyms("size"), env)
        Dim scatter As Boolean = CLRVector.asScalarLogical(args.getBySynonyms("scatter"))
        Dim theme As New Theme With {.colorSet = colors}
        Dim app As New FillPolygons(polygons, scatter, theme)

        Return app.Plot(size)
    End Function

    Private Function fillPolygons(polygons As Polygon2D(), args As list, env As Environment) As Object
        Dim colors = RColorPalette.getColorSet(args.getBySynonyms("colors", "colorset", "colorSet"), "paper")
        Dim size = InteropArgumentHelper.getSize(args.getBySynonyms("size"), env)
        Dim scatter As Boolean = CLRVector.asScalarLogical(args.getBySynonyms("scatter"))
        Dim padding As String = InteropArgumentHelper.getPadding(args!padding, "padding: 10% 10% 15% 20%;")
        Dim theme As New Theme With {.colorSet = colors, .padding = padding}
        Dim driver As Drivers = env.getDriver

        If polygons.IsNullOrEmpty Then
            Return g.GraphicsPlots(
                size.SizeParser, "padding:0px", "white",
                plotAPI:=Sub(ByRef gfx, rect)

                         End Sub,
                driver:=driver)
        Else
            Dim app As New FillPolygons(polygons, scatter, theme)
            Return app.Plot(size, driver:=driver)
        End If
    End Function

    ''' <summary>
    ''' alpha shapes algorithm method
    ''' </summary>
    ''' <param name="pts">
    ''' + for dataframe object, data fields x and y should be exists
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' A set of point data which could be used for build a polygon object
    ''' </returns>
    <ExportAPI("concaveHull")>
    <RApiReturn(GetType(Polygon2D))>
    Public Function ConcaveHull(<RRawVectorArgument>
                                pts As Object,
                                Optional as_polygon As Boolean = False,
                                Optional r As Double = -1,
                                Optional env As Environment = Nothing) As Object

        Dim x As Double(), y As Double()

        If pts Is Nothing Then
            Return Nothing
        End If
        If TypeOf pts Is dataframe Then
            With DirectCast(pts, dataframe)
                x = CLRVector.asNumeric(.getVector("x"))
                y = CLRVector.asNumeric(.getVector("y"))
            End With
        ElseIf TypeOf pts Is Polygon2D Then
            With DirectCast(pts, Polygon2D)
                x = .xpoints
                y = .ypoints
            End With
        Else
            Return Message.InCompatibleType(GetType(dataframe), pts.GetType, env)
        End If

        Dim points As PointF() = x _
            .Select(Function(xi, i)
                        Return New PointF(xi, y(i))
                    End Function) _
            .ToArray
        Dim polygon As PointF() = points.ConcaveHull(r)

        If as_polygon Then
            Return New Polygon2D(polygon)
        Else
            Return New dataframe With {
                .columns = New Dictionary(Of String, Array) From {
                    {"x", points.X.ToArray},
                    {"y", points.Y.ToArray}
                }
            }
        End If
    End Function

    ''' <summary>
    ''' Create a new 2d polygon shape object by given points data.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("polygon2D")>
    <RApiReturn(GetType(Polygon2D))>
    Public Function createPolygon2D(<RRawVectorArgument> x As Object, <RRawVectorArgument> y As Object, Optional env As Environment = Nothing) As Object
        Dim xvec As Double() = CLRVector.asNumeric(x)
        Dim yvec As Double() = CLRVector.asNumeric(y)

        If xvec.TryCount <> yvec.TryCount Then
            Return RInternal.debug.stop($"the size of the x({xvec.TryCount}) vector is not equals to the size of the y({yvec.TryCount}) vector!", env)
        ElseIf xvec.TryCount = 0 OrElse yvec.TryCount = 0 Then
            Call "empty polygon shape data.".warning
            Return Nothing
        End If

        Return New Polygon2D(xvec, yvec)
    End Function

    ''' <summary>
    ''' Evaluate the density value of a set of 2d points.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="k"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' a density value vector. the elements in the resulted density 
    ''' value vector is keeps the same order as the input [x,y] 
    ''' vector.
    ''' </returns>
    <ExportAPI("density2D")>
    <RApiReturn(GetType(Double))>
    Public Function density2D(x As Integer(), y As Integer(),
                              Optional k As Integer = 6,
                              Optional env As Environment = Nothing) As Object

        If x.IsNullOrEmpty OrElse y.IsNullOrEmpty Then
            Return Nothing
        ElseIf x.Length <> y.Length Then
            Return RInternal.debug.stop($"the size({x.Length}) of vector x must be equals to the size({y.Length}) of vector y!", env)
        End If

        Dim density As Dictionary(Of String, Double) = x _
            .AsParallel _
            .Select(Function(xi, i) (xi, y(i), $"{xi},{y(i)}")) _
            .Density(Function(d) d.Item3, Function(d) d.Item1, Function(d) d.Item2, New Size(k, k)) _
            .ToDictionary(Function(d) d.Name,
                            Function(d)
                                Return d.Value
                            End Function)
        Dim vDensity As Double() = x _
            .Select(Function(xi, i) density($"{xi},{y(i)}")) _
            .ToArray

        Return vDensity
    End Function

    ''' <summary>
    ''' just used for do kd-tree unit test
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("Kdtest")>
    Public Function Kdtest(Optional n As Integer = 1000,
                           Optional knn As Integer = 60,
                           <RRawVectorArgument>
                           Optional size As Object = "5200,4500",
                           Optional env As Environment = Nothing) As Object

        Dim sizeVal = InteropArgumentHelper.getSize(size, env, [default]:="3300,2100").SizeParser
        Dim points2 As Point2D() = n _
            .SeqRandom _
            .Select(Function(i)
                        Return New Point2D(randf.NextInteger(sizeVal.Width), randf.NextInteger(sizeVal.Height))
                    End Function) _
            .ToArray
        Dim tree2 As New KdTree(Of Point2D)(points2, New PointAccess)
        Dim query = {
            New NamedValue(Of PointF)("1", points2.Random, "#009EFB"),
            New NamedValue(Of PointF)("1", points2.Random, "#55CE63"),
            New NamedValue(Of PointF)("1", points2.Random, "#F62D51"),
            New NamedValue(Of PointF)("1", points2.Random, "#FFBC37"),
            New NamedValue(Of PointF)("1", points2.Random, "#7460EE"),
            New NamedValue(Of PointF)("1", points2.Random, "#52E5DD"),
            New NamedValue(Of PointF)("1", points2.Random, "#984ea3"),
            New NamedValue(Of PointF)("1", points2.Random, "#ffff00")
        }

        Return DrawKDTree.Plot(tree2, query, k:=knn, size:=$"{sizeVal.Width},{sizeVal.Height}", padding:="padding: 50px 50px 50px 50px;")
    End Function

    ''' <summary>
    ''' Create a 2D geometric transformation argument object.
    ''' </summary>
    ''' <param name="theta"></param>
    ''' <param name="translate"></param>
    ''' <param name="scale"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("transform")>
    <RApiReturn(GetType(Transform))>
    Public Function transform(Optional theta As Double = 0,
                              <RRawVectorArgument(TypeCodes.double)>
                              Optional translate As Object = Nothing,
                              <RRawVectorArgument(TypeCodes.double)>
                              Optional scale As Object = Nothing,
                              Optional env As Environment = Nothing) As Object

        If theta = 0.0 AndAlso translate Is Nothing AndAlso scale Is Nothing Then
            Call env.AddMessage("empty 2D geometric transformation information!", MSG_TYPES.WRN)

            Return New Transform() With {
                .ty = 0,
                .tx = 0,
                .theta = 0,
                .scaley = 1,
                .scalex = 1
            }
        End If

        Dim translate_vec As Double() = CLRVector.asNumeric(translate)
        Dim scale_vec As Double() = CLRVector.asNumeric(scale)

        Return New Transform With {
            .theta = theta,
            .scalex = If(scale_vec.IsNullOrEmpty, 1.0, scale_vec(0)),
            .scaley = If(scale_vec.IsNullOrEmpty OrElse scale_vec.Length = 1, .scalex, scale_vec(1)),
            .tx = If(translate_vec.IsNullOrEmpty, 0.0, translate_vec(0)),
            .ty = If(translate_vec.IsNullOrEmpty OrElse translate_vec.Length = 1, .tx, translate_vec(1))
        }
    End Function

End Module

Public Class PointAccess : Inherits KdNodeAccessor(Of Point2D)

    Public Overrides Sub setByDimensin(x As Point2D, dimName As String, value As Double)
        If dimName.TextEquals("x") Then
            x.X = value
        Else
            x.Y = value
        End If
    End Sub

    Public Overrides Function GetDimensions() As String()
        Return {"x", "y"}
    End Function

    Public Overrides Function metric(a As Point2D, b As Point2D) As Double
        Return a.DistanceTo(b)
    End Function

    Public Overrides Function getByDimension(x As Point2D, dimName As String) As Double
        If dimName.TextEquals("x") Then
            Return x.X
        Else
            Return x.Y
        End If
    End Function

    Public Overrides Function nodeIs(a As Point2D, b As Point2D) As Boolean
        Return a Is b
    End Function

    Public Overrides Function activate() As Point2D
        Return New Point2D
    End Function
End Class
