#If DATAMINING_DATASET Then

Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

Module DataMiningDataSet

    Public Function getDataModel(x As Object, env As Environment) As [Variant](Of Message, EntityClusterModel())
        Dim model As EntityClusterModel()

        If x Is Nothing Then
            Return Internal.debug.stop("the given dataset should not be nothing!", env)
        End If

        If x.GetType.IsArray Then
#Disable Warning
            Select Case REnv.MeasureArrayElementType(x)
                Case GetType(DataSet)
                    model = DirectCast(REnv.asVector(Of DataSet)(x), DataSet()).ToKMeansModels
                Case GetType(EntityClusterModel)
                    model = REnv.asVector(Of EntityClusterModel)(x)
                Case Else
                    Return REnv.Internal.debug.stop(New InvalidProgramException(x.GetType.FullName), env)
            End Select
#Enable Warning
        ElseIf TypeOf x Is Rdataframe Then
            Dim colNames As String() = DirectCast(x, Rdataframe).columns _
                .Keys _
                .ToArray

            model = DirectCast(x, Rdataframe) _
                .forEachRow _
                .Select(Function(r)
                            Return New EntityClusterModel With {
                                .ID = r.name,
                                .Properties = colNames _
                                    .SeqIterator _
                                    .ToDictionary(Function(c) c.value,
                                                  Function(i)
                                                      Return CType(r(i), Double)
                                                  End Function)
                            }
                        End Function) _
                .ToArray
        Else
            Return REnv.Internal.debug.stop(New InvalidProgramException(x.GetType.FullName), env)
        End If

        Return model
    End Function
End Module
#End If