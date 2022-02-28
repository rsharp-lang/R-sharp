#Region "Microsoft.VisualBasic::a2207d1a6c21f1a752b0f7793741277e, R#\Runtime\Internal\internalInvokes\graphics.vb"

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

'     Module graphics
' 
'         Function: bitmap, plot, readImage, resizeImage
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Net.Http
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Assembly: InternalsVisibleTo("ggplot")>

Namespace Runtime.Internal.Invokes

    Module graphics

        ''' <summary>
        ''' ## Generic X-Y Plotting
        ''' 
        ''' Generic function for plotting of R objects. 
        ''' </summary>
        ''' <param name="[object]"></param>
        ''' <param name="args"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("plot")>
        Public Function plot(<RRawVectorArgument> [object] As Object,
                             <RRawVectorArgument, RListObjectArgument> args As Object,
                             Optional env As Environment = Nothing) As Object

            Dim argumentsVal As Object = base.Rlist(args, env)

            If Program.isException(argumentsVal) Then
                Return argumentsVal
            Else
                Return DirectCast(argumentsVal, list).invokeGeneric([object], env)
            End If
        End Function

        ''' <summary>
        ''' windows metafile device
        ''' </summary>
        ''' <param name="image"></param>
        ''' <param name="file"></param>
        ''' <param name="args"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("wmf")>
        Public Function wmf(image As Object,
                            Optional file As Object = Nothing,
                            <RListObjectArgument>
                            Optional args As list = Nothing,
                            Optional env As Environment = Nothing) As Object

            If image Is Nothing Then
                Return debug.stop("the source bitmap image can not be nothing!", env)
            ElseIf Not image.GetType.ImplementInterface(Of SaveGdiBitmap) Then
                Return Message.InCompatibleType(GetType(SaveGdiBitmap), image.GetType, env)
            Else
                Return fileStreamWriter(
                    env:=env,
                    file:=file,
                    write:=Sub(stream)
                               Call DirectCast(image, SaveGdiBitmap).Save(stream, Nothing)
                           End Sub)
            End If
        End Function

        ''' <summary>
        ''' open stream for file write
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="write"></param>
        Public Function fileStreamWriter(env As Environment, file As Object, write As Action(Of Stream)) As Object
            Dim stream As Stream
            Dim is_file As Boolean = False

            If file Is Nothing Then
                stream = Console.OpenStandardOutput
            ElseIf TypeOf file Is String Then
                stream = DirectCast(file, String).Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False)
                is_file = True
            ElseIf TypeOf file Is Stream Then
                stream = file
            Else
                Return Message.InCompatibleType(GetType(Stream), file.GetType, env)
            End If

            Call write(stream)
            Call stream.Flush()

            If is_file Then
                Call stream.Close()
                Call stream.Dispose()
            End If

            Return True
        End Function

        ''' <summary>
        ''' save image data as bitmap image file
        ''' </summary>
        ''' <param name="image"></param>
        ''' <param name="file"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("bitmap")>
        Public Function bitmap(image As Object,
                               Optional file As Object = Nothing,
                               Optional format As ImageFormats = ImageFormats.Png,
                               <RListObjectArgument>
                               Optional args As list = Nothing,
                               Optional env As Environment = Nothing) As Object

            If image Is Nothing Then
                Return debug.stop("the source bitmap image can not be nothing!", env)
            Else
                Return fileStreamWriter(
                    env:=env,
                    file:=file,
                    write:=Sub(stream)
                               If image.GetType.ImplementInterface(Of SaveGdiBitmap) Then
                                   Call DirectCast(image, SaveGdiBitmap).Save(stream, format.GetFormat)
                               Else
                                   Call DirectCast(image, Image).Save(stream, format.GetFormat)
                               End If
                           End Sub)
            End If
        End Function

        ''' <summary>
        ''' readImage: this function reads various types of images
        ''' 
        ''' Reads images of type .png, .jpeg, .jpg, .tiff
        ''' 
        ''' This function takes as input a string-path and returns the image in a matrix or array form. 
        ''' Supported types of images are .png, .jpeg, .jpg, .tiff. Extension types similar to .tiff 
        ''' such as .tif, .TIFF, .TIF are also supported
        ''' </summary>
        ''' <param name="file">a string specifying the path to the saved image</param>
        ''' <returns>the image in a matrix or array form</returns>
        <ExportAPI("readImage")>
        <RApiReturn(GetType(Image))>
        Public Function readImage(file As Object, Optional env As Environment = Nothing) As Object
            If file Is Nothing Then
                Return Nothing
            ElseIf TypeOf file Is String Then
                Return DirectCast(file, String).LoadImage(base64:=isBase64StringOrFile(DirectCast(file, String)))
            ElseIf TypeOf file Is Stream Then
                Return Image.FromStream(DirectCast(file, Stream))
            Else
                Return Message.InCompatibleType(GetType(Stream), file.GetType, env)
            End If
        End Function

        Private Function isBase64StringOrFile(str As String) As Boolean
            If str.FileExists Then
                Return False
            Else
                Return Base64Codec.IsBase64Pattern(str)
            End If
        End Function

        ''' <summary>
        ''' resize image to a new pixel size
        ''' </summary>
        ''' <param name="image"></param>
        ''' <param name="factor">
        ''' + a single number will be scale the image lager or smaller
        ''' + a numeric vector with two element will specific the new size in pixel directly
        ''' + other situation will throw a new exception
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("resizeImage")>
        Public Function resizeImage(image As Object, factor As Double(), Optional env As Environment = Nothing) As Object
            If factor.IsNullOrEmpty Then
                Return Internal.debug.stop("the required new size factor can not be nothing!", env)
            ElseIf factor.Length > 2 Then
                factor = factor.Take(2).ToArray
            End If

            Dim bitmap As Bitmap
            Dim newSize As Size

            If image Is Nothing Then
                Return Internal.debug.stop("the required image data can not be nothing!", env)
            ElseIf TypeOf image Is Image Then
                bitmap = CType(DirectCast(image, Image), Bitmap)
            ElseIf TypeOf image Is Bitmap Then
                bitmap = DirectCast(image, Bitmap)
            Else
                Return Message.InCompatibleType(GetType(Bitmap), image.GetType, env)
            End If

            If factor.Length = 1 Then
                Dim scaleI As Double = factor(Scan0)
                Dim oldSize As Size = bitmap.Size

                newSize = New Size(oldSize.Width * scaleI, oldSize.Height * scaleI)
            Else
                newSize = New Size(factor(0), factor(1))
            End If

            Dim resize As Image = bitmap.Resize(newSize)
            Return resize
        End Function
    End Module
End Namespace
