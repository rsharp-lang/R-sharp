Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.MachineLearning.ComponentModel.StoreProcedure
Imports Microsoft.VisualBasic.MachineLearning.VariationalAutoencoder
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("VAE")>
Module VAE

    ''' <summary>
    ''' create vae training algorithm
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("vae")>
    <RApiReturn(GetType(Trainer))>
    Public Function Trainer(<RRawVectorArgument> dims As Object, Optional env As Environment = Nothing) As Object
        Dim imgDims = InteropArgumentHelper.getSize(dims, env, [default]:=Nothing)

        If imgDims.StringEmpty Then
            Return Internal.debug.stop("the required image dimension size must be specificed!", env)
        End If

        With imgDims.SizeParser
            Return New Trainer(.Width, .Height)
        End With
    End Function

    <ExportAPI("train")>
    Public Function train(vae As Trainer, ds As DataSet) As Object

    End Function
End Module
