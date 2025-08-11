#Region "Microsoft.VisualBasic::8e52bcfc4e0e9110e4ec4e52dc88f668, Library\graphics\graphics.vb"

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

    '   Total Lines: 740
    '    Code Lines: 443 (59.86%)
    ' Comment Lines: 215 (29.05%)
    '    - Xml Docs: 82.79%
    ' 
    '   Blank Lines: 82 (11.08%)
    '     File Size: 30.70 KB


    ' Module grDevices
    ' 
    '     Function: arrow, bitmap, colorTable, devCur, devOff
    '               drawText, getImageObject, isBase64StringOrFile, OpenNewBitmapDevice, plot
    '               png, rasterFont, rasterImage, rasterPixels, readImage
    '               resizeImage, setCurrentDev, thumbnail, wmf
    ' 
    '     Sub: Main
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
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports R_graphics.Common.Runtime
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

#If NET48 Then
Imports Microsoft.VisualBasic.Drawing

Imports Pen = System.Drawing.Pen
Imports Pens = System.Drawing.Pens
Imports Brush = System.Drawing.Brush
Imports Font = System.Drawing.Font
Imports Brushes = System.Drawing.Brushes
Imports SolidBrush = System.Drawing.SolidBrush
Imports DashStyle = System.Drawing.Drawing2D.DashStyle
Imports Image = System.Drawing.Image
Imports Bitmap = System.Drawing.Bitmap
Imports GraphicsPath = System.Drawing.Drawing2D.GraphicsPath
Imports FontStyle = System.Drawing.FontStyle
#Else
Imports Font = Microsoft.VisualBasic.Imaging.Font
Imports SolidBrush = Microsoft.VisualBasic.Imaging.SolidBrush
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports FontStyle = Microsoft.VisualBasic.Imaging.FontStyle
#End If

<Assembly: InternalsVisibleTo("ggplot")>
<Assembly: InternalsVisibleTo("graphics")>

Partial Module grDevices

    Sub Main()
        makeDataframe.addHandler(GetType(Color()), AddressOf colorTable)
    End Sub

    Private Function colorTable(raster As Color(), args As list, env As Environment) As dataframe
        Dim df As New dataframe With {
            .columns = New Dictionary(Of String, Array)
        }

        df.add("a", raster.Select(Function(c) CDbl(c.A)))
        df.add("r", raster.Select(Function(c) CDbl(c.R)))
        df.add("g", raster.Select(Function(c) CDbl(c.G)))
        df.add("b", raster.Select(Function(c) CDbl(c.B)))

        Return df
    End Function

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
            dev = R_graphics.Common.Runtime.graphics.Devices.LastOrDefault

            If dev.g Is Nothing Then
                Return RInternal.debug.stop("Error in dev.off() : cannot shut down device 1 (the null device)", env)
            Else
                Call R_graphics.Common.Runtime.graphics.PopLastDevice()
            End If
        Else
            dev = R_graphics.Common.Runtime.graphics.Devices.ElementAtOrDefault(which - 1)

            If dev.g Is Nothing Then
                Return RInternal.debug.stop($"Error in dev.off() : cannot shut down device {which} (the null device)", env)
            Else
                R_graphics.Common.Runtime.graphics.RemoveAt(which)
            End If
        End If

        Call dev.g.Flush()

        If dev.g.GetType.ImplementInterface(Of SaveGdiBitmap) Then
            Call DirectCast(dev.g, SaveGdiBitmap).Save(dev.file, dev.TryMeasureFormatEncoder)
        End If

        Call dev.file.Flush()

        If Not dev.leaveOpen Then
            Try
                Call dev.file.Close()
            Catch ex As Exception
            End Try
        End If

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
            Return RInternal.debug.stop("no active graphics device is specificed!", env)
        ElseIf dev Is Nothing Then
            R_graphics.Common.Runtime.graphics.SwapDevice(which, R_graphics.Common.Runtime.graphics.Devices.Count - 1)
        Else
            R_graphics.Common.Runtime.graphics.PushNewDevice(New graphicsDevice With {
                .args = New list With {.slots = New Dictionary(Of String, Object)},
                .file = Nothing,
                .g = dev,
                .index = R_graphics.Common.Runtime.graphics.Devices.Count,
                .dev = "dev.set"
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
            Return RInternal.debug.stop("no graphics device!", env)
        ElseIf labels.StringEmpty Then
            Return Nothing
        Else
            Dim color As New SolidBrush(graphicsPipeline.GetRawColor(col))
            Dim pos As New PointF(x, y)

            Call curDev.g.DrawString(labels, font, color, pos)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' Create a raster font object
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="size"></param>
    ''' <param name="style"></param>
    ''' <returns></returns>
    <ExportAPI("rasterFont")>
    Public Function rasterFont(name As String,
                               Optional size As Single = 12,
                               Optional style As FontStyle = FontStyle.Regular) As Font

        Return New Font(name, size, style)
    End Function

    ''' <summary>
    ''' convert the image to a collection of raster pixels
    ''' </summary>
    ''' <param name="image"></param>
    ''' <param name="env"></param>
    ''' <returns>
    ''' A dataframe object that contains the raster pixel data
    ''' </returns>
    <ExportAPI("rasterPixels")>
    <RApiReturn(GetType(dataframe))>
    Public Function rasterPixels(image As Object, Optional env As Environment = Nothing) As Object
        If image Is Nothing Then
            Return Nothing
        ElseIf TypeOf image Is Image OrElse TypeOf image Is Bitmap Then
            Return grDevices.colorTable(BitmapBuffer.FromImage(CType(image, Image)).GetPixelsAll.ToArray, New list, env)
        Else
            Return Message.InCompatibleType(GetType(Image), image.GetType, env)
        End If
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
            Return RInternal.debug.stop("no graphics device!", env)
        ElseIf size Is Nothing Then
            Call curDev.g.DrawImageUnscaled(image, x, y)
        Else
#Disable Warning
            Dim s_size As String = image.Size.Expression
            Dim sizeVec = graphicsPipeline.getSize(size, env, s_size).SizeParser
            Dim layout As New Rectangle(x, y, sizeVec.Width, sizeVec.Height)

            Call curDev.g.DrawImage(image, layout)
#Enable Warning
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

            Return argumentList.invokeGeneric(graphics, env, funcName:="plot")
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
            Dim buffer = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env)

            If buffer Like GetType(Message) Then
                Return buffer.TryCast(Of Message)
            Else
                'Call openNew(
                '    dev:=New Wmf(size, buffer.TryCast(Of Stream)),
                '    buffer:=buffer.TryCast(Of Stream),
                '    args:=args
                ')
                Throw New NotImplementedException
            End If

            Return Nothing
        ElseIf Not image.GetType.ImplementInterface(Of SaveGdiBitmap) Then
            Return Message.InCompatibleType(GetType(SaveGdiBitmap), image.GetType, env)
        Else
            Return SMRUCC.Rsharp.FileStreamWriter(
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
            Return OpenNewBitmapDevice(file, args, format, env)
        ElseIf TypeOf file Is Serialize.bitmapBuffer Then
            Dim buf As Serialize.bitmapBuffer = file

            If image.GetType.ImplementInterface(Of SaveGdiBitmap) Then
                Using ms As Stream = New MemoryStream
                    Call DirectCast(image, SaveGdiBitmap).Save(ms, format)
                    Call ms.Seek(0, SeekOrigin.Begin)

#If NET48 Then
                    buf.bitmap = System.Drawing.Image.FromStream(ms)
#Else
                    buf.bitmap = Microsoft.VisualBasic.Imaging.Image.FromStream(ms)
#End If
                End Using
            Else
#Disable Warning
                buf.bitmap = DirectCast(image, Image)
#Enable Warning
            End If

            Return buf
        Else
            Return SMRUCC.Rsharp.FileStreamWriter(
                env:=env,
                file:=file,
                write:=Sub(stream)
                           If image.GetType.ImplementInterface(Of SaveGdiBitmap) Then
                               Call DirectCast(image, SaveGdiBitmap).Save(stream, format)
                           ElseIf TypeOf image Is BitmapBuffer Then
                               Call DirectCast(image, BitmapBuffer).Save(stream)
                           Else
#If NET48 Then
                               Call DirectCast(image, Image).Save(stream, format.GetFormat)
#Else
                               Call DirectCast(image, Image).Save(stream, format)
#End If
                           End If
                       End Sub)
        End If
    End Function

    ''' <summary>
    ''' ### BMP, JPEG, PNG and TIFF graphics devices
    ''' 
    ''' Graphics devices for BMP, JPEG, PNG and TIFF format bitmap files.
    ''' </summary>
    ''' <param name="filename">the path Of the output file, up To 511 characters. The page 
    ''' number Is substituted If a C Integer format Is included In the character String, 
    ''' As In the Default, And tilde-expansion Is performed (see path.expand). (The result 
    ''' must be less than 600 characters Long. See postscript For further details.)
    ''' </param>
    ''' <param name="width">the width of the device.</param>
    ''' <param name="height">the height of the device.</param>
    ''' <param name="units">The units in which height and width are given. Can be px (pixels,
    ''' the default), in (inches), cm or mm.</param>
    ''' <param name="pointsize">the default pointsize of plotted text, interpreted as big
    ''' points (1/72 inch) at res ppi.</param>
    ''' <param name="bg">the initial background colour: can be overridden by setting par("bg").
    ''' </param>
    ''' <param name="res">The nominal resolution In ppi which will be recorded In the bitmap 
    ''' file, If a positive Integer. Also used For units other than the Default. If Not 
    ''' specified, taken As 72 ppi To Set the size Of text And line widths.</param>
    ''' <param name="family">A length-one character vector specifying the default font family. 
    ''' The default means to use the font numbers on the Windows GDI versions and "sans" on 
    ''' the cairographics versions.</param>
    ''' <param name="restoreConsole">See the ‘Details’ section of windows. For type == "windows" 
    ''' only.</param>
    ''' <param name="type">Should be plotting be done using Windows GDI or cairographics?</param>
    ''' <param name="antialias">Length-one character vector.
    '''
    ''' For allowed values And their effect on fonts with type = "windows" see windows: 
    ''' For that type if the argument Is missing the default Is taken from 
    ''' ``windows.options()$bitmap.aa.win``.
    '''
    ''' For allowed values And their effect (on fonts And lines, but Not fills) with 
    ''' type = "cairo" see svg.</param>
    ''' <returns></returns>
    <ExportAPI("png")>
    Public Function png(Optional image As Object = Nothing, Optional filename As Object = Nothing,
                        Optional width As Integer = 480, Optional height As Integer = 480,
                        Optional units As Object = "px", Optional pointsize As Single = 12,
                        Optional bg As Object = "white", Optional res As Object = Nothing,
                        Optional family As Object = "", Optional restoreConsole As Boolean = True,
                        <RRawVectorArgument(GetType(String))>
                        Optional type As Object = "windows|cairo|cairo-png",
                        Optional antialias As Boolean = True,
                        Optional symbolfamily As Object = "default",
                        <RListObjectArgument>
                        Optional args As list = Nothing,
                        Optional env As Environment = Nothing) As Object

        If Not args.hasName("size") Then
            args.add("size", {width, height})
        End If
        If filename Is Nothing AndAlso args.hasName("file") Then
            filename = args!file
        End If

        args.add("width", width)
        args.add("height", height)
        args.add("units", units)
        args.add("pointsize", pointsize)
        args.add("bg", bg)
        args.add("res", res)
        args.add("family", family)
        args.add("restoreConsole", restoreConsole)
        args.add("type", type)
        args.add("antialias", antialias)
        args.add("symbolfamily", symbolfamily)

        Return bitmap(image:=image, file:=filename,
                      format:=ImageFormats.Png,
                      args:=args,
                      env:=env
        )
    End Function

    ''' <summary>
    ''' image data is nothing
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Private Function OpenNewBitmapDevice(file As Object, args As list, format As ImageFormats, env As Environment) As Object
        ' just open a new device
        Dim size As SizeF = args.getSize(env, [default]:=New SizeF(2700, 2000))
        Dim backColor As Object = args.getValue(Of Object)({"fill", "color", "background"}, env)
        Dim fill As Color = graphicsPipeline.GetRawColor(backColor, [default]:=NameOf(Color.Transparent))
        Dim dpi As Integer = graphicsPipeline.getDpi(args.slots, env, [default]:=100)
        Dim dev_name As String = "bitmap"

        Select Case format
            Case ImageFormats.Gif : dev_name = "gif"
            Case ImageFormats.Jpeg : dev_name = "jpeg"
            Case ImageFormats.Png : dev_name = "png"
            Case ImageFormats.Tiff : dev_name = "tiff"
            Case ImageFormats.Webp : dev_name = "webp"
        End Select

        If file Is Nothing Then
            ' just open a new canvas object and returns to user?
            Dim println As Action(Of Object) = env.WriteLineHandler
            Dim verbose As Boolean = env.verboseOption

            If verbose Then
                Call println($"open a new bitmap canvas devices:")
                Call println($"  (width={size.Width}, height={size.Height})")
            End If

            Call env.AddMessage("neither image plot data nor file stream is missing, a gdi graphics object will be returns!")

            Return DriverLoad.CreateGraphicsDevice(New Size(size.Width, size.Height), fill, dpi:=dpi, driver:=Drivers.GDI)
        Else
            Dim buffer = SMRUCC.Rsharp.GetFileStream(file, FileAccess.Write, env)

            If buffer Like GetType(Message) Then
                Return buffer.TryCast(Of Message)
            Else
                Call openNew(
                    dev:=DriverLoad.CreateGraphicsDevice(New Size(size.Width, size.Height), fill, dpi, driver:=Drivers.GDI),
                    buffer:=buffer.TryCast(Of Stream),
                    args:=args,
                    [function]:=dev_name
                )
            End If

            Return Nothing
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
        ElseIf TypeOf file Is FileReference Then
            Dim p As FileReference = file
            Dim stream As Stream = p.fs.OpenFile(p.filepath, FileMode.OpenOrCreate, FileAccess.Read)

            Return Image.FromStream(stream)
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
    ''' Create thumbnail image
    ''' </summary>
    ''' <param name="image"></param>
    ''' <param name="max_width"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("thumbnail")>
    Public Function thumbnail(image As Object, Optional max_width As Integer = 250, Optional env As Environment = Nothing) As Object
        Dim bitmapVal = getImageObject(image, env)
        Dim bitmap As Bitmap

        If bitmapVal Like GetType(Message) Then
            Return bitmapVal.TryCast(Of Message)
        Else
            bitmap = bitmapVal.TryCast(Of Bitmap)
        End If

        Throw New NotImplementedException
        'Dim resize As Image = bitmap.Resize(max_width, onlyResizeIfWider:=False)

        'If resize.Height / resize.Width > 1.3 Then
        '    ' 高度远远大于宽度，则垂直居中截断图片
        '    Dim newHeight As Integer = max_width
        '    Dim offsetY = (resize.Height - max_width) / 2
        '    Dim corp As Image = resize.ImageCrop(New Rectangle(0, offsetY, resize.Width, newHeight))

        '    Return corp
        'Else
        '    Return resize
        'End If
    End Function

    Private Function getImageObject(image As Object, env As Environment) As [Variant](Of Bitmap, Message)
        If image Is Nothing Then
            Return RInternal.debug.stop("the required image data can not be nothing!", env)
        ElseIf TypeOf image Is Image Then
            Return CType(DirectCast(image, Image), Bitmap)
        ElseIf TypeOf image Is Bitmap Then
            Return DirectCast(image, Bitmap)
        Else
            Return Message.InCompatibleType(GetType(Bitmap), image.GetType, env)
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
            Return RInternal.debug.stop("the required new size factor can not be nothing!", env)
        ElseIf factor.Length > 2 Then
            factor = factor.Take(2).ToArray
        End If

        Dim bitmapVal = getImageObject(image, env)
        Dim newSize As Size
        Dim bitmap As Bitmap

        If bitmapVal Like GetType(Message) Then
            Return bitmapVal.TryCast(Of Message)
        Else
            bitmap = bitmapVal.TryCast(Of Bitmap)
        End If

        If factor.Length = 1 Then
            Dim scaleI As Double = factor(Scan0)
            Dim oldSize As Size = bitmap.Size

            ' scale image size by a fiven factor
            'newSize = New Size(oldSize.Width * scaleI, oldSize.Height * scaleI)
            'Dim resize As Image = bitmap.Resize(newSize.Width, onlyResizeIfWider:=newSize.Width > bitmap.Size.Width)
            'Return resize
            Throw New NotImplementedException
        Else
            ' resize the image to a given size
            newSize = New Size(factor(0), factor(1))

            Using g As IGraphics = DriverLoad.CreateGraphicsDevice(newSize, Color.Transparent, driver:=Drivers.GDI)
                Call g.DrawImage(bitmapVal.TryCast(Of Bitmap), 0, 0, newSize.Width, newSize.Height)
                Return DirectCast(g, GdiRasterGraphics).ImageResource
            End Using
        End If
    End Function

    ''' <summary>
    ''' ### Describe arrows to add to a line.
    ''' 
    ''' Produces a description of what arrows to add to a line. The result can be passed 
    ''' to a function that draws a line, e.g., grid.lines.
    ''' </summary>
    ''' <param name="angle">The angle of the arrow head in degrees (smaller numbers produce narrower, pointier arrows). Essentially describes the width of the arrow head.</param>
    ''' <param name="length">A unit specifying the length of the arrow head (from tip to base).</param>
    ''' <param name="ends">One of "last", "first", or "both", indicating which ends of the line to draw arrow heads.</param>
    ''' <param name="type">One of "open" or "closed" indicating whether the arrow head should be a closed triangle.</param>
    ''' <returns></returns>
    <ExportAPI("arrow")>
    <RApiReturn(GetType(Triangle))>
    Public Function arrow(Optional angle! = 30,
                          <RLazyExpression>
                          Optional length As Object = "~unit(0.25, ""inches"")",
                          Optional ends As String = "last",
                          Optional type As String = "open",
                          Optional env As Environment = Nothing) As Object

        Dim d As New Triangle With {.Angle = angle}
        Dim len As Double = CLRVector.asNumeric(length).DefaultFirst(2.5)
        Dim a As New PointF(0, len)
        Dim b As New PointF(-1, 0)
        Dim c As New PointF(1, 0)

        d.Vertex1 = a
        d.Vertex2 = b
        d.Vertex3 = c

        Return d
    End Function
End Module
