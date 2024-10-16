#Region "Microsoft.VisualBasic::4c9b5cd2a1d915ff5d4458f3746bcf51, studio\Rsharp_kit\MLkit\ROC.vb"

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

    '   Total Lines: 74
    '    Code Lines: 60 (81.08%)
    ' Comment Lines: 6 (8.11%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 8 (10.81%)
    '     File Size: 2.70 KB


    ' Class ROC
    ' 
    '     Properties: accuracy, All, AUC, BER, BestThreshold
    '                 F1Score, F2Score, FN, FP, FPR
    '                 NPV, precision, sensibility, specificity, threshold
    '                 TN, TP
    ' 
    '     Function: IEnumerable, IEnumerable_GetEnumerator
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.DataMining
Imports Microsoft.VisualBasic.My.JavaScript

Public Class ROC : Inherits JavaScriptObject
    Implements IEnumerable(Of Evaluation.Validation)

    Public Property threshold As Double()
    Public Property specificity As Double()
    ''' <summary>
    ''' TPR
    ''' </summary>
    ''' <returns></returns>
    Public Property sensibility As Double()
    Public Property accuracy As Double()
    Public Property precision As Double()
    Public Property BER As Double()
    Public Property FPR As Double()
    Public Property NPV As Double()
    Public Property F1Score As Double()
    Public Property F2Score As Double()
    Public Property All As Integer()
    Public Property TP As Integer()
    Public Property FP As Integer()
    Public Property TN As Integer()
    Public Property FN As Integer()

    Public ReadOnly Property AUC() As Double
        Get
            Dim TPR = sensibility
            Dim FPR = Me.FPR
            ' 20241012
            ' make both data be sorted via the TPR
            Dim sort = TPR.Zip(second:=FPR) _
                .Where(Function(z)
                           Return Not (z.First.IsNaNImaginary OrElse z.Second.IsNaNImaginary)
                       End Function) _
                .OrderBy(Function(z) z.First) _
                .ToArray

            TPR = sort.Select(Function(a) a.First).ToArray
            FPR = sort.Select(Function(a) a.Second).ToArray

            Return Evaluation.SimpleAUC(TPR, FPR) / 100
        End Get
    End Property

    Public ReadOnly Property BestThreshold As Double
        Get
            Return threshold(Evaluation.BestThreshold(sensibility, FPR))
        End Get
    End Property

    Protected Overrides Iterator Function IEnumerable_GetEnumerator() As IEnumerator
        Yield IEnumerable()
    End Function

    Private Overloads Iterator Function IEnumerable() As IEnumerator(Of Evaluation.Validation) Implements IEnumerable(Of Evaluation.Validation).GetEnumerator
        If threshold.Length > 1000 Then
            For i As Integer = 0 To threshold.Length - 1 Step CInt(threshold.Length) / 1000
                Yield New Evaluation.Validation With {
                    .specificity = specificity(i),
                    .sensibility = sensibility(i)
                }
            Next
        Else
            For i As Integer = 0 To threshold.Length - 1
                Yield New Evaluation.Validation With {
                    .specificity = specificity(i),
                    .sensibility = sensibility(i)
                }
            Next
        End If
    End Function
End Class
