
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' bitmap image dataset helper function
''' </summary>
''' 
<Package("bitmap")>
Module bitmap_func

    <ExportAPI("open")>
    Public Function open(<RRawVectorArgument> file As Object, Optional env As Environment = Nothing) As Object
        Dim buf = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Read, env)

        If buf Like GetType(Message) Then
            Return buf.TryCast(Of Message)
        End If

        Return New BitmapReader(buf.TryCast(Of Stream))
    End Function

    ''' <summary>
    ''' reader test for the pixels
    ''' </summary>
    ''' <param name="bmp"></param>
    ''' <param name="pos"></param>
    ''' <param name="size"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("corp_rectangle")>
    Public Function corp_rectangle(bmp As BitmapReader,
                                   <RRawVectorArgument> pos As Object,
                                   <RRawVectorArgument> size As Object,
                                   Optional env As Environment = Nothing) As Object

    End Function
End Module
