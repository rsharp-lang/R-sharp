#Region "Microsoft.VisualBasic::ed1d9ca94f53735d3cfa3086a956cf3d, R-sharp\Library\graphics\Render3D\plot3D.vb"

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

    '   Total Lines: 83
    '    Code Lines: 73
    ' Comment Lines: 0
    '   Blank Lines: 10
    '     File Size: 3.15 KB


    ' Module plot3D
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: plotScatter3D, serial3D
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Data.ChartPlots.Plot3D
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports REnv = SMRUCC.Rsharp.Runtime
Imports scatter3D = Microsoft.VisualBasic.Data.ChartPlots.Plot3D.Scatter

<Package("chart3D", Category:=APICategories.SoftwareTools)>
Module plot3D

    Sub New()
        REnv.Internal.generic.add("plot", GetType(Serial3D), AddressOf plotScatter3D)
        REnv.Internal.generic.add("plot", GetType(Serial3D()), AddressOf plotScatter3D)
    End Sub

    Private Function plotScatter3D(input As Object, args As list, env As Environment) As GraphicsData
        Dim data As Serial3D()
        Dim camera As Camera = args.getValue(Of Camera)("camera", env)
        Dim bg$ = RColorPalette.getColor(args!bg, "white")
        Dim title As String = Scripting.ToString(getFirst(args!title), "Plot deSolve")

        If TypeOf input Is Serial3D Then
            data = {DirectCast(input, Serial3D)}
        Else
            data = input
        End If

        Return scatter3D.Plot(
            serials:=data,
            camera:=camera,
            bg:=bg
        )
    End Function

    <ExportAPI("serial3D")>
    Public Function serial3D(x As Array, y As Array, z As Array,
                             Optional name$ = "data serial",
                             Optional color As Object = "black",
                             Optional shape As LegendStyles = LegendStyles.Circle,
                             Optional alpha As Integer = 255,
                             Optional ptSize As Integer = 5) As Serial3D

        Dim cx As Double() = REnv.asVector(Of Double)(x)
        Dim cy As Double() = REnv.asVector(Of Double)(y)
        Dim cz As Double() = REnv.asVector(Of Double)(z)
        Dim aligns As Array() = Vectorization.VectorAlignment(cx, cy, cz).ToArray
        Dim points As New List(Of NamedValue(Of Point3D))

        cx = aligns(0)
        cy = aligns(1)
        cz = aligns(2)

        For i As Integer = 0 To aligns(Scan0).Length - 1
            points += New NamedValue(Of Point3D) With {
                .Name = Nothing,
                .Value = New Point3D With {
                    .X = cx(i),
                    .Y = cy(i),
                    .Z = cz(i)
                }
            }
        Next

        Return New Serial3D With {
            .Color = RColorPalette _
                .getColor(color) _
                .DoCall(AddressOf TranslateColor) _
                .Alpha(alpha),
            .PointSize = ptSize,
            .Shape = shape,
            .Title = name,
            .Points = points.ToArray
        }
    End Function
End Module
