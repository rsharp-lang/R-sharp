﻿#Region "Microsoft.VisualBasic::c36661ab574428034bae49f421919b99, studio\Rsharp_kit\MLkit\dataMining\dbscanResult.vb"

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

    '   Total Lines: 40
    '    Code Lines: 28 (70.00%)
    ' Comment Lines: 4 (10.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (20.00%)
    '     File Size: 1.09 KB


    ' Class dbscanResult
    ' 
    '     Properties: classLabels, cluster, dataLabels, eps, isseed
    '                 MinPts
    ' 
    '     Function: extractClusterId
    ' 
    ' Enum dbScanMethods
    ' 
    '     dist, hybrid, raw
    ' 
    '  
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining.KMeans

Public Class dbscanResult

    Public Property cluster As EntityClusterModel()
    Public Property isseed As String()
    Public Property eps As Double
    Public Property MinPts As Integer

    ''' <summary>
    ''' keeps the original order of the input dataset 
    ''' </summary>
    ''' <returns></returns>
    Public Property dataLabels As String()

    Public ReadOnly Property classLabels As String()
        Get
            Return extractClusterId.ToArray
        End Get
    End Property

    Private Iterator Function extractClusterId() As IEnumerable(Of String)
        Dim cluster_index As Dictionary(Of String, EntityClusterModel) = cluster.ToDictionary(Function(a) a.ID)

        For Each tag As String In dataLabels
            If cluster_index.ContainsKey(tag) Then
                Yield cluster_index(tag).Cluster
            Else
                Yield "Noise"
            End If
        Next
    End Function

End Class

Public Enum dbScanMethods
    hybrid
    raw
    dist
End Enum
