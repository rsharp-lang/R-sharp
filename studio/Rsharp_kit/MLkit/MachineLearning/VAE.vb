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
    <RApiReturn(GetType(Trainer))>
    Public Function Trainer(<RRawVectorArgument>
                            dims As Object,
                            Optional latent_dims As Integer = 100,
                            Optional env As Environment = Nothing) As Object

        Dim imgDims = InteropArgumentHelper.getSize(dims, env, [default]:=Nothing)

        If imgDims.StringEmpty Then
            Return Internal.debug.stop("the required image dimension size must be specificed!", env)
        End If

        With imgDims.SizeParser
            Return New Trainer(.Width, .Height, latent_dims)
        End With
    End Function

    <ExportAPI("train")>
    <RApiReturn(GetType(VAE))>
    Public Function train(vae As Trainer,
                          <RRawVectorArgument>
                          ds As Object,
                          Optional steps As Integer = 100,
                          Optional env As Environment = Nothing) As Object

        Dim dataset_input As stdvec()

        If TypeOf ds Is list Then
            dataset_input = DirectCast(ds, list).slots _
                .Values _
                .Select(Function(o) New stdvec(CLRVector.asNumeric(o))) _
                .ToArray
        ElseIf TypeOf ds Is DataSet Then
            dataset_input = DirectCast(ds, DataSet).DataSamples _
                .AsEnumerable _
                .Select(Function(si) New stdvec(si.vector)) _
                .ToArray
        Else
            Throw New NotImplementedException
        End If

        Call vae.train(dataset_input, steps)

        Return vae.autoencoder
    End Function
End Module
