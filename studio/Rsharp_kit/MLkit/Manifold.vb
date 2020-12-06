Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.visualize
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.DataMining.UMAP
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("umap")>
Module Manifold

    <ExportAPI("umap")>
    <RApiReturn(GetType(list))>
    Public Function umapProjection(<RRawVectorArgument> data As Object, Optional dimension% = 2, Optional env As Environment = Nothing) As Object
        Dim labels As String()
        Dim matrix As Double()()

        If TypeOf data Is Rdataframe Then
            labels = DirectCast(data, Rdataframe).getRowNames
            matrix = DirectCast(data, Rdataframe) _
                .forEachRow _
                .Select(Function(r)
                            Return r.Select(Function(n) CDbl(n)).ToArray
                        End Function) _
                .ToArray
        Else
            Dim raw As pipeline = pipeline.TryCreatePipeline(Of DataSet)(data, env)

            If raw.isError Then
                Return raw.getError
            End If

            Dim rawMatrix As DataSet() = raw.populates(Of DataSet)(env).ToArray
            Dim cols As String() = rawMatrix.PropertyNames

            labels = rawMatrix.Keys(distinct:=False)
            matrix = rawMatrix.Select(Function(r) r(cols)).ToArray
        End If

        Dim umap As New Umap(
            distance:=AddressOf DistanceFunctions.CosineForNormalizedVectors,
            dimensions:=dimension
        )
        Dim nEpochs As Integer

        Call Console.WriteLine("Initialize fit..")

        nEpochs = umap.InitializeFit(matrix)

        Console.WriteLine("- Done")
        Console.WriteLine()
        Console.WriteLine("Calculating..")

        For i As Integer = 0 To nEpochs - 1
            Call umap.Step()

            If i Mod 10 = 0 Then
                Console.WriteLine($"- Completed {i + 1} of {nEpochs}")
            End If
        Next

        Return New list With {
            .slots = New Dictionary(Of String, Object) From {
                {"labels", labels},
                {"umap", umap}
            }
        }
    End Function

    <ExportAPI("as.graph")>
    <RApiReturn(GetType(NetworkGraph))>
    Public Function asGraph(umap As Umap, <RRawVectorArgument> labels As Object, Optional env As Environment = Nothing) As Object
        Dim labelList As String() = REnv.asVector(Of String)(labels)
        Dim g As NetworkGraph = umap.CreateGraph(labelList)

        Return g
    End Function
End Module
