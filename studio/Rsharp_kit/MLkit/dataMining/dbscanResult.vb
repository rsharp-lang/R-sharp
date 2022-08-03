#Region "Microsoft.VisualBasic::da23cbf6529b8e28487f3f6edb8651f7, R-sharp\studio\Rsharp_kit\MLkit\dataMining\dbscanResult.vb"

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

    '   Total Lines: 14
    '    Code Lines: 12
    ' Comment Lines: 0
    '   Blank Lines: 2
    '     File Size: 324 B


    ' Class dbscanResult
    ' 
    '     Properties: cluster, eps, isseed, MinPts
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
End Class

Public Enum dbScanMethods
    hybrid
    raw
    dist
End Enum
