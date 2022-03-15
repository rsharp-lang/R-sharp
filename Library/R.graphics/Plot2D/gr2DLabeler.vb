#Region "Microsoft.VisualBasic::57819aabf4a5df57e2a2c03c57ad9740, R-sharp\Library\R.graphics\Plot2D\gr2DLabeler.vb"

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


     Code Statistics:

        Total Lines:   98
        Code Lines:    72
        Comment Lines: 17
        Blank Lines:   9
        File Size:     3.95 KB


    ' Module gr2DLabeler
    ' 
    '     Function: height, labeler, setAnchors, setLabels, start
    '               width
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("graphics2D.labeler", Category:=APICategories.SoftwareTools)>
Module gr2DLabeler

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="maxMove"></param>
    ''' <param name="maxAngle"></param>
    ''' <param name="w_len">leader line length </param>
    ''' <param name="w_inter">leader line intersection</param>
    ''' <param name="w_lab2">label-label overlap</param>
    ''' <param name="w_lab_anc">label-anchor overlap</param>
    ''' <param name="w_orient">orientation bias</param>
    ''' <returns></returns>
    <ExportAPI("d3js.labeler")>
    Public Function labeler(Optional maxMove# = 5,
                            Optional maxAngle# = 0.5,
                            Optional w_len# = 0.2,
                            Optional w_inter# = 1.0,
                            Optional w_lab2# = 30.0,
                            Optional w_lab_anc# = 30.0,
                            Optional w_orient# = 3.0) As Labeler

        Return d3js.labeler(
            maxMove:=maxMove,
            maxAngle:=maxAngle,
            w_inter:=w_inter,
            w_lab2:=w_lab2,
            w_lab_anc:=w_lab_anc,
            w_len:=w_len,
            w_orient:=w_orient
        )
        'Call d3js.labeler(maxMove:=20) _
        '    .Labels(labels) _
        '    .Anchors(labels.GetLabelAnchors(ptSize)) _
        '    .Width(plotRegion.Width) _
        '    .Height(plotRegion.Height) _
        '    .Start(showProgress:=True, nsweeps:=1000)
    End Function

    <ExportAPI("width")>
    Public Function width(labeler As Labeler, w As Double) As Labeler
        Return labeler.Width(w)
    End Function

    <ExportAPI("height")>
    Public Function height(labeler As Labeler, h As Double) As Labeler
        Return labeler.Height(h)
    End Function

    <ExportAPI("labels")>
    Public Function setLabels(labeler As Labeler, labels As Label()) As Labeler
        Return labeler.Labels(labels)
    End Function

    <ExportAPI("anchors")>
    <RApiReturn(GetType(Labeler))>
    Public Function setAnchors(labeler As Labeler, <RRawVectorArgument> anchor As Object, Optional env As Environment = Nothing) As Object
        If anchor Is Nothing Then
            Return Internal.debug.stop("anchor object can not be nothing!", env)
        ElseIf TypeOf anchor Is Anchor() Then
            Return labeler.Anchors(DirectCast(anchor, Anchor()))
        ElseIf RType.TypeOf(anchor).mode = TypeCodes.integer Then
            Return labeler.Anchors(labeler.GetLabelAnchors(CSng(REnv.asVector(Of Long)(anchor).GetValue(Scan0))))
        ElseIf RType.TypeOf(anchor).mode = TypeCodes.double Then
            Return labeler.Anchors(labeler.GetLabelAnchors(CSng(REnv.asVector(Of Double)(anchor).GetValue(Scan0))))
        Else
            Return Internal.debug.stop(Message.InCompatibleType(GetType(Anchor()), anchor.GetType, env), env)
        End If
    End Function

    <ExportAPI("start")>
    Public Function start(labeler As Labeler,
                          Optional nsweeps% = 2000,
                          Optional T# = 1,
                          Optional initialT# = 1,
                          Optional rotate# = 0.5,
                          Optional showProgress As Boolean = True) As Labeler

        Return labeler _
            .Temperature(T, initialT) _
            .RotateChance(rotate) _
            .Start(
                nsweeps:=nsweeps,
                showProgress:=showProgress
            )
    End Function
End Module
