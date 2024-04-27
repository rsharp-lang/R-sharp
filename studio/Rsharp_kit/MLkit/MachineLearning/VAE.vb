#Region "Microsoft.VisualBasic::c5277b79488fd1ed776d0193deafb9a6, G:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//MachineLearning/VAE.vb"

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

    '   Total Lines: 65
    '    Code Lines: 20
    ' Comment Lines: 35
    '   Blank Lines: 10
    '     File Size: 2.39 KB


    ' Module VAE
    ' 
    '     Function: Trainer
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports stdvec = Microsoft.VisualBasic.Math.LinearAlgebra.Vector

<Package("VAE")>
Module VAE

    ''' <summary>
    ''' create vae training algorithm
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("vae")>
    Public Function Trainer(<RRawVectorArgument>
                            dims As Object,
                            Optional latent_dims As Integer = 100,
                            Optional env As Environment = Nothing) As Object

        'Dim imgDims = InteropArgumentHelper.getSize(dims, env, [default]:=Nothing)

        'If imgDims.StringEmpty Then
        '    Return Internal.debug.stop("the required image dimension size must be specificed!", env)
        'End If

        'With imgDims.SizeParser
        '    Return New Trainer(.Width, .Height, latent_dims)
        'End With
    End Function

    '<ExportAPI("train")>
    '<RApiReturn(GetType(VAE))>
    'Public Function train(vae As Trainer,
    '                      <RRawVectorArgument>
    '                      ds As Object,
    '                      Optional steps As Integer = 100,
    '                      Optional env As Environment = Nothing) As Object

    '    Dim dataset_input As stdvec()

    '    If TypeOf ds Is list Then
    '        dataset_input = DirectCast(ds, list).slots _
    '            .Values _
    '            .Select(Function(o) New stdvec(CLRVector.asNumeric(o))) _
    '            .ToArray
    '    ElseIf TypeOf ds Is DataSet Then
    '        dataset_input = DirectCast(ds, DataSet).DataSamples _
    '            .AsEnumerable _
    '            .Select(Function(si) New stdvec(si.vector)) _
    '            .ToArray
    '    Else
    '        Throw New NotImplementedException
    '    End If

    '    Call vae.train(dataset_input, steps)

    '    Return vae.autoencoder
    'End Function
End Module
