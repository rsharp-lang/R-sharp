#Region "Microsoft.VisualBasic::0fab0c653b51683fbc0c4705d5cae0ca, Library\R.graph\Visualize.vb"

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
    '     Function: colorByTypeGroup, renderPlot
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop

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
    Public Function renderPlot(g As NetworkGraph,
                               <RRawVectorArgument>
                               Optional canvasSize As Object = "1024,768",
                               Optional defaultColor$ = "skyblue",
                               Optional labelerIterations% = 0) As GraphicsData

        Return g.DrawImage(
            canvasSize:=InteropArgumentHelper.getSize(canvasSize),
            labelerIterations:=labelerIterations,
            defaultColor:=InteropArgumentHelper.getColor(defaultColor)
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

End Module
