#Region "Microsoft.VisualBasic::77783f837b6e743412f45a339d2fb0c2, Library\R.graph\Layouts.vb"

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

    ' Module Layouts
    ' 
    '     Function: forceDirect, orthogonalLayout
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports R.graphics
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("igraph.layouts")>
Module Layouts

    ''' <summary>
    ''' Do force directed layout
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="iterations"></param>
    ''' <param name="clearScreen">
    ''' Clear of the console screen when display the progress bar.
    ''' </param>
    ''' <returns></returns>
    <ExportAPI("layout.force_directed")>
    Public Function forceDirect(g As NetworkGraph,
                                Optional iterations% = 1000,
                                Optional clearScreen As Boolean = False) As NetworkGraph

        Return g.doForceLayout(
            showProgress:=True,
            iterations:=iterations,
            clearScreen:=clearScreen
        )
    End Function

    <ExportAPI("layout.orthogonal")>
    Public Function orthogonalLayout(g As NetworkGraph,
                                     <RRawVectorArgument>
                                     Optional gridSize As Object = "10,10",
                                     Optional delta# = 1) As NetworkGraph

        Dim size As Size = InteropArgumentHelper _
            .getSize(gridSize) _
            .SizeParser

        Call Orthogonal.DoLayout(g, size, delta)

        Return g
    End Function
End Module
