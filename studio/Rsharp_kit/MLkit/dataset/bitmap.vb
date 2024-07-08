
Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
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
    <RApiReturn(GetType(BitmapReader))>
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
    <RApiReturn(GetType(Bitmap))>
    Public Function corp_rectangle(bmp As BitmapReader,
                                   <RRawVectorArgument> pos As Object,
                                   <RRawVectorArgument> size As Object,
                                   Optional env As Environment = Nothing) As Object

        Dim loc_str = InteropArgumentHelper.getSize(pos, env, Nothing)
        Dim size_str = InteropArgumentHelper.getSize(size, env, Nothing)

        If loc_str.StringEmpty(, True) OrElse loc_str = "0,0" Then
            Return Internal.debug.stop("the required location should not be empty!", env)
        End If
        If size_str.StringEmpty(, True) OrElse size_str = "0,0" Then
            Return Internal.debug.stop("the required rectangle size for corp should not be empty!", env)
        End If

        Dim loc As Point = Casting.PointParser(loc_str)
        Dim sizeVal As Size = size_str.SizeParser
        Dim copy As New Bitmap(sizeVal.Width, sizeVal.Height)
        Dim c As Color

        For x As Integer = loc.X To loc.X + sizeVal.Width
            For y As Integer = loc.Y To loc.Y + sizeVal.Height
                c = bmp.GetPixelColor(y, x)
                copy.SetPixel(x - loc.X, y - loc.Y, c)
            Next
        Next

        Return copy
    End Function
End Module
