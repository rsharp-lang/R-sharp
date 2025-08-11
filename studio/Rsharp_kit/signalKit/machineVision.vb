
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Filters
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Math.MachineVision.CCL
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("machineVision")>
Public Module machineVision

    <ExportAPI("ccl")>
    <RApiReturn(GetType(Polygon2D))>
    Public Function ccl(image As Object, Optional two_pass As Boolean = False, Optional env As Environment = Nothing) As Object
        If image Is Nothing Then
            Return Nothing
        End If

        Dim buffer = bitmapCommon(image, env)

        If buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        If two_pass Then
            Return CCLabeling.TwoPassProcess(buffer).ToArray
        Else
            Return CCLabeling.Process(buffer).ToArray
        End If
    End Function

    Private Function bitmapCommon(image As Object, env As Environment) As [Variant](Of BitmapBuffer, Message)
        If TypeOf image Is Bitmap Then
            Return DirectCast(image, Bitmap).MemoryBuffer
        ElseIf TypeOf image Is Image Then
            Return New Bitmap(DirectCast(image, Image)).MemoryBuffer
        ElseIf TypeOf image Is BitmapBuffer Then
            Return DirectCast(image, BitmapBuffer)
        Else
            Return Message.InCompatibleType(GetType(BitmapBuffer), image.GetType, env)
        End If
    End Function

    <ExportAPI("ostu")>
    <RApiReturn(GetType(BitmapBuffer))>
    Public Function ostuFilter(image As Object, Optional flip As Boolean = False, Optional env As Environment = Nothing) As Object
        If image Is Nothing Then
            Return Nothing
        End If

        Dim buffer = bitmapCommon(image, env)

        If buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        End If

        Return Thresholding.ostuFilter(buffer, flip)
    End Function
End Module
