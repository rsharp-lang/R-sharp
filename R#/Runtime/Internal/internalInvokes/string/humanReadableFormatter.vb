#Region "Microsoft.VisualBasic::d81fb9b3f3a91fbbfac05705837026ac, R#\Runtime\Internal\internalInvokes\string\humanReadableFormatter.vb"

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

    '   Total Lines: 69
    '    Code Lines: 45
    ' Comment Lines: 16
    '   Blank Lines: 8
    '     File Size: 2.72 KB


    '     Module humanReadableFormatter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: size, splitParagraph, timespan_string, timespanStr
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]

Namespace Runtime.Internal.Invokes

    <Package("humanReadable")>
    Module humanReadableFormatter

        Sub New()
            Call Internal.generic.add("toString", GetType(TimeSpan), AddressOf timespan_string)
            Call Internal.generic.add("toString", GetType(TimeSpan()), AddressOf timespan_string)
        End Sub

        Private Function timespan_string(spans As TimeSpan(), args As list, env As Environment) As Object
            Dim show_ms As Boolean = args.getValue({"showMs", "show_ms", "show.ms"}, env, [default]:=False)
            Dim s As String() = timespanStr(spans, show_ms)

            Return s
        End Function

        ''' <summary>
        ''' convert byte number into human readable size string
        ''' </summary>
        ''' <param name="bytes"></param>
        ''' <returns></returns>
        <ExportAPI("byte_size")>
        Public Function size(bytes As Double()) As String()
            Return bytes.SafeQuery.Select(AddressOf StringFormats.Lanudry).ToArray
        End Function

        ''' <summary>
        ''' cast timespan value to human readable string
        ''' </summary>
        ''' <param name="spans"></param>
        ''' <returns></returns>
        <ExportAPI("time_span")>
        Public Function timespanStr(spans As TimeSpan(), Optional show_ms As Boolean = True) As String()
            Return spans _
                .SafeQuery _
                .Select(Function(ti)
                            Return StringFormats.Lanudry(ti, showMs:=show_ms)
                        End Function) _
                .ToArray
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
