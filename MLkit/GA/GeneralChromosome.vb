Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.MachineLearning.Darwinism.Models
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Components.Interface

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