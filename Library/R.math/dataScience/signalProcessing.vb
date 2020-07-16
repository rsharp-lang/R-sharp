
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("signal_kit")>
Module signalProcessing

    Sub New()

    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="signals"></param>
    ''' <param name="signalId">
    ''' if this parameter is not empty, then it means one of the 
    ''' property data in <see cref="GeneralSignal.meta"/> will be 
    ''' used as signal id. the <see cref="GeneralSignal.reference"/>
    ''' is used as signal id by default.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("signal.matrix")>
    <RApiReturn(GetType(DataSet))>
    Public Function asMatrix(signals As Object, Optional signalId As String = Nothing, Optional env As Environment = Nothing) As Object
        Dim signalStream As pipeline = pipeline.TryCreatePipeline(Of GeneralSignal)(signals, env)

        If signalStream.isError Then
            Return signalStream.getError
        End If

        Dim allSignals As GeneralSignal() = signalStream.populates(Of GeneralSignal)(env).ToArray

        If allSignals.Select(Function(sig) sig.measureUnit).Distinct.Count > 1 Then
            Call env.AddMessage("contains multiple measure unit of the signals...", MSG_TYPES.WRN)
        End If

        Dim allMeasures As String() = allSignals _
            .Select(Function(a)
                        Return a.Measures.Select(Function(b) b.ToString)
                    End Function) _
            .IteratesALL _
            .Distinct _
            .OrderBy(AddressOf Val) _
            .ToArray
        Dim matrix As New List(Of DataSet)

        For Each signal As GeneralSignal In allSignals
            Dim raw = signal.Measures _
                .SeqIterator _
                .ToDictionary(Function(a) a.value.ToString,
                              Function(i)
                                  Return signal.Strength(i)
                              End Function)
            Dim row = allMeasures _
                .ToDictionary(Function(a) a,
                              Function(a)
                                  Return If(raw.ContainsKey(a), raw(a), 0)
                              End Function)
            Dim rowObj As New DataSet With {
                .ID = If(signalId.StringEmpty, signal.reference, signal.meta(signalId)),
                .Properties = row
            }

            Call matrix.Add(rowObj)
        Next

        Return matrix.ToArray
    End Function
End Module
