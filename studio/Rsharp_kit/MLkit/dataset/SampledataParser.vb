#Region "Microsoft.VisualBasic::d7c009ddb11fdb8fe07c858e9407e9e3, studio\Rsharp_kit\MLkit\dataset\SampledataParser.vb"

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

    '   Total Lines: 76
    '    Code Lines: 66 (86.84%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 10 (13.16%)
    '     File Size: 3.04 KB


    ' Module SampledataParser
    ' 
    '     Function: Convert, ConvertVAE
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

Module SampledataParser

    Public Function Convert(x As Object, env As Environment) As [Variant](Of Message, SampleData())
        If x Is Nothing Then
            Return RInternal.debug.stop("the required sample data should not be nothing", env)
        End If

        Throw New NotImplementedException
    End Function

    Public Function ConvertVAE(x As Object, env As Environment) As [Variant](Of Message, SampleData())
        If x Is Nothing Then
            Return RInternal.debug.stop("the required sample data x should not be nothing!", env)
        End If

        If TypeOf x Is dataframe Then
            Dim rows = DirectCast(x, dataframe).forEachRow.ToArray

            Return rows _
                .Select(Function(a)
                            Dim v As Double() = CLRVector.asNumeric(a.value)

                            Return New SampleData With {
                                .id = a.name,
                                .features = v,
                                .labels = v
                            }
                        End Function) _
                .ToArray
        ElseIf x.GetType.ImplementInterface(Of INumericMatrix) Then
            Dim rows = DirectCast(x, INumericMatrix).ArrayPack
            Dim labels As String()

            If x.GetType.ImplementInterface(Of ILabeledMatrix) Then
                labels = DirectCast(x, ILabeledMatrix).GetLabels.ToArray
            Else
                labels = rows.Select(Function(r, i) $"r_{i + 1}").ToArray
            End If

            Return rows _
                .Select(Function(r, i)
                            Return New SampleData With {
                                .id = labels(i),
                                .labels = r,
                                .features = r
                            }
                        End Function) _
                .ToArray
        ElseIf TypeOf x Is list Then
            Return DirectCast(x, list).slots _
                .Select(Function(r)
                            Dim v As Double() = CLRVector.asNumeric(r.Value)

                            Return New SampleData With {
                                .id = r.Key,
                                .features = v,
                                .labels = v
                            }
                        End Function) _
                .ToArray
        Else
            Return Message.InCompatibleType(GetType(dataframe), x.GetType, env)
        End If
    End Function
End Module
