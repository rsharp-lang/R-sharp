#Region "Microsoft.VisualBasic::ddf8fbe6f218446c5ce841a57efddfd7, D:/GCModeller/src/R-sharp/Library/graphics//Plot2D/geometry2D.vb"

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

    '   Total Lines: 124
    '    Code Lines: 92
    ' Comment Lines: 16
    '   Blank Lines: 16
    '     File Size: 4.87 KB


    ' Module geometry2D
    ' 
    '     Function: density2D, Kdtest
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
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.DataMining.DensityQuery
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions

<Package("geometry2D")>
Module geometry2D

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
            Return Internal.debug.stop($"the size({x.Length}) of vector x must be equals to the size({y.Length}) of vector y!", env)
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
