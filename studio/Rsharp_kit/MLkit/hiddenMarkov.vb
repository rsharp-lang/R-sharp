
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("hiddenMarkov")>
Module hiddenMarkov

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="matrix">
    ''' A dataframe matrix object should contains fields:
    ''' 
    ''' ``states`` and probability
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("as.statesMatrix")>
    <RApiReturn(GetType(StatesObject))>
    Public Function statesMatrix(<RRawVectorArgument> matrix As Object, Optional env As Environment = Nothing) As Object
        If TypeOf matrix Is dataframe Then
            Dim table As dataframe = DirectCast(matrix, dataframe)
            Dim states As StatesObject() = table _
                .forEachRow _
                .Select(Function(row)
                            Return New StatesObject With {
                                .state = row.name,
                                .prob = REnv.asVector(Of Double)(row.value)
                            }
                        End Function) _
                .ToArray

            Return states
        Else
            With pipeline.TryCreatePipeline(Of StatesObject)(matrix, env)
                If .isError Then
                    Return .getError
                Else
                    Return .populates(Of StatesObject)(env).ToArray
                End If
            End With
        End If
    End Function

End Module
