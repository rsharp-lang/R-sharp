#Region "Microsoft.VisualBasic::523de70547fe6306028670947666e58f, Library\graphics\grDevices.vb"

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

    '   Total Lines: 941
    '    Code Lines: 463 (49.20%)
    ' Comment Lines: 405 (43.04%)
    '    - Xml Docs: 80.49%
    ' 
    '   Blank Lines: 73 (7.76%)
    '     File Size: 49.24 KB


    ' Module grDevices
    ' 
    '     Function: adjustAlpha, colorPopulator, colors, display, getFormatFromSuffix
    '               imageAttrs, openSvgDevice, pdfDevice, postscriptDev, registerCustomPalette
    '               requireSvgData, rgb, saveBitmap, saveImage, saveSvgFile
    '               saveSvgStream, svg, tryMeasureFormat
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
Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Scaler
Imports Microsoft.VisualBasic.Imaging.Driver
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
    ''' ### PostScript Graphics
    ''' 
    ''' ``postscript`` starts the graphics device driver for producing PostScript graphics.
    ''' </summary>
    ''' <param name="file">	
    ''' a character string giving the file path. If it is "", the output is piped to the command given 
    ''' by the argument command. If it is of the form "|cmd", the output is piped to the command given
    ''' by cmd.
    ''' 
    ''' For use with onefile = FALSE give a printf format such as "Rplot%03d.ps" (the default in that case). 
    ''' The string should not otherwise contain a %: if it is really necessary, use %% in the string for % 
    ''' in the file name. A single integer format matching the regular expression "%[#0 +=-]*[0-9.]*[diouxX]" 
    ''' is allowed.
    ''' 
    ''' Tilde expansion (see path.expand) is done. An input with a marked encoding is converted to the 
    ''' native encoding or an error is given.</param>
    ''' <param name="onefile">logical: if true (the default) allow multiple figures in one file. If false, 
    ''' generate a file name containing the page number for each page and use an EPSF header and no 
    ''' DocumentMedia comment. Defaults to TRUE.</param>
    ''' <param name="family">the initial font family to be used, normally as a character string. See the 
    ''' section ‘Families’. Defaults to "Helvetica".</param>
    ''' <param name="title">title string to embed as the Title comment in the file. Defaults to "R Graphics Output".</param>
    ''' <param name="fonts">a character vector specifying additional R graphics font family names for font 
    ''' families whose declarations will be included in the PostScript file and are available for use with 
    ''' the device. See ‘Families’ below. Defaults to NULL.</param>
    ''' <param name="encoding">the name of an encoding file. Defaults to "default". The latter is interpreted
    ''' 
    ''' on Unix-alikes
    ''' as ‘"ISOLatin1.enc"’ unless the locale is recognized as corresponding to a language using ISO 8859-{2,5,7,13,15} or KOI8-{R,U}.
    ''' 
    ''' on Windows
    ''' as ‘"CP1250.enc"’ (Central European), "CP1251.enc" (Cyrillic), "CP1253.enc" (Greek) or "CP1257.enc" 
    ''' (Baltic) if one of those codepages is in use, otherwise ‘"WinAnsi.enc"’ (codepage 1252).
    ''' 
    ''' The file is looked for in the ‘enc’ directory of package grDevices if the path does not contain a path separator. 
    ''' An extension ".enc" can be omitted.</param>
    ''' <param name="bg">the initial background color to be used. If "transparent" (or any other non-opaque colour), 
    ''' no background is painted. Defaults to "transparent".</param>
    ''' <param name="fg">the initial foreground color to be used. Defaults to "black".</param>
    ''' <param name="width">the width and height of the graphics region in inches. Default to 0.</param>
    ''' <param name="height">the width and height of the graphics region in inches. Default to 0.
    ''' If paper != "special" and width or height is less than 0.1 or too large to give a total margin of 0.5 inch, 
    ''' the graphics region is reset to the corresponding paper dimension minus 0.5.</param>
    ''' <param name="horizontal">the orientation of the printed image, a logical. Defaults to true, 
    ''' that is landscape orientation on paper sizes with width less than height.</param>
    ''' <param name="pointsize">the default point size to be used. Strictly speaking, in bp, 
    ''' that is 1/72 of an inch, but approximately in points. Defaults to 12.</param>
    ''' <param name="paper">the size of paper in the printer. The choices are "a4", "letter" (or "us"), 
    ''' "legal" and "executive" (and these can be capitalized). Also, "special" can be used, when arguments 
    ''' width and height specify the paper size. A further choice is "default" (the default): If this is 
    ''' selected, the papersize is taken from the option "papersize" if that is set and to "a4" if it is 
    ''' unset or empty.</param>
    ''' <param name="pagecentre">logical: should the device region be centred on the page? Defaults to true.</param>
    ''' <param name="print_it">logical: should the file be printed when the device is closed? (This only applies if file is a real file name.) Defaults to false.</param>
    ''' <param name="command">the command to be used for ‘printing’. Defaults to "default", the value of option "printcmd". 
    ''' The length limit is 2*PATH_MAX, typically 8096 bytes on Unix-alikes and 520 bytes on Windows. Recent Windows 
    ''' systems may be configured to use long paths, raising this limit currently to 10000.</param>
    ''' <param name="colormodel">a character string describing the color model: currently allowed values as 
    ''' "srgb", "srgb+gray", "rgb", "rgb-nogray", "gray" (or "grey") and "cmyk". Defaults to "srgb". 
    ''' See section ‘Color models’.</param>
    ''' <param name="useKerning">logical. Should kerning corrections be included in setting text and calculating string widths? Defaults to TRUE.</param>
    ''' <param name="fillOddEven">logical controlling the polygon fill mode: see polygon for details. Default FALSE.</param>
    ''' <returns></returns>
    ''' <remarks>
    ''' All arguments except file default to values given by ps.options(). The ultimate defaults are quoted in the arguments section.
    ''' 
    ''' postscript opens the file file and the PostScript commands needed to plot any graphics requested are written to that file. 
    ''' This file can then be printed on a suitable device to obtain hard copy.
    ''' 
    ''' The file argument is interpreted as a C integer format as used by sprintf, with integer argument the page number. The default 
    ''' gives files ‘Rplot001.ps’, ..., ‘Rplot999.ps’, ‘Rplot1000.ps’, ....
    ''' 
    ''' The postscript produced for a single R plot is EPS (Encapsulated PostScript) compatible, and can be included into other documents,
    ''' e.g., into LaTeX, using \includegraphics{&lt;filename>}. For use in this way you will probably want to use setEPS() to set
    ''' the defaults as horizontal = FALSE, onefile = FALSE, paper = "special". Note that the bounding box is for the device region: 
    ''' if you find the white space around the plot region excessive, reduce the margins of the figure region via par(mar = ).
    ''' 
    ''' Most of the PostScript prologue used is taken from the R character vector .ps.prolog. This is marked in the output, and can be
    ''' changed by changing that vector. (This is only advisable for PostScript experts: the standard version is in namespace:grDevices.)
    ''' 
    ''' A PostScript device has a default family, which can be set by the user via family. If other font families are to be used when
    ''' drawing to the PostScript device, these must be declared when the device is created via fonts; the font family names for 
    ''' this argument are R graphics font family names (see the documentation for postscriptFonts).
    ''' 
    ''' Line widths as controlled by par(lwd = ) are in multiples of 1/96 inch: multiples less than 1 are allowed. pch = "." 
    ''' with cex = 1 corresponds to a square of side 1/72 inch, which is also the ‘pixel’ size assumed for graphics parameters 
    ''' such as "cra".
    ''' 
    ''' When the background colour is fully transparent (as is the initial default value), the PostScript produced does not paint
    ''' the background. Almost all PostScript viewers will use a white canvas so the visual effect is if the background were
    ''' white. This will not be the case when printing onto coloured paper, though.
    ''' 
    ''' ##### Families
    ''' 
    ''' Font families are collections of fonts covering the five font faces, (conventionally plain, bold, italic, bold-italic and
    ''' symbol) selected by the graphics parameter par(font = ) or the grid parameter gpar(fontface = ). Font families can be specified 
    ''' either as an initial/default font family for the device via the family argument or after the device is opened by the graphics
    ''' parameter par(family = ) or the grid parameter gpar(fontfamily = ). Families which will be used in addition to the initial
    ''' family must be specified in the fonts argument when the device is opened.
    ''' 
    ''' Font families are declared via a call to postscriptFonts.
    ''' 
    ''' The argument family specifies the initial/default font family to be used. In normal use it is one of "AvantGarde", "Bookman",
    ''' "Courier", "Helvetica", "Helvetica-Narrow", "NewCenturySchoolbook", "Palatino" or "Times", and refers to the standard Adobe
    ''' PostScript fonts families of those names which are included (or cloned) in all common PostScript devices.
    ''' 
    ''' Many PostScript emulators (including those based on ghostscript) use the URW equivalents of these fonts, which are "URWGothic", 
    ''' "URWBookman", "NimbusMon", "NimbusSan", "NimbusSanCond", "CenturySch", "URWPalladio" and "NimbusRom" respectively. 
    ''' If your PostScript device is using URW fonts, you will obtain access to more characters and more appropriate metrics by using 
    ''' these names. To make these easier to remember, "URWHelvetica" == "NimbusSan" and "URWTimes" == "NimbusRom" are also supported.
    ''' 
    ''' Another type of family makes use of CID-keyed fonts for East Asian languages – see postscriptFonts.
    ''' 
    ''' The family argument is normally a character string naming a font family, but family objects generated by Type1Font and CIDFont 
    ''' are also accepted. For compatibility with earlier versions of R, the initial family can also be specified as a vector of 
    ''' four or five afm files.
    ''' 
    ''' Note that R does not embed the font(s) used in the PostScript output: see embedFonts for a utility to help do so.
    ''' 
    ''' Viewers and embedding applications frequently substitute fonts for those specified in the family, and the substitute will 
    ''' often have slightly different font metrics. useKerning = TRUE spaces the letters in the string using kerning corrections 
    ''' for the intended family: this may look uglier than useKerning = FALSE.
    ''' 
    ''' ##### Encodings
    ''' 
    ''' Encodings describe which glyphs are used to display the character codes (in the range 0–255). Most commonly R uses ISOLatin1 encoding,
    ''' and the examples for text are in that encoding. However, the encoding used on machines running R may well be different, and by using
    ''' the encoding argument the glyphs can be matched to encoding in use. This suffices for European and Cyrillic languages, but not for
    ''' East Asian languages. For the latter, composite CID fonts are used. These fonts are useful for other languages: for example they 
    ''' may contain Greek glyphs. (The rest of this section applies only when CID fonts are not used.)
    ''' 
    ''' None of this will matter if only ASCII characters (codes 32–126) are used as all the encodings (except "TeXtext") agree over that range.
    ''' Some encodings are supersets of ISOLatin1, too. However, if accented and special characters do not come out as you expect, you may 
    ''' need to change the encoding. Some other encodings are supplied with R: "WinAnsi.enc" and "MacRoman.enc" correspond to the encodings 
    ''' normally used on Windows and Classic Mac OS (at least by Adobe), and "PDFDoc.enc" is the first 256 characters of the Unicode encoding,
    ''' the standard for PDF. There are also encodings "ISOLatin2.enc", "CP1250.enc", "ISOLatin7.enc" (ISO 8859-13), "CP1257.enc", and "ISOLatin9.enc" 
    ''' (ISO 8859-15), "Cyrillic.enc" (ISO 8859-5), "KOI8-R.enc", "KOI8-U.enc", "CP1251.enc", "Greek.enc" (ISO 8859-7) and "CP1253.enc". Note
    ''' that many glyphs in these encodings are not in the fonts corresponding to the standard families. (The Adobe ones for all but Courier, 
    ''' Helvetica and Times cover little more than Latin-1, whereas the URW ones also cover Latin-2, Latin-7, Latin-9 and Cyrillic but no Greek.
    ''' The Adobe exceptions cover the Latin character sets, but not the Euro.)
    ''' 
    ''' If you specify the encoding, it is your responsibility to ensure that the PostScript font contains the glyphs used. One issue here 
    ''' is the Euro symbol which is in the WinAnsi and MacRoman encodings but may well not be in the PostScript fonts. (It is in the
    ''' URW variants; it is not in the supplied Adobe Font Metric files.)
    ''' 
    ''' There is an exception. Character 45 ("-") is always set as minus (its value in Adobe ISOLatin1) even though it is hyphen in the 
    ''' other encodings. Hyphen is available as character 173 (octal 0255) in all the Latin encodings, Cyrillic and Greek. (This can 
    ''' be entered as "\uad" in a UTF-8 locale.) There are some discrepancies in accounts of glyphs 39 and 96: the supplied encodings
    ''' (except CP1250 and CP1251) treat these as ‘quoteright’ and ‘quoteleft’ (rather than ‘quotesingle’/‘acute’ and ‘grave’ respectively), 
    ''' as they are in the Adobe documentation.
    ''' 
    ''' ##### TeX fonts
    ''' 
    ''' TeX has traditionally made use of fonts such as Computer Modern which are encoded rather differently, in a 7-bit encoding. This encoding 
    ''' can be specified by encoding = "TeXtext.enc", taking care that the ASCII characters &lt; > \ _ { } are not available in those fonts.
    ''' 
    ''' There are supplied families "ComputerModern" and "ComputerModernItalic" which use this encoding, and which are only supported for
    ''' postscript (and not pdf). They are intended to use with the Type 1 versions of the TeX CM fonts. It will normally be possible to 
    ''' include such output in TeX or LaTeX provided it is processed with dvips -Ppfb -j0 or the equivalent on your system. (-j0 turns off
    ''' font subsetting.) When family = "ComputerModern" is used, the italic/bold-italic fonts used are slanted fonts (cmsl10 and cmbxsl10). 
    ''' To use text italic fonts instead, set family = "ComputerModernItalic".
    ''' 
    ''' These families use the TeX math italic and symbol fonts for a comprehensive but incomplete coverage of the glyphs covered by the Adobe
    ''' symbol font in other families. This is achieved by special-casing the postscript code generated from the supplied ‘CM_symbol_10.afm’.
    ''' 
    ''' ##### Color models
    ''' 
    ''' The default color model ("srgb") is sRGB.
    ''' 
    ''' The alternative "srgb+gray" uses sRGB for colors, but with pure gray colors (including black and white) expressed as greyscales 
    ''' (which results in smaller files and can be advantageous with some printer drivers). Conversely, its files can be rendered much
    ''' slower on some viewers, and there can be a noticeable discontinuity in color gradients involving gray or white.
    ''' 
    ''' Other possibilities are "gray" (or "grey") which used only greyscales (and converts other colours to a luminance), and "cmyk". 
    ''' The simplest possible conversion from sRGB to CMYK is used (https://en.wikipedia.org/wiki/CMYK_color_model#Mapping_RGB_to_CMYK), 
    ''' and raster images are output in RGB.
    ''' 
    ''' Color models provided for backwards compatibility are "rgb" (which is RGB+gray) and "rgb-nogray" which use uncalibrated RGB 
    ''' (as used in R prior to 2.13.0). These result in slightly smaller files which may render faster, but do rely on the viewer
    ''' being properly calibrated.
    ''' 
    ''' ##### Printing
    ''' 
    ''' A postscript plot can be printed via postscript in two ways.
    ''' 
    ''' Setting print.it = TRUE causes the command given in argument command to be called with argument "file" when the device is closed. 
    ''' Note that the plot file is not deleted unless command arranges to delete it.
    ''' 
    ''' file = "" or file = "|cmd" can be used to print using a pipe. Failure to open the command will probably be reported to the
    ''' terminal but not to R, in which case close the device by dev.off immediately.
    ''' 
    ''' On Windows the default "printcmd" is empty and will give an error if print.it = TRUE is used. Suitable commands to spool a 
    ''' PostScript file to a printer can be found in ‘RedMon’ suite available from http://pages.cs.wisc.edu/~ghost/index.html.
    ''' The command will be run in a minimized window. GSView 4.x provides ‘gsprint.exe’ which may be more convenient (it requires
    ''' Ghostscript version 6.50 or later).
    ''' 
    ''' ##### Conventions
    ''' 
    ''' This section describes the implementation of the conventions for graphics devices set out in the ‘R Internals’ manual.
    ''' 
    ''' The default device size is 7 inches square.
    ''' 
    ''' Font sizes are in big points.
    ''' 
    ''' The default font family is Helvetica.
    ''' 
    ''' Line widths are as a multiple of 1/96 inch, with a minimum of 0.01 enforced.
    ''' 
    ''' Circle of any radius are allowed.
    ''' 
    ''' Colours are by default specified as sRGB.
    ''' 
    ''' At very small line widths, the line type may be forced to solid.
    ''' 
    ''' Raster images are currently limited to opaque colours.
    ''' 
    ''' ##### Note
    ''' 
    ''' If you see problems with postscript output, do remember that the problem is much more likely to be in your viewer than in R.
    ''' Try another viewer if possible. Symptoms for which the viewer has been at fault are apparent grids on image plots (turn off 
    ''' graphics anti-aliasing in your viewer if you can) and missing or incorrect glyphs in text (viewers silently doing font 
    ''' substitution).
    ''' 
    ''' Unfortunately the default viewers on most Linux and macOS systems have these problems, and no obvious way to turn off graphics 
    ''' anti-aliasing.
    ''' </remarks>
    <ExportAPI("postscript")>
    Public Function postscriptDev(Optional file As Object = Nothing,
                                  Optional onefile As Boolean = True,
                                  Optional family As String = "Helvetica",
                                  Optional title As String = "R Graphics Output",
                                  <RRawVectorArgument>
                                  Optional fonts As Object = Nothing,
                                  Optional encoding As Object = "default",
                                  Optional bg As Object = "transparent",
                                  Optional fg As Object = "black",
                                  Optional width As Integer = 0, Optional height As Integer = 0,
                                  Optional horizontal As Boolean = True,
                                  Optional pointsize As Single = 12,
                                  Optional paper As Object = "default",
                                  Optional pagecentre As Boolean = True,
                                  Optional print_it As Boolean = False,
                                  Optional command As Object = "default",
                                  Optional colormodel As Object = "srgb",
                                  Optional useKerning As Boolean = True,
                                  Optional fillOddEven As Boolean = False) As Object

    End Function

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
                        Call DirectCast(result, Message).message.JoinBy("; ").warning
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

    <ExportAPI("palette")>
    Public Function palette(<RRawVectorArgument> x As Object, name As String, Optional env As Environment = Nothing) As Object
        Dim factors As String() = CLRVector.asCharacter(x)

        If factors.IsNullOrEmpty Then
            Return Nothing
        End If

        Dim colors As New CategoryColorProfile(factors, colorSet:=name)
        Dim colorList As String() = (From xi As String
                                     In factors
                                     Select colors _
                                         .GetColor(xi) _
                                         .ToHtmlColor).ToArray
        Return colorList
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

    ''' <summary>
    ''' Display images in ansii escape sequences
    ''' </summary>
    ''' <param name="image">
    ''' the clr image object or a file path of the image file.
    ''' 
    ''' If this parameter is a file path, then the image will be loaded
    ''' from the given file path, otherwise, if this parameter is a clr
    ''' image object, then this function will try to display it in the
    ''' ansii escape sequences.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    ''' <example>
    ''' grDevices::display("https://raw.githubusercontent.com/sciBASIC/sciBASIC.NET/master/Library/graphics/graphics.png");
    ''' grDevices::display("C:\Users\user\Pictures\image.png");
    ''' 
    ''' # display a clr image object
    ''' grDevices::display(image);
    ''' </example>
    <ExportAPI("display")>
    Public Function display(image As Object,
                            Optional width As Integer? = Nothing,
                            Optional c256_color As Boolean = False,
                            Optional env As Environment = Nothing) As Object

        If image Is Nothing Then
            Return False
        End If
        If width Is Nothing OrElse width <= 0 Then
            width = Console.WindowWidth - 1
        End If

        If TypeOf image Is String Then
            Dim filePath As String = NetFile.GetMapPath(DirectCast(image, String))

            If Not filePath.FileExists Then
                Return RInternal.debug.stop(New FileNotFoundException(filePath), env)
            Else
                image = Microsoft.VisualBasic.Imaging.Image.FromFile(filePath)
            End If
        End If

        If TypeOf image Is Image Then
            If c256_color Then
                ANSI.useTrueColor = False
            End If

            Call Console.WriteLine(ANSI.GenerateImagePreview(DirectCast(image, Image), width))
        Else
            Return Message.InCompatibleType(GetType(Image), image.GetType, env)
        End If

        Return True
    End Function
End Module
