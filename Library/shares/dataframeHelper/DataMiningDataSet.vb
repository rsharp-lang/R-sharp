#If DATAMINING_DATASET Then

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports Rdataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports REnv = SMRUCC.Rsharp.Runtime

Module DataMiningDataSet

    Public Function getDataModel(x As Object, check_class As Boolean, env As Environment) As [Variant](Of Message, EntityClusterModel())
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
            Dim df = DirectCast(x, Rdataframe)
            Dim colNames As String() = df.columns _
                .Keys _
                .ToArray
            Dim has_class As Boolean = False
            Dim class_col As String = Nothing

            If check_class Then
                For Each col As String In {"class", "cluster", "group", "type", "Class", "Cluster", "Group", "Type"}
                    If df.checkClassLabels(col) Then
                        class_col = col
                        Exit For
                    End If
                Next
            End If

            Dim class_labels As String() = Nothing

            If Not class_col.StringEmpty Then
                class_labels = CLRVector.asCharacter(df(class_col))
                df = New Rdataframe With {
                    .columns = New Dictionary(Of String, Array)(df.columns),
                    .rownames = df.rownames
                }
                df.columns.Remove(class_col)
            End If

            model = df _
                .forEachRow _
                .Select(Function(r, index)
                            Return New EntityClusterModel With {
                                .ID = r.name,
                                .Properties = colNames _
                                    .SeqIterator _
                                    .ToDictionary(Function(c) c.value,
                                                  Function(i)
                                                      Return CType(r(i), Double)
                                                  End Function),
                                .Cluster = class_labels.ElementAtOrNull(index)
                            }
                        End Function) _
                .ToArray
        Else
            Return REnv.Internal.debug.stop(New InvalidProgramException(x.GetType.FullName), env)
        End If

        Return model
    End Function

    <Extension>
    Private Function checkClassLabels(df As Rdataframe, col As String) As Boolean
        Return df.hasName(col) AndAlso CLRVector _
            .asCharacter(df(col)) _
            .All(Function(str)
                     Return str.IsPattern("((class)|(cluster)|(group)).*\d+")
                 End Function)
    End Function
End Module
#End If