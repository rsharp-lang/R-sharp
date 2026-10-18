Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports gpu = Microsoft.VisualBasic.Computing.ILCuda.GPUTensor
Imports ILCudaRuntime = Microsoft.VisualBasic.Computing.ILCuda.Runtime
Imports tfCompute = Microsoft.VisualBasic.MachineLearning.TensorFlow.Compute

<Package("CUDA")>
Module CUDATools

    <ExportAPI("register_cuda_tensor")>
    Public Function register_cuda_tensor(p As Precision) As Boolean
        Dim opts As New ILCudaRuntime.EngineOptions()
        Dim check As Boolean

        Select Case p
            Case Precision.fp64 : check = CudaTensor.Register(opts)
            Case Precision.fp32 : check = CudaTensorF.Register(opts)
            Case Precision.fp16
                Throw New NotImplementedException
        End Select

        If Not check Then
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
    Public Sub unregister_cuda(p As Precision)
        Select Case p
            Case Precision.fp64 : Call tfCompute.SIMDTensor.Register()
            Case Precision.fp32 : Call tfCompute.SIMDTensorF.Register()
            Case Precision.fp16 : Call tfCompute.SIMDTensorH.Register()
        End Select
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="expr">
    ''' the math expression that will be compute in CUDA, value should be a runtime function expression
    ''' </param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("compute")>
    Public Function compute(expr As Object, <RListObjectArgument> args As list, Optional env As Environment = Nothing)

    End Function
End Module
