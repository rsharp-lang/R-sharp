
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("hiddenMarkov")>
<RTypeExport("state", GetType(StatesObject))>
<RTypeExport("observable", GetType(Observable))>
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

    <ExportAPI("MarkovChain")>
    Public Function MarkovChain(states As StatesObject(), init As Double()) As MarkovChain
        Return New MarkovChain(states, init)
    End Function

    <ExportAPI("sequenceProb")>
    Public Function sequenceProb(markovChain As MarkovChain, <RRawVectorArgument> seq As Object, Optional env As Environment = Nothing) As Object
        Dim chain As Chain = getChain(seq)
        Dim prob As Double = markovChain.SequenceProb(chain)

        Return prob
    End Function

    Private Function getChain(seq As Object) As Chain
        If TypeOf seq Is Chain Then
            Return DirectCast(seq, Chain)
        Else
            Return New Chain() With {
                .obSequence = REnv.asVector(Of String)(seq)
            }
        End If
    End Function

    <ExportAPI("transMatrix")>
    Public Function transMatrix(markov As Object) As GeneralMatrix
        If TypeOf markov Is HMM Then
            Return DirectCast(markov, HMM).GetTransMatrix
        Else
            Return DirectCast(markov, MarkovChain).GetTransMatrix
        End If
    End Function

    <ExportAPI("HMM")>
    Public Function HMM(hiddenStates As StatesObject(), observables As Observable(), hiddenInit As Double()) As Object
        Return New HMM(hiddenStates, observables, hiddenInit)
    End Function

    <ExportAPI("bayesTheorem")>
    Public Function bayesTheorem(hmm As HMM, observation$, hiddenState$) As Double
        Return hmm.bayesTheorem(observation, hiddenState)
    End Function

    <ExportAPI("forwardAlgorithm")>
    Public Function forwardAlgorithm(hmm As HMM, <RRawVectorArgument> obs As Object) As Object
        Return hmm.forwardAlgorithm(getChain(obs))
    End Function

    <ExportAPI("backwardAlgorithm")>
    Public Function backwardAlgorithm(hmm As HMM, <RRawVectorArgument> obs As Object) As Object
        Return hmm.backwardAlgorithm(getChain(obs))
    End Function

    <ExportAPI("viterbiAlgorithm")>
    Public Function viterbiAlgorithm(hmm As HMM, <RRawVectorArgument> obs As Object) As Object
        Return hmm.viterbiAlgorithm(getChain(obs))
    End Function

    <ExportAPI("baumWelchAlgorithm")>
    Public Function baumWelchAlgorithm(hmm As HMM, <RRawVectorArgument> obs As Object) As Object
        Return hmm.baumWelchAlgorithm(getChain(obs))
    End Function

End Module
