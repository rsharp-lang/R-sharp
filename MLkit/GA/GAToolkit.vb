Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Components.Interface

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
End Module
