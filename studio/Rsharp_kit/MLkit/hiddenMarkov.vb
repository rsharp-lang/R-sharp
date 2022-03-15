#Region "Microsoft.VisualBasic::4f2d29a7c10eb6ba7a4ea015676e614f, R-sharp\studio\Rsharp_kit\MLkit\hiddenMarkov.vb"

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


     Code Statistics:

        Total Lines:   163
        Code Lines:    125
        Comment Lines: 10
        Blank Lines:   28
        File Size:     5.75 KB


    ' Module hiddenMarkov
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: backwardAlgorithm, baumWelchAlgorithm, bayesTheorem, forwardAlgorithm, getChain
    '               HMM, MarkovChain, printAlpha, printBeta, printViterbi
    '               sequenceProb, statesMatrix, transMatrix, viterbiAlgorithm
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain
Imports Microsoft.VisualBasic.DataMining.HiddenMarkovChain.Models
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.MIME.application.json
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("hiddenMarkov")>
<RTypeExport("state", GetType(StatesObject))>
<RTypeExport("observable", GetType(Observable))>
Module hiddenMarkov

    Sub New()
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of Alpha)(AddressOf printAlpha)
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of Beta)(AddressOf printBeta)
        Call Internal.ConsolePrinter.AttachConsoleFormatter(Of viterbiSequence)(AddressOf printViterbi)
    End Sub

    Private Function printAlpha(a As Alpha) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine($"alpha: {a.alphaF}")
        Call sb.AppendLine(New String("-"c, 32))

        For Each line In a.alphas
            Call sb.AppendLine(line.ToArray.GetJson)
        Next

        Return sb.ToString
    End Function

    Private Function printBeta(b As Beta) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine($"beta: {b.betaF}")
        Call sb.AppendLine(New String("-"c, 32))

        For Each line In b.betas
            Call sb.AppendLine(line.ToArray.GetJson)
        Next

        Return sb.ToString
    End Function

    Private Function printViterbi(seq As viterbiSequence) As String
        Dim sb As New StringBuilder

        Call sb.AppendLine($"state sequence: {seq.stateSequence.JoinBy("->")}")
        Call sb.AppendLine($"termination probability: {seq.terminationProbability}")
        Call sb.AppendLine(New String("-"c, 32))

        For Each line In seq.trellisSequence
            Call sb.AppendLine(line.ToArray.GetJson)
        Next

        Return sb.ToString
    End Function

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
