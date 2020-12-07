#Region "Microsoft.VisualBasic::de17f74375dd427858ab896603249bbe, studio\Rsharp_kit\MLkit\dataMining\btreeCluster.vb"

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

    ' Class btreeCluster
    ' 
    '     Properties: key, left, members, right
    ' 
    '     Function: (+2 Overloads) GetClusters, hleaf, hnode, ToHClust
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.Hierarchy
Imports Microsoft.VisualBasic.Linq

Public Class btreeCluster

    Public Property left As btreeCluster
    Public Property right As btreeCluster

    Public Property key As String
    ''' <summary>
    ''' 这个列表之中已经保存有<see cref="key"/>了
    ''' </summary>
    ''' <returns></returns>
    Public Property members As String()

    Public Shared Function GetClusters(btree As AVLTree(Of String, String)) As btreeCluster
        Return GetClusters(btree.root)
    End Function

    Private Shared Function GetClusters(btree As BinaryTree(Of String, String)) As btreeCluster
        If btree Is Nothing Then
            Return Nothing
        Else
            Return New btreeCluster With {
                .key = btree.Key,
                .members = btree.Members,
                .left = GetClusters(btree.Left),
                .right = GetClusters(btree.Right)
            }
        End If
    End Function

    Private Function hleaf() As Cluster
        ' 是一个叶子节点
        If members.IsNullOrEmpty Then
            Return New Cluster(key) With {
                .Distance = New Distance(0, 1)
            }
        Else
            Dim leafTree As New Cluster($"leaf-{key}") With {
                .Distance = New Distance(0, members.Length)
            }

            For Each key As String In members
                Call New Cluster(key) With {
                    .Distance = New Distance(0, 1)
                }.DoCall(AddressOf leafTree.AddChild)
            Next

            Return leafTree
        End If
    End Function

    Private Function hnode() As Cluster
        Dim node As New Cluster($"node-{key}")
        Dim distance As Double
        Dim cl As Cluster

        If Not left Is Nothing Then
            cl = left.ToHClust
            node.AddChild(cl)
            distance += cl.DistanceValue + 1
        End If
        If Not right Is Nothing Then
            cl = right.ToHClust
            node.AddChild(cl)
            distance += cl.DistanceValue + 1
        End If

        For Each key As String In members
            Call New Cluster(key) With {
                .Distance = New Distance(0, 1)
            }.DoCall(AddressOf node.AddChild)
        Next

        node.Distance = New Distance(distance)

        Return node
    End Function

    ''' <summary>
    ''' leaf distance value is ZERO always
    ''' </summary>
    ''' <returns></returns>
    Public Function ToHClust() As Cluster
        If left Is Nothing AndAlso right Is Nothing Then
            Return hleaf()
        Else
            Return hnode()
        End If
    End Function
End Class
