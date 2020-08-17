Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree

Public Class btreeCluster

    Public Property left As btreeCluster
    Public Property right As btreeCluster

    Public Property key As String
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
End Class
