
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

        Return labeler.Start(
            nsweeps:=nsweeps,
            T:=T,
            initialT:=initialT,
            rotate:=rotate,
            showProgress:=showProgress
        )
    End Function
End Module
