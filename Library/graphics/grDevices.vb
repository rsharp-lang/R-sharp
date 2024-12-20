﻿#Region "Microsoft.VisualBasic::4611a7af0f1ab228431e4bc5a52b4a23, Library\graphics\grDevices.vb"

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

    '   Total Lines: 636
    '    Code Lines: 413 (64.94%)
    ' Comment Lines: 158 (24.84%)
    '    - Xml Docs: 85.44%
    ' 
    '   Blank Lines: 65 (10.22%)
    '     File Size: 27.52 KB


    ' Module grDevices
    ' 
    '     Function: adjustAlpha, colorPopulator, colors, getFormatFromSuffix, imageAttrs
    '               openSvgDevice, pdfDevice, registerCustomPalette, requireSvgData, rgb
    '               saveBitmap, saveImage, saveSvgFile, saveSvgStream, svg
    '               tryMeasureFormat
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.SVG
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.[Interface]
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Serialize
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports any = Microsoft.VisualBasic.Scripting
Imports Bitmap = Microsoft.VisualBasic.Imaging.Bitmap
Imports bitmapBuffer = SMRUCC.Rsharp.Runtime.Serialize.bitmapBuffer
Imports Image = Microsoft.VisualBasic.Imaging.Image
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal


''' <summary>
''' The R# Graphics Devices and Support for Colours and Fonts
''' </summary>
<Package("grDevices", Category:=APICategories.UtilityTools)>
Public Module grDevices

    ''' <summary>
    ''' ### PDF Graphics Device
    ''' 
    ''' pdf starts the graphics device driver for producing PDF graphics.
    ''' </summary>
    ''' <param name="image"></param>
    ''' <param name="file">a character String giving the file path. If it Is Of the 
    ''' form "|cmd", the output Is piped To the command given by cmd. If it Is NULL, 
    ''' Then no external file Is created (effectively, no drawing occurs), but the 
    ''' device may still be queried (e.g., For size Of text).
    '''
    ''' For use with onefile = FALSE give a C integer format such as "Rplot%03d.pdf" 
    ''' (the default in that case). (See postscript for further details.)
    '''
    ''' Tilde expansion(see path.expand) Is done. An input with a marked encoding Is 
    ''' converted to the native encoding Or an error Is given.</param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("pdf")>
    Public Function pdfDevice(Optional image As Object = Nothing,
                              Optional file As Object = Nothing,
                              <RListObjectArgument>
                              Optional args As list = Nothing,
                              Optional env As Environment = Nothing) As Object

        Dim size As Size = graphicsPipeline.getSize(args!size, env, "2700,2000").SizeParser
        Dim dpi As Integer = graphicsPipeline.getDpi(args.slots, env, [default]:=100)

        If image Is Nothing Then
            ' just open a new device
            Dim buffer = GetFileStream(file, FileAccess.Write, env)
            Dim fill As Color = graphicsPipeline.GetRawColor(args!fill, [default]:="white")

            If buffer Like GetType(Message) Then
                Return buffer.TryCast(Of Message)
            Else
                Dim pdfImage = DriverLoad.CreateGraphicsDevice(size, dpi:=dpi, driver:=Drivers.PDF)

                Call pdfImage.Clear(fill)
                Call R_graphics.Common.Runtime.graphics.openNew(
                    dev:=pdfImage,
                    buffer:=buffer.TryCast(Of Stream),
                    args:=args,
                    [function]:="pdf"
                )
            End If

            Return Nothing
        Else
            Return env.FileStreamWriter(
                file, Sub(stream)
                          If TypeOf image Is Plot Then
                              Call DirectCast(image, Plot).Plot(size, dpi, Drivers.PDF).Save(stream)
                          Else
                              Call DirectCast(image, PdfImage).Save(stream)
                          End If
                      End Sub)
        End If
    End Function

    Private Function openSvgDevice(file As Object, args As list, env As Environment) As Object
        Dim size As Size = graphicsPipeline.getSize(args, env, New SizeF(2700, 2000)).ToSize
        ' just open a new device
        Dim buffer = GetFileStream(file, FileAccess.Write, env)
        Dim fill As Color = graphicsPipeline.GetRawColor(args!fill, [default]:="white")
        Dim dpi As Integer = args.getValue(Of Integer)({"res", "dpi", "ppi"}, env, [default]:=100)

        If buffer Like GetType(Message) Then
            Return buffer.TryCast(Of Message)
        Else
            Dim svgImage As IGraphics = DriverLoad.CreateGraphicsDevice(size, fill, dpi, driver:=Drivers.SVG)

            Call R_graphics.Common.Runtime.graphics.openNew(
                dev:=svgImage,
                buffer:=buffer.TryCast(Of Stream),
                args:=args,
                [function]:="svg"
            )
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' generates the error message for non svg image object
    ''' </summary>
    ''' <param name="image"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Private Function requireSvgData(image As Object, env As Environment) As Message
        Dim message_str As String = "The given type is incompatible with the required type!"

        If TypeOf image Is ImageData Then
            message_str &= "The data chart plot of the function for produce such data plot not supports the svg driver."
            message_str &= "Probably you should use the 'bitmap' or 'png' device for save this gdi+ raster image data to file."
        End If

        Return Message.InCompatibleType(GetType(SVGData), image.GetType, env, message_str)
    End Function

    ''' <summary>
    ''' due to the reason of svg is not a raster image, no needs for high dpi
    ''' </summary>
    ''' <param name="image"></param>
    ''' <param name="file"></param>
    ''' <param name="args"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Private Function saveSvgStream(image As Object, file As textBuffer, args As list, env As Environment) As Object
        If Not TypeOf image Is SVGData Then
            If image.GetType.IsInheritsFrom(GetType(Plot)) Then
                Dim arg1 = args.slots
                Dim arg2 = env.GetAcceptorArguments
                Dim size = graphicsPipeline.getSize(If(arg1.CheckSizeArgument, arg1, arg2), env, New SizeF(3300, 2700))
                Dim wh As String = $"{size.Width},{size.Height}"
                ' set default dpi to 100 for svg
                Dim dpi As Integer = graphicsPipeline.getDpi(
                    If(arg1.CheckDpiArgument, arg1, arg2), env, [default]:=100)

                file.mime = "image/svg+xml"
                file.text = DirectCast(image, Plot).Plot(wh, dpi, driver:=Drivers.SVG).AsSVG.GetSVGXml
            Else
                ' throw error from this error message helper function
                Return requireSvgData(image, env)
            End If
        Else
            file.mime = "image/svg+xml"
            file.text = DirectCast(image, SVGData).GetSVGXml
        End If

        Return file
    End Function

    Private Function saveSvgFile(image As Object, file As Object, args As list, env As Environment) As Object
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

        If Not TypeOf image Is SVGData Then
            If image.GetType.IsInheritsFrom(GetType(Plot)) Then
                Dim arg1 = args.slots
                Dim arg2 = env.GetAcceptorArguments
                Dim size = graphicsPipeline.getSize(If(arg1.CheckSizeArgument, arg1, arg2), env, New SizeF(3300, 2700))
                Dim wh As String = $"{size.Width},{size.Height}"
                Dim dpi As Integer = graphicsPipeline.getDpi(If(arg1.CheckDpiArgument, arg1, arg2), env, 100)

                Call DirectCast(image, Plot).Plot(wh, dpi, driver:=Drivers.SVG).Save(stream)
            Else
                Return requireSvgData(image, env)
            End If
        Else
            Call DirectCast(image, SVGData).Save(stream)
        End If

        Call stream.Flush()

        If is_file Then
            Call stream.Close()
            Call stream.Dispose()
        End If

        Return True
    End Function

    ''' <summary>
    ''' ## Cairographics-based SVG, PDF and PostScript Graphics Devices
    ''' 
    ''' Graphics devices for SVG, PDF and PostScript 
    ''' graphics files using the cairo graphics API.
    ''' </summary>
    ''' <param name="image"></param>
    ''' <param name="file">the name of the output file. The page number is substituted if 
    ''' a C integer format is included in the character string, as in the default. (Depending
    ''' on the platform, the result must be less than PATH_MAX characters long, and may 
    ''' be truncated if not. See postscript for further details.) Tilde expansion is 
    ''' performed where supported by the platform.</param>
    ''' <returns></returns>
    <ExportAPI("svg")>
    Public Function svg(Optional image As Object = Nothing,
                        Optional file As Object = Nothing,
                        <RListObjectArgument>
                        Optional args As list = Nothing,
                        Optional env As Environment = Nothing) As Object

        If image Is Nothing Then
            ' svg(file=...);
            Return openSvgDevice(file, args, env)
        ElseIf TypeOf file Is textBuffer Then
            Return saveSvgStream(image, file, args, env)
        Else
            ' svg(file=...) {...};
            Return saveSvgFile(image, file, args, env)
        End If
    End Function

    ''' <summary>
    ''' save the graphics plot object as image file
    ''' </summary>
    ''' <param name="graphics">a graphics plot object</param>
    ''' <param name="file">the file path for save the image file. 
    ''' (if this file path parameter is nothing, then the resulted 
    ''' image object will be flush to the standard output stream 
    ''' of R# environment.)
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("graphics")>
    Public Function saveImage(graphics As Object,
                              Optional file As Object = Nothing,
                              Optional env As Environment = Nothing) As Object

        If graphics Is Nothing Then
            Return RInternal.debug.stop("Graphics data is NULL!", env)
        ElseIf graphics.GetType Is GetType(Image) Then
            Return saveBitmap(Of Image)(graphics, file, env)
        ElseIf graphics.GetType Is GetType(Bitmap) Then
            Return saveBitmap(Of Bitmap)(graphics, file, env)
        ElseIf graphics.GetType().ImplementInterface(Of GdiRasterGraphics) Then
            Return saveBitmap(Of Image)(DirectCast(graphics, GdiRasterGraphics).ImageResource, file, env)
        ElseIf graphics.GetType.IsInheritsFrom(GetType(GraphicsData)) Then
            With DirectCast(graphics, GraphicsData)
                If file Is Nothing OrElse TypeOf file Is String AndAlso DirectCast(file, String).StringEmpty Then
                    env.globalEnvironment.stdout.WriteStream(AddressOf .Save, .content_type)
                ElseIf TypeOf file Is String Then
                    Return .Save(DirectCast(file, String))
                ElseIf TypeOf file Is Stream Then
                    Dim fs As Stream = DirectCast(file, Stream)
                    Call .Save(fs)
                    Call fs.Flush()
                    Return fs
                ElseIf TypeOf graphics Is ImageData AndAlso TypeOf file Is bitmapBuffer Then
                    DirectCast(file, bitmapBuffer).bitmap = DirectCast(graphics, ImageData).Image
                    Return file
                ElseIf TypeOf graphics Is SVGData AndAlso TypeOf file Is textBuffer Then
                    With DirectCast(file, textBuffer)
                        .text = DirectCast(graphics, SVGData).GetSVGXml
                        .mime = "image/svg+xml"
                    End With

                    Return file
                Else
                    Return Message.InCompatibleType(GetType(String), file.GetType, env)
                End If
            End With
        Else
            Return RInternal.debug.stop(New InvalidProgramException($"'{graphics.GetType.Name}' is not a graphics data object!"), env)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' save gdi+ image, file format to save will be auto detected via the file extension name suffix.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="graphics"></param>
    ''' <param name="file"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    Private Function saveBitmap(Of T As Image)(graphics As T, file As Object, env As Environment) As Object
        If file Is Nothing OrElse TypeOf file Is String AndAlso DirectCast(file, String).StringEmpty Then
            env.globalEnvironment.stdout.Write(DirectCast(graphics, Image))
        ElseIf TypeOf file Is String Then
            Call DirectCast(graphics, Image).Save(file, getFormatFromSuffix(filename:=file))
        ElseIf TypeOf file Is Stream Then
            ' save as png image by default for stream object
            ' due to the reason of we can not detected the
            ' file name suffix from a stream object
            Dim fs As Stream = DirectCast(file, Stream)
            Call DirectCast(graphics, Image).Save(fs, tryMeasureFormat(fs))
            Call fs.Flush()

            Return fs
        ElseIf TypeOf file Is bitmapBuffer Then
            DirectCast(file, bitmapBuffer).bitmap = graphics
            Return file
        Else
            Return Message.InCompatibleType(GetType(String), file.GetType, env, "invalid file for save bitmap!")
        End If

        Return Nothing
    End Function

    Private Function tryMeasureFormat(file As Stream) As ImageFormats
        If TypeOf file Is FileStream Then
            Dim fs As FileStream = DirectCast(file, FileStream)
            Dim filename As String = fs.Name
            Dim format As ImageFormats = getFormatFromSuffix(filename)

            Return format
        Else
            Return ImageFormats.Png
        End If
    End Function

    Private Function getFormatFromSuffix(filename As String) As ImageFormats
        Select Case filename.ExtensionSuffix.ToLower
            Case "jpg", "jpeg" : Return ImageFormats.Jpeg
            Case "bmp" : Return ImageFormats.Bmp
            Case "gif" : Return ImageFormats.Gif
            Case "ico" : Return ImageFormats.Icon
            Case "png" : Return ImageFormats.Png
            Case "tiff" : Return ImageFormats.Tiff
            Case "emf" : Return ImageFormats.Emf
            Case "wmf" : Return ImageFormats.Wmf
            Case Else
                Return ImageFormats.Png
        End Select
    End Function

    <ExportAPI("graphics.attrs")>
    Public Function imageAttrs(image As GraphicsData,
                               <RListObjectArgument>
                               Optional attrs As Object = Nothing,
                               Optional env As Environment = Nothing) As Object

        Dim attrVals As NamedValue(Of Object)() = RListObjectArgumentAttribute.getObjectList(attrs, env).ToArray

        Select Case image.Driver
            Case Drivers.GDI
                ' only works for jpeg
                Dim jpeg As Image = DirectCast(image, ImageData).Image
            Case Else
                Return RInternal.debug.stop(New NotImplementedException, env)
        End Select

        Return image
    End Function

    ''' <summary>
    ''' # RGB Color Specification
    ''' 
    ''' This function creates colors corresponding to the given intensities (between 0 and max) of the red, 
    ''' green and blue primaries. The colour specification refers to the standard sRGB colorspace 
    ''' (IEC standard 61966).
    ''' 
    ''' An alpha transparency value can also be specified (As an opacity, so 0 means fully transparent And 
    ''' max means opaque). If alpha Is Not specified, an opaque colour Is generated.
    ''' 
    ''' The names argument may be used To provide names For the colors.
    ''' 
    ''' The values returned by these functions can be used With a col= specification In graphics functions 
    ''' Or In par.
    ''' </summary>
    ''' <param name="red">numeric vectors with values in [0, M] where M is maxColorValue. When this is 255, 
    ''' the red, blue, green, and alpha values are coerced to integers in 0:255 and the result is computed 
    ''' most efficiently.</param>
    ''' <param name="green"></param>
    ''' <param name="blue"></param>
    ''' <param name="alpha"></param>
    ''' <param name="names">character vector. The names for the resulting vector.</param>
    ''' <param name="maxColorValue">number giving the maximum of the color values range, see above.</param>
    ''' <returns>
    ''' A character vector with elements of 7 or 9 characters, "#" followed by the red, blue, green and 
    ''' optionally alpha values in hexadecimal (after rescaling to 0 ... 255). The optional alpha values 
    ''' range from 0 (fully transparent) to 255 (opaque).
    '''
    ''' R does Not use 'premultiplied alpha’.
    ''' </returns>
    ''' <remarks>
    ''' The colors may be specified by passing a matrix or data frame as argument red, and leaving blue and 
    ''' green missing. In this case the first three columns of red are taken to be the red, green and blue 
    ''' values.
    ''' 
    ''' Semi-transparent colors (0 &lt; alpha &lt; 1) are supported only on some devices: at the time Of 
    ''' writing On the pdf, windows, quartz And X11(type = "cairo") devices And associated bitmap devices 
    ''' (jpeg, png, bmp, tiff And bitmap). They are supported by several third-party devices such As those 
    ''' In packages Cairo, cairoDevice And JavaGD. Only some Of these devices support semi-transparent 
    ''' backgrounds.
    ''' 
    ''' Most other graphics devices plot semi-transparent colors As fully transparent, usually With a 
    ''' warning When first encountered.
    ''' 
    ''' NA values are Not allowed For any Of red, blue, green Or alpha.
    ''' </remarks>
    <ExportAPI("rgb")>
    Public Function rgb(red As Integer(),
                        green As Integer(),
                        blue As Integer(),
                        Optional alpha As Integer() = Nothing,
                        Optional names As String() = Nothing,
                        Optional maxColorValue As Integer = 1,
                        Optional envir As Environment = Nothing) As vector

        If alpha.IsNullOrEmpty Then
            alpha = {255}
        ElseIf {red, green, blue}.Any(Function(bytes) bytes.IsNullOrEmpty) Then
            Return Nothing
        End If

        Dim colors As New vector With {
            .data = colorPopulator(
                red:=red,
                green:=green,
                blue:=blue,
                alpha:=alpha,
                maxColorValue:=maxColorValue
            ).ToArray
        }
        Dim result As Object = Nothing

        If Not names.IsNullOrEmpty Then
            result = colors.setNames(names, envir)
        End If

        If Program.isException(result) Then
            Return result
        Else
            Return colors
        End If
    End Function

    Private Iterator Function colorPopulator(red%(), green%(), blue%(), alpha%(), maxColorValue%) As IEnumerable(Of Color)
        Dim counts As i32 = Scan0

        For Each r As Integer In red
            For Each g As Integer In green
                For Each b As Integer In blue
                    For Each a As Integer In alpha
                        If maxColorValue > 0 AndAlso ++counts = maxColorValue Then
                            GoTo break
                        Else
                            Yield Color.FromArgb(a, r, g, b)
                        End If
                    Next
                Next
            Next
        Next
break:
        ' exit iterator loops
    End Function

    ''' <summary>
    ''' adjust color alpha value
    ''' </summary>
    ''' <param name="color"></param>
    ''' <param name="alpha">the color alpha value should be in range ``[0,1]``.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("alpha")>
    Public Function adjustAlpha(<RRawVectorArgument> color As Object, alpha As Double, Optional env As Environment = Nothing) As Object
        Return pipeline.TryCreatePipeline(Of Object)(color, env) _
            .populates(Of Object)(env) _
            .Select(Function(obj)
                        Return RColorPalette.getColor(obj).TranslateColor.Alpha(alpha * 255)
                    End Function) _
            .ToArray
    End Function

    ''' <summary>
    ''' register a custom color palette to the graphics system
    ''' </summary>
    ''' <param name="name"></param>
    ''' <param name="palette">A set of the colors or a function for produce 
    ''' the color palette from a given name.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("register.color_palette")>
    Public Function registerCustomPalette(Optional name As String = Nothing,
                                          <RRawVectorArgument>
                                          Optional palette As Object = Nothing,
                                          Optional env As Environment = Nothing) As Object
        If palette Is Nothing Then
            Return RInternal.debug.stop("the palette object should not be nothing!", env)
        End If

        If palette.GetType.ImplementInterface(Of RFunction) Then
            Dim eval As RFunction = palette
            Dim external As Designer.TryGetExternalColorPalette =
                Function(term) As Color()
                    Dim result = eval.Invoke({term}, env)

                    If TypeOf result Is Message Then
                        Call DirectCast(result, Message).message.JoinBy("; ").Warning
                        Return Nothing
                    Else
                        Dim chars As String() = CLRVector.asCharacter(result)
                        Dim rgb As Color() = chars.Select(Function(c) c.TranslateColor).ToArray

                        Return rgb
                    End If
                End Function

            Call Designer.Register(external)
        Else
            Dim colorSet As Color() = RColorPalette _
                .getColors(palette, -1, [default]:=Nothing) _
                .Select(Function(c) c.TranslateColor) _
                .ToArray

            Call Designer.Register(name, colorSet)
        End If

        Return Nothing
    End Function

    ''' <summary>
    ''' get color set
    ''' </summary>
    ''' <param name="term">the color set name, if the parameter value 
    ''' is an image data, then this function will try to extract the
    ''' theme colors from it.</param>
    ''' <param name="n">
    ''' number of colors from the given color set(apply cubic 
    ''' spline for the color sequence), negative value or 
    ''' ZERO means no cubic spline on the color sequence.
    ''' </param>
    ''' <param name="character">
    ''' function returns a color object sequence 
    ''' or html color code string vector if this parameter 
    ''' value is set to ``TRUE``
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("colors")>
    Public Function colors(<RRawVectorArgument> term As Object,
                           Optional n% = 256,
                           Optional character As Boolean = False,
                           Optional env As Environment = Nothing) As Object

        Dim list As Color()

        Static printHelp As Boolean = False

        If (Not env.globalEnvironment.Rscript.silent) AndAlso (Not printHelp) Then
            printHelp = True

            GetType(grDevices).Assembly _
                .FromAssembly _
                .DoCall(Sub(asm)
                            Call CLITools.AppSummary(
                                assem:=asm,
                                description:="Welcome to use the sciBASIC.NET Color Designer",
                                SYNOPSIS:=DesignerTerms.TermHelpInfo,
                                write:=env.globalEnvironment.stdout
                            )
                        End Sub)
        End If

        If term Is Nothing Then
            Return Nothing
        ElseIf TypeOf term Is vector Then
            term = DirectCast(term, vector).data
        End If

        If TypeOf term Is Image OrElse TypeOf term Is Bitmap Then
            list = CustomDesigns.ExtractThemeColors(CType(term, Bitmap), topN:=n) _
                .DoCall(AddressOf CustomDesigns.Order) _
                .ToArray
        ElseIf term.GetType.IsArray Then
            With DirectCast(term, Array) _
                .AsObjectEnumerator _
                .Select(Function(a) any.ToString(a)) _
                .ToArray

                If .Length = 1 Then
                    If n > 0 Then
                        list = Designer.GetColors(CStr(.GetValue(Scan0)), n)
                    Else
                        list = Designer.GetColors(CStr(.GetValue(Scan0)))
                    End If
                Else
                    If n > 0 Then
                        list = Designer.GetColors(.JoinBy(","), n)
                    Else
                        list = Designer.GetColors(.JoinBy(","))
                    End If
                End If
            End With
        ElseIf term.GetType Is GetType(String) Then
            If n > 0 Then
                list = Designer.GetColors(CStr(term), n)
            Else
                list = Designer.GetColors(CStr(term))
            End If
        Else
            Return RInternal.debug.stop(New InvalidProgramException(term.GetType.FullName), env)
        End If

        If character Then
            Return list.Select(Function(c) c.ToHtmlColor).ToArray
        Else
            Return list
        End If
    End Function
End Module
