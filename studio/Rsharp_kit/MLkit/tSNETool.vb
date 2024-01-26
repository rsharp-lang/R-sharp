#Region "Microsoft.VisualBasic::9c2d25258a4e44e04b7f2c33ff90b06f, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//tSNETool.vb"

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

    '   Total Lines: 121
    '    Code Lines: 90
    ' Comment Lines: 7
    '   Blank Lines: 24
    '     File Size: 4.03 KB


    ' Module tSNETool
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: createResultTable, createtSNEAlgorithm, LoadDataSet, Solve
    ' 
    ' Class tSNEDataSet
    ' 
    '     Properties: dimension
    ' 
    '     Function: GetEmbedding, GetOutput
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.MachineLearning.tSNE
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

<Package("t-SNE")>
Module tSNETool

    Sub New()
        Call Internal.generic.add("plot", GetType(tSNE), AddressOf datasetKit.EmbeddingRender)
        Call Internal.generic.add("plot", GetType(tSNEDataSet), AddressOf datasetKit.EmbeddingRender)
        Call Internal.Object.Converts.makeDataframe.addHandler(GetType(tSNEDataSet), AddressOf createResultTable)
    End Sub

    <RGenericOverloads("as.data.frame")>
    Private Function createResultTable(result As tSNEDataSet, args As list, env As Environment) As dataframe
        Return result.GetOutput
    End Function

    ''' <summary>
    ''' create t-SNE algorithm module
    ''' </summary>
    ''' <param name="perplexity"></param>
    ''' <param name="dimension"></param>
    ''' <param name="epsilon"></param>
    ''' <returns></returns>
    <ExportAPI("t.SNE")>
    Public Function createtSNEAlgorithm(Optional perplexity As Double = 30,
                                        Optional dimension As Integer = 2,
                                        Optional epsilon As Double = 10) As tSNE

        Return New tSNE(perplexity, dimension, epsilon)
    End Function

    <ExportAPI("data")>
    <RApiReturn(GetType(tSNEDataSet))>
    Public Function LoadDataSet(tSNE As tSNE, <RRawVectorArgument> dataset As Object, Optional env As Environment = Nothing) As Object
        If dataset Is Nothing Then
            Return Internal.debug.stop("the required dataset can not be nothing!", env)
        End If

        Dim matrix As New List(Of Double())
        Dim labels As String()

        If TypeOf dataset Is dataframe Then
            With DirectCast(dataset, dataframe)
                labels = .getRowNames

                For Each row In .forEachRow
                    matrix.Add(CLRVector.asNumeric(row.ToArray))
                Next
            End With
        Else
            Return Message.InCompatibleType(GetType(dataframe), dataset.GetType, env)
        End If

        Call tSNE.InitDataRaw(matrix)

        Return New tSNEDataSet With {
            .algorithm = tSNE,
            .labels = labels
        }
    End Function

    <ExportAPI("solve")>
    Public Function Solve(tSNE As tSNEDataSet, Optional iterations% = 500) As tSNEDataSet
        Dim nEpochs As Integer = iterations

        Console.WriteLine("Calculating...")

        For i As Integer = 0 To iterations
            Call tSNE.algorithm.Step()

            If (100 * i / nEpochs) Mod 5 = 0 Then
                Console.WriteLine($"- Completed {i + 1} of {nEpochs} [{CInt(100 * i / nEpochs)}%]")
            End If
        Next

        Return tSNE
    End Function

End Module

Public Class tSNEDataSet : Inherits IDataEmbedding

    Friend algorithm As tSNE
    Friend labels As String()

    Public Overrides ReadOnly Property dimension As Integer
        Get
            Return algorithm.dimension
        End Get
    End Property

    Public Function GetOutput() As dataframe
        Dim matrix = algorithm.GetEmbedding
        Dim result As New dataframe With {
            .columns = New Dictionary(Of String, Array),
            .rownames = labels
        }
        Dim width As Integer = matrix(Scan0).Length

        For i As Integer = 0 To width - 1
#Disable Warning
            result.columns($"X{i + 1}") = matrix _
                .Select(Function(r) r(i)) _
                .ToArray
#Enable Warning
        Next

        Return result
    End Function

    Public Overrides Function GetEmbedding() As Double()()
        Return algorithm.GetEmbedding
    End Function
End Class
