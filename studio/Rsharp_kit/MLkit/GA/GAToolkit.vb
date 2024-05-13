#Region "Microsoft.VisualBasic::fb55b25fec99aa2d56edea1a819425a1, studio\Rsharp_kit\MLkit\GA\GAToolkit.vb"

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

    '   Total Lines: 94
    '    Code Lines: 63
    ' Comment Lines: 20
    '   Blank Lines: 11
    '     File Size: 3.81 KB


    ' Module GAToolkit
    ' 
    '     Function: population, runANNTraining, template
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Population
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork
Imports Microsoft.VisualBasic.MachineLearning.NeuralNetwork.DarwinismHybrid
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' A Genetic Algorithm Toolkit for R# language
''' </summary>
<Package("GA_toolkit")>
Module GAToolkit

    ''' <summary>
    ''' create a new chromosome template.
    ''' </summary>
    ''' <param name="seed"></param>
    ''' <param name="crossover"></param>
    ''' <param name="mutate"></param>
    ''' <param name="uniqueId"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("template")>
    Public Function template(seed As Object, crossover As RFunction, mutate As RFunction, uniqueId As RFunction, Optional env As Environment = Nothing) As GeneralChromosome
        Return New GeneralChromosome With {
            .crossoverGeneric = Function(a, b) crossover.Invoke({a, b}, env),
            .mutateGeneric = Function(obj, rate) mutate.Invoke({obj, rate}, env),
            .MutationRate = 0.1,
            .target = seed,
            .uniqueId = Function(obj) uniqueId.Invoke({obj}, env)
        }
    End Function

    <ExportAPI("population")>
    Public Function population(ancestor As GeneralChromosome,
                               Optional mutation_rate As Double = 0.2,
                               Optional population_size As Integer = 500,
                               Optional env As Environment = Nothing) As Population(Of GeneralChromosome)

        If mutation_rate <= 0 Then

        End If

        ancestor.MutationRate = mutation_rate

        Return ancestor.InitialPopulation(popSize:=population_size, parallel:=True)
    End Function

    ''' <summary>
    ''' Run ANN training under the GA framework
    ''' </summary>
    ''' <param name="trainingSet"></param>
    ''' <param name="mutationRate#"></param>
    ''' <param name="populationSize%"></param>
    ''' <param name="iterations%"></param>
    ''' <returns></returns>
    <ExportAPI("ANN.training")>
    <RApiReturn(GetType(Network))>
    Public Function runANNTraining(ANN As Object, trainingSet As DataSet,
                                   Optional mutationRate# = 0.2,
                                   Optional populationSize% = 1000,
                                   Optional iterations% = 10000,
                                   Optional env As Environment = Nothing) As Object

        Dim trainingMatrix As Sample() = trainingSet _
            .DataSamples _
            .AsEnumerable _
            .ToArray
        Dim network As Network

        If ANN Is Nothing Then
            Return Internal.debug.stop("the required ANN model object can not be nothing!", env)
        ElseIf TypeOf ANN Is ANNTrainer Then
            network = DirectCast(ANN, ANNTrainer).NeuronNetwork
        ElseIf TypeOf ANN Is Network Then
            network = DirectCast(ANN, Network)
        Else
            Return Message.InCompatibleType(GetType(Network), ANN.GetType, env)
        End If

        Return network.RunGATrainer(
            trainingSet:=trainingMatrix,
            mutationRate:=mutationRate,
            populationSize:=populationSize,
            iterations:=iterations
        )
    End Function
End Module
