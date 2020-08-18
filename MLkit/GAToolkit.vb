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

Public Class GeneralChromosome : Implements Chromosome(Of GeneralChromosome)

    Public Property MutationRate As Double Implements Chromosome(Of GeneralChromosome).MutationRate

    Public ReadOnly Property UniqueHashKey As String Implements Chromosome(Of GeneralChromosome).UniqueHashKey

    Friend crossoverGeneric As Func(Of Object, Object, Object)
    Friend mutateGeneric As Func(Of Object, Double, Object)
    Friend uniqueId As Func(Of Object, String)
    Friend target As Object

    Public Iterator Function Crossover(another As GeneralChromosome) As IEnumerable(Of GeneralChromosome) Implements Chromosome(Of GeneralChromosome).Crossover
        Dim newObj As Object = crossoverGeneric(Me.target, another.target)
        Dim newId As String = uniqueId(newObj)

        Yield New GeneralChromosome With {
            .crossoverGeneric = crossoverGeneric,
            .mutateGeneric = mutateGeneric,
            .MutationRate = MutationRate,
            .uniqueId = uniqueId,
            .target = newObj,
            ._UniqueHashKey = newId
        }
    End Function

    Public Function Mutate() As GeneralChromosome Implements Chromosome(Of GeneralChromosome).Mutate
        Dim newObj As Object = mutateGeneric(Me.target, MutationRate)
        Dim newId As String = uniqueId(newObj)

        Return New GeneralChromosome With {
            .crossoverGeneric = crossoverGeneric,
            .mutateGeneric = mutateGeneric,
            .MutationRate = MutationRate,
            .uniqueId = uniqueId,
            .target = newObj,
            ._UniqueHashKey = newId
        }
    End Function

    Public Overrides Function ToString() As String
        Return target.ToString
    End Function
End Class