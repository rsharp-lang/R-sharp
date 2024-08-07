﻿#Region "Microsoft.VisualBasic::e7f9a42d3ad81b2a21f4770a5fcb2d62, Library\igraph\builder.vb"

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

    '   Total Lines: 52
    '    Code Lines: 34 (65.38%)
    ' Comment Lines: 11 (21.15%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (13.46%)
    '     File Size: 2.12 KB


    ' Module builder
    ' 
    '     Function: FromCorrelations, SimilarityGraph
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Math.DataFrame
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' helper module for convert datasets to network graph object
''' </summary>
<Package("igraph.builder")>
Module builder

    ''' <summary>
    ''' create a network graph based on the item correlations
    ''' </summary>
    ''' <param name="x">a correlation matrix</param>
    ''' <param name="threshold">the absolute threshold value of the correlation value</param>
    ''' <param name="pvalue"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("correlation.graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function FromCorrelations(x As CorrelationMatrix,
                                     Optional threshold As Double = 0.65,
                                     Optional pvalue As Double = 1,
                                     Optional group As list = Nothing,
                                     Optional env As Environment = Nothing) As Object

        Dim g As NetworkGraph = x.BuildNetwork(threshold, pvalue).Item1

        If Not group Is Nothing Then
            Dim class_labels As Dictionary(Of String, String) = group.AsGeneric(env, "no_class")

            For Each v As Node In g.vertex
                v.data(NamesOf.REFLECTION_ID_MAPPING_NODETYPE) = class_labels _
                    .TryGetValue(v.label, [default]:="no_class")
            Next
        End If

        Return g
    End Function

    <ExportAPI("similarity_graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function SimilarityGraph(x As DistanceMatrix, Optional cutoff As Double = 0.6) As Object
        Return x.BuildNetwork(cutoff)
    End Function
End Module
