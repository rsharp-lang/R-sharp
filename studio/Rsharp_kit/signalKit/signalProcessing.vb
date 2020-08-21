#Region "Microsoft.VisualBasic::1570a4f2d04ff007b9586e62e6fe7fa4, Library\R.math\signalProcessing.vb"

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

    ' Module signalProcessing
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: asGeneral, asMatrix, writeCDF
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Data.Signal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SignalProcessing
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("signal_kit")>
Module signalProcessing

    Sub New()

    End Sub

    <ExportAPI("as.signal")>
    Public Function asGeneral(measure As vector, signals As vector,
                              Optional title$ = "general signal",
                              <RListObjectArgument>
                              Optional meta As list = Nothing,
                              Optional env As Environment = Nothing) As GeneralSignal

        Return New GeneralSignal With {
            .description = title,
            .Measures = REnv.asVector(Of Double)(measure),
            .measureUnit = measure.unit?.ToString Or "n/a".AsDefault,
            .meta = meta.AsGeneric(Of String)(env),
            .reference = App.NextTempName,
            .Strength = REnv.asVector(Of Double)(signals)
        }
    End Function

    <ExportAPI("writeCDF")>
    <RApiReturn(GetType(Boolean))>
    Public Function writeCDF(<RRawVectorArgument> signals As Object, file As String, Optional description As String = "no description", Optional env As Environment = Nothing) As Object
        Dim signalData As pipeline = pipeline.TryCreatePipeline(Of GeneralSignal)(signals, env)

        If signalData.isError Then
            Return signalData.getError
        End If

        Return signalData.populates(Of GeneralSignal)(env).WriteCDF(file, description)
    End Function

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
