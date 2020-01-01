#Region "Microsoft.VisualBasic::004295d7ccb1e61074f84816dc0b900d, Library\R.graph\Dijkstra.vb"

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

' Module Dijkstra
' 
'     Function: CreateRouter, DijkstraRoutine
' 
' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.GraphTheory
Imports Microsoft.VisualBasic.Data.GraphTheory.Dijkstra
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports REnv = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' Graph router
''' </summary>
<Package("igraph.dijkstra")>
Module Dijkstra

    Sub New()
        REnv.AttachHtmlFormatter(Of Route)(AddressOf printRoutine)
    End Sub

    Private Function printRoutine(obj As Object) As String
        Dim route As Route = DirectCast(obj, Route)
        Dim summary As New StringBuilder

        Call summary.AppendLine(route.ToString)

        For Each link As VertexEdge In route.Connections
            Call summary.AppendLine($"  {link.U.label} -> {link.V.label}")
        Next

        Return summary.ToString
    End Function

    <ExportAPI("router.dijkstra")>
    Public Function CreateRouter(g As NetworkGraph, Optional undirected As Boolean = False) As DijkstraRouter
        Return DijkstraRouter.FromNetwork(g, undirected)
    End Function

    <ExportAPI("routine.min_cost")>
    Public Function DijkstraRoutine(router As DijkstraRouter,
                                    from As Object,
                                    [to] As Object,
                                    Optional env As Environment = Nothing) As Object

        If from Is Nothing Then
            Return REnv.debug.stop("start point is nothing!", env)
        ElseIf [to] Is Nothing Then
            Return REnv.debug.stop("stop point is not specific!", env)
        End If

        If from.GetType Is GetType(String) Then
            from = router.GetLocation(from)
        End If
        If [to].GetType Is GetType(String) Then
            [to] = router.GetLocation([to])
        End If

        Return router.CalculateMinCost(from, [to])
    End Function
End Module

