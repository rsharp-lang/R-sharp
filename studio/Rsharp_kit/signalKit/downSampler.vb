#Region "Microsoft.VisualBasic::591889fdfb5255b02a701c4f8fd58a12, studio\Rsharp_kit\signalKit\downSampler.vb"

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

    '   Total Lines: 88
    '    Code Lines: 52 (59.09%)
    ' Comment Lines: 23 (26.14%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 13 (14.77%)
    '     File Size: 2.70 KB


    ' Module downSampler
    ' 
    '     Function: down_sampling, ltd, ltob, lttb, maxmin
    '               piplot
    ' 
    ' /********************************************************************************/

#End Region


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

