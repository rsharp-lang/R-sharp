#Region "Microsoft.VisualBasic::b8ca79a4d0d52e4bd30c986d456bb1b9, R-sharp\R#\Runtime\Internal\internalInvokes\graphics\graphics.vb"

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

    '   Total Lines: 450
    '    Code Lines: 276
    ' Comment Lines: 120
    '   Blank Lines: 54
    '     File Size: 19.11 KB


    '     Module graphics
    ' 
    '         Function: bitmap, devCur, devOff, drawText, isBase64StringOrFile
    '                   plot, rasterFont, rasterImage, readImage, resizeImage
    '                   setCurrentDev, wmf
    ' 
    '         Sub: openNew
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
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

<Assembly: InternalsVisibleTo("ggplot")>
<Assembly: InternalsVisibleTo("graphics")>

Namespace Runtime.Internal.Invokes

    Module graphics

        ReadOnly devlist As New List(Of graphicsDevice)

        ''' <summary>
        ''' the current actived graphics device
        ''' </summary>
        Public ReadOnly Property curDev As graphicsDevice
            Get
                Return devlist.LastOrDefault
            End Get
        End Property

        Friend Sub openNew(dev As IGraphics, buffer As Stream, args As list)
            Dim curDev = New graphicsDevice With {
                .g = dev,
                .file = buffer,
                .args = args,
                .index = devlist.Count
            }
            devlist.Add(curDev)
        End Sub

        ''' <summary>
        ''' returns the number and name of the new active device 
        ''' (after the specified device has been shut down).
        ''' </summary>
        ''' <param name="which">An integer specifying a device number.</param>
        ''' <returns></returns>
        <ExportAPI("dev.off")>
        Public Function devOff(Optional which% = -1, Optional env As Environment = Nothing) As Object
            Dim dev As graphicsDevice

            If which < 1 Then
                dev = devlist.LastOrDefault

                If dev.g Is Nothing Then
                    Return Internal.debug.stop("Error in dev.off() : cannot shut down device 1 (the null device)", env)
                Else
                    devlist.Pop()
                End If
            Else
                dev = devlist.ElementAtOrDefault(which - 1)

                If dev.g Is Nothing Then
                    Return Internal.debug.stop($"Error in dev.off() : cannot shut down device {which} (the null device)", env)
                Else
                    devlist.RemoveAt(which)
                End If
            End If

            Call dev.g.Flush()

            If dev.g.GetType.ImplementInterface(Of SaveGdiBitmap) Then
                Call DirectCast(dev.g, SaveGdiBitmap).Save(dev.file, Nothing)
            End If

            Call dev.file.Flush()

            Return which
        End Function

        ''' <summary>
        ''' returns a length-one named integer vector giving the number and name of the 
        ''' active device, or 1, the null device, if none is active.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("dev.cur")>
        Public Function devCur() As Integer
            If curDev.g Is Nothing Then
                Return -1
            Else
                Return curDev.index
            End If
        End Function

        ''' <summary>
        ''' dev.set makes the specified device the active device. If there 
        ''' is no device with that number, it is equivalent to dev.next. 
        ''' If which = 1 it opens a new device and selects that.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("dev.set")>
        <RApiReturn(GetType(graphicsDevice))>
        Public Function setCurrentDev(Optional dev As IGraphics = Nothing,
                                      Optional which As Integer = -1,
                                      Optional env As Environment = Nothing) As Object

            If dev Is Nothing AndAlso which < 0 Then
                Return Internal.debug.stop("no active graphics device is specificed!", env)
            ElseIf dev Is Nothing Then
                devlist.Swap(which, devlist.Count - 1)
            Else
                devlist.Add(New graphicsDevice With {
                    .args = New list With {.slots = New Dictionary(Of String, Object)},
                    .file = Nothing,
                    .g = dev,
                    .index = devlist.Count
                })
            End If

            Return curDev
        End Function

        ''' <summary>
        ''' ## Add Text to a Plot
        ''' 
        ''' text draws the strings given in the vector labels at 
        ''' the coordinates given by x and y. y may be missing 
        ''' since xy.coords(x, y) is used for construction of the 
        ''' coordinates.
        ''' </summary>
        ''' <param name="x">numeric vectors of coordinates where 
        ''' the text labels should be written. If the length of 
        ''' x and y differs, the shorter one is recycled.
        ''' </param>
        ''' <param name="y">numeric vectors of coordinates where 
        ''' the text labels should be written. If the length of 
        ''' x and y differs, the shorter one is recycled.
        ''' </param>
        ''' <param name="labels"></param>
        ''' <returns></returns>
        <ExportAPI("text")>
        Public Function drawText(x As Double, y As Double, labels As String,
                                 Optional col As Object = "black",
                                 Optional font As Font = Nothing,
                                 Optional env As Environment = Nothing) As Object

            If font Is Nothing Then
                font = New Font(FontFace.MicrosoftYaHei, 12, FontStyle.Regular)
            End If

            If curDev.g Is Nothing Then
                Return Internal.debug.stop("no graphics device!", env)
            ElseIf labels.StringEmpty Then
                Return Nothing
            Else
                Dim color As New SolidBrush(graphicsPipeline.GetRawColor(col))
                Dim pos As New PointF(x, y)

                Call curDev.g.DrawString(labels, font, color, pos)
            End If

            Return Nothing
        End Function

        <ExportAPI("rasterFont")>
        Public Function rasterFont(name As String,
                                   Optional size As Single = 12,
                                   Optional style As FontStyle = FontStyle.Regular) As Font

            Return New Font(name, size, style)
        End Function

        ''' <summary>
        ''' draw a raster image on a specific position
        ''' </summary>
        ''' <param name="image"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="size"></param>
        ''' <param name="env"></param>
        <ExportAPI("rasterImage")>
        Public Function rasterImage(image As Image,
                                    x As Single,
                                    y As Single,
                                    <RRawVectorArgument>
                                    Optional size As Object = Nothing,
                                    Optional env As Environment = Nothing) As Object

            If curDev.g Is Nothing Then
                Return Internal.debug.stop("no graphics device!", env)
            ElseIf size Is Nothing Then
                Call curDev.g.DrawImageUnscaled(image, x, y)
            Else
                Dim sizeVec = graphicsPipeline.getSize(size, env, image.Size.Expression).SizeParser
                Dim layout As New Rectangle(x, y, sizeVec.Width, sizeVec.Height)

                Call curDev.g.DrawImage(image, layout)
            End If

            Return Nothing
        End Function

        ''' <summary>
        ''' ## Generic X-Y Plotting
        ''' 
        ''' Generic function for plotting of R objects. 
        ''' </summary>
        ''' <param name="graphics">
        ''' the graphics context
        ''' </param>
        ''' <param name="args">plot arguments</param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("plot")>
        Public Function plot(<RRawVectorArgument> graphics As Object,
                             <RRawVectorArgument, RListObjectArgument> args As Object,
                             Optional env As Environment = Nothing) As Object

            Dim argumentsVal As Object = base.Rlist(args, env)

            If Program.isException(argumentsVal) Then
                Return argumentsVal
            End If

            Dim argumentList As list = DirectCast(argumentsVal, list)

            If TypeOf graphics Is Image OrElse TypeOf graphics Is Bitmap Then
                If Not curDev.g Is Nothing Then
                    ' rendering on current graphics device
                    Dim image As Image = CType(graphics, Image)
                    Dim pos As New Point

                    If argumentList.hasName("x") OrElse argumentList.hasName("y") Then
                        pos = New Point With {
                            .X = argumentList.getValue("x", env, [default]:=0),
                            .Y = argumentList.getValue("y", env, [default]:=0)
                        }
                    End If

                    Call curDev.g.DrawImage(image, pos)

                    Return Nothing
                Else
                    ' just returns the image
                    Return graphics
                End If
            Else
                If Not curDev.g Is Nothing Then
                    Call argumentList.add("grDevices", curDev)
                End If

                Return argumentList.invokeGeneric(graphics, env)
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
        Public Function wmf(Optional image As Object = Nothing,
                            Optional file As Object = Nothing,
                            <RListObjectArgument>
                            Optional args As list = Nothing,
                            Optional env As Environment = Nothing) As Object

            If image Is Nothing Then
                ' just open a new device
                Dim size As Size = graphicsPipeline.getSize(args!size, env, "2700,2000").SizeParser
                Dim buffer = GetFileStream(file, FileAccess.Write, env)

                If buffer Like GetType(Message) Then
                    Return buffer.TryCast(Of Message)
                Else
                    Call openNew(
                        dev:=New Wmf(size, buffer.TryCast(Of Stream)),
                        buffer:=buffer.TryCast(Of Stream),
                        args:=args
                    )
                End If

                Return Nothing
            ElseIf Not image.GetType.ImplementInterface(Of SaveGdiBitmap) Then
                Return Message.InCompatibleType(GetType(SaveGdiBitmap), image.GetType, env)
            Else
                Return FileStreamWriter(
                    env:=env,
                    file:=file,
                    write:=Sub(stream)
                               Call DirectCast(image, SaveGdiBitmap).Save(stream, Nothing)
                           End Sub)
            End If
        End Function

        ''' <summary>
        ''' ## Graphics Device for Bitmap Files via Ghostscript
        ''' 
        ''' save image data as bitmap image file, bitmap generates 
        ''' a graphics file. dev2bitmap copies the current graphics 
        ''' device to a file in a graphics format.
        ''' </summary>
        ''' <param name="image"></param>
        ''' <param name="file">The output file name, with an appropriate extension.</param>
        ''' <param name="args">
        ''' additional arguments for the bitmap device:
        ''' 
        ''' 1. size, width, height: Dimensions of the display region.
        ''' 2. res, dpi: Resolution, in dots per inch.
        ''' </param>
        ''' <param name="env"></param>
        ''' <remarks>
        ''' This section describes the implementation of the conventions for 
        ''' graphics devices set out in the “R Internals Manual”. These devices 
        ''' follow the underlying device, so when viewed at the stated res:
        ''' 
        ''' 1. The Default device size Is 7 inches square.
        ''' 2. Font sizes are In big points.
        ''' 3. The Default font family Is (For the standard Ghostscript setup) URW Nimbus Sans.
        ''' 4. Line widths are As a multiple Of 1/96 inch, With no minimum.
        ''' 5. Circle of any radius are allowed.
        ''' 6. Colours are interpreted by the viewing/printing application.
        ''' 
        ''' </remarks>
        ''' <returns></returns>
        <ExportAPI("bitmap")>
        Public Function bitmap(Optional image As Object = Nothing,
                               Optional file As Object = Nothing,
                               Optional format As ImageFormats = ImageFormats.Png,
                               <RListObjectArgument>
                               Optional args As list = Nothing,
                               Optional env As Environment = Nothing) As Object

            If image Is Nothing Then
                ' just open a new device
                Dim size As SizeF = args.getSize(env, [default]:=New SizeF(2700, 2000))
                Dim backColor As Object = args.getValue(Of Object)({"fill", "color", "background"}, env)
                Dim fill As Color = graphicsPipeline.GetRawColor(backColor, [default]:=NameOf(Color.Transparent))
                Dim dpi As Integer = args.getValue(Of Integer)({"dpi", "res"}, env, [default]:=100)

                If file Is Nothing Then
                    ' just open a new canvas object and returns to user?
                    Dim println As Action(Of Object) = env.WriteLineHandler

                    Call println($"open a new bitmap canvas devices:")
                    Call println($"  (width={size.Width}, height={size.Height})")

                    Return New Size(size.Width, size.Height).CreateGDIDevice(filled:=fill, dpi:=$"{dpi},{dpi}")
                Else
                    Dim buffer = GetFileStream(file, FileAccess.Write, env)

                    If buffer Like GetType(Message) Then
                        Return buffer.TryCast(Of Message)
                    Else
                        Call openNew(
                            dev:=New Size(size.Width, size.Height).CreateGDIDevice(filled:=fill, dpi:=$"{dpi},{dpi}"),
                            buffer:=buffer.TryCast(Of Stream),
                            args:=args
                        )
                    End If

                    Return Nothing
                End If
            Else
                Return FileStreamWriter(
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
