Imports Microsoft.VisualBasic.DataMining.KMeans

Public Class dbscanResult
    Public Property cluster As EntityClusterModel()
    Public Property isseed As String()
    Public Property eps As Double
    Public Property MinPts As Integer
End Class

Public Enum dbScanMethods
    hybrid
    raw
    dist
End Enum