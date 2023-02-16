#Region "Microsoft.VisualBasic::95060b38b05cb7970c4f2429e674e917, E:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/string/humanReadableFormatter.vb"

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

    '   Total Lines: 46
    '    Code Lines: 30
    ' Comment Lines: 11
    '   Blank Lines: 5
    '     File Size: 1.74 KB


    '     Module humanReadableFormatter
    ' 
    '         Function: size, splitParagraph, timespanStr
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text

Namespace Runtime.Internal.Invokes

    <Package("humanReadable")>
    Module humanReadableFormatter

        ''' <summary>
        ''' convert byte number into human readable size string
        ''' </summary>
        ''' <param name="bytes"></param>
        ''' <returns></returns>
        <ExportAPI("byte_size")>
        Public Function size(bytes As Double()) As String()
            Return bytes.SafeQuery.Select(AddressOf StringFormats.Lanudry).ToArray
        End Function

        <ExportAPI("time_span")>
        Public Function timespanStr(spans As TimeSpan()) As String()
            Return spans.SafeQuery.Select(AddressOf StringFormats.Lanudry).ToArray
        End Function

        ''' <summary>
        ''' split a given text data into multiple lines
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="len"></param>
        ''' <returns></returns>
        <ExportAPI("splitParagraph")>
        Public Function splitParagraph(text As String,
                                       Optional len As Integer = 80,
                                       Optional delimiters As String = ";:,.-_&*!+'~" & vbTab & " ",
                                       Optional floatChars As Integer = 6) As String()
            Return text _
                .SplitParagraph(
                    len:=len,
                    delimiters:=delimiters,
                    floatChars:=floatChars
                ) _
                .ToArray
        End Function
    End Module
End Namespace
