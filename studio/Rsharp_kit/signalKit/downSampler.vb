
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Math.DownSampling
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

''' <summary>
''' the down sampler of the time signal data
''' </summary>
<Package("downSampler")>
Module downSampler

    ''' <summary>
    ''' Largest Triangle One Bucket
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("LTOB")>
    Public Function ltob() As DownSamplingAlgorithm
        Return DSAlgorithms.LTOB
    End Function

    ''' <summary>
    ''' Largest Triangle Three Bucket
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("LTTB")>
    Public Function lttb() As DownSamplingAlgorithm
        Return DSAlgorithms.LTTB
    End Function

    ''' <summary>
    ''' Largest Triangle Dynamic
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("LTD")>
    Public Function ltd() As DownSamplingAlgorithm
        Return DSAlgorithms.LTD
    End Function

    ''' <summary>
    ''' Maximum and minimum value
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("MAXMIN")>
    Public Function maxmin() As DownSamplingAlgorithm
        Return DSAlgorithms.MAXMIN
    End Function

    ''' <summary>
    ''' OSIsoft PI PlotValues
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("PIPLOT")>
    Public Function piplot() As DownSamplingAlgorithm
        Return DSAlgorithms.PIPLOT
    End Function

    <ExportAPI("down_sampling")>
    <RApiReturn(GetType(ITimeSignal))>
    Public Function down_sampling(alg As DownSamplingAlgorithm,
                                  <RRawVectorArgument> x As Object,
                                  <RRawVectorArgument>
                                  Optional y As Object = Nothing,
                                  Optional n As Integer = 100,
                                  Optional env As Environment = Nothing) As Object
        If x Is Nothing Then
            Return Nothing
        End If

        Dim signal As PlainEvent()

        If Not y Is Nothing Then
            Dim vx As Double() = CLRVector.asNumeric(x)
            Dim vy As Double() = CLRVector.asNumeric(y)

            signal = vx.Select(Function(xi, i) New PlainEvent(xi, y(i))).ToArray
        Else
            Return Message.InCompatibleType(GetType(ITimeSignal), x.GetType, env)
        End If

        Return alg.process(signal.ToList, n).ToArray
    End Function

End Module
