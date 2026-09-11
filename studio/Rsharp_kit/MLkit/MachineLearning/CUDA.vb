Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

<Package("CUDA")>
Module CUDATools

    <ExportAPI("register_cuda_tensor")>
    Public Function register_cuda_tensor() As Boolean
        Dim opts As New ILCudaRuntime.EngineOptions()

        If Not gpu.CudaTensor.Register(opts) Then
            Console.WriteLine($"    注册失败: {gpu.CudaTensor.LastError}")

            For Each line In opts.Diagnostics
                Console.WriteLine($"      {line}")
            Next

            Return False
        Else
            Return True
        End If
    End Function

    <ExportAPI("unregister_cuda")>
    Public Sub unregister_cuda()
        Call tfCompute.SIMDTensor.Register()
    End Sub
End Module
