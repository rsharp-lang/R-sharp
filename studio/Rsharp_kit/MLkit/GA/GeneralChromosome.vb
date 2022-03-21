#Region "Microsoft.VisualBasic::a91cc7c722a375782a5ad13217a78e28, R-sharp\studio\Rsharp_kit\MLkit\GA\GeneralChromosome.vb"

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

    '   Total Lines: 50
    '    Code Lines: 41
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 2.11 KB


    ' Class GeneralChromosome
    ' 
    '     Properties: MutationRate, UniqueHashKey
    ' 
    '     Function: Crossover, Mutate, ToString
    ' 
    ' /********************************************************************************/

#End Region

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
