Imports Microsoft.VisualBasic.ComponentModel.Algorithm.BinaryTree

Public Class btreeCluster

    Public Property left As btreeCluster
    Public Property right As btreeCluster

    Public Property key As String
    Public Property members As String()

    Public Shared Function GetClusters(btree As AVLTree(Of String, String)) As btreeCluster

    End Function

End Class
