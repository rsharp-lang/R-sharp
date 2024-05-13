#Region "Microsoft.VisualBasic::51b486c571d8784508a15430d9c4197c, studio\Rsharp_kit\MLkit\dataset\FeatureDescription.vb"

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

    '   Total Lines: 54
    '    Code Lines: 46
    ' Comment Lines: 0
    '   Blank Lines: 8
    '     File Size: 1.92 KB


    ' Module FeatureDescription
    ' 
    '     Function: DescribCharacter, DescribFeature, DescribLogical, DescribNumeric, GetDescriptions
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.DataFrame
Imports SMRUCC.Rsharp.Runtime.Vectorization

Public Module FeatureDescription

    Public Function GetDescriptions() As IEnumerable(Of String)
        Return {
            "min", "max", "width", "mean", "sd", "rsd"
        }
    End Function

    <Extension>
    Public Function DescribFeature(feature As FeatureVector) As Array
        If feature.type Is GetType(Boolean) Then
            Return DescribLogical(feature.vector)
        ElseIf DataFramework.IsNumericType(feature.type) Then
            Return DescribNumeric(CLRVector.asNumeric(feature.vector))
        ElseIf feature.type Is GetType(String) Then
            Return DescribCharacter(feature.vector)
        Else
            Throw New NotImplementedException
        End If
    End Function

    Private Function DescribNumeric(v As Double()) As Double()
        Return {
            v.Min, v.Max, New DoubleRange(v).Length, v.Average, v.SD, v.RSD
        }
    End Function

    Private Function DescribCharacter(v As String()) As String()
        Dim order = v.OrderBy(Function(s) s).ToArray
        Dim counts = order _
            .GroupBy(Function(s) s) _
            .OrderByDescending(Function(g) g.Count) _
            .ToArray

        Return {
            order.First, order.Last, counts.Length.ToString, counts.First.Key, "NA", "NA"
        }
    End Function

    Private Function DescribLogical(v As Boolean()) As Boolean()
        Dim logicalGroup = v.GroupBy(Function(b) b).OrderByDescending(Function(g) g.Count).ToArray

        Return {
            False, True, True, logicalGroup.First.Key, False, False
        }
    End Function
End Module
