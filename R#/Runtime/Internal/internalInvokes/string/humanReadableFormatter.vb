#Region "Microsoft.VisualBasic::8ac281e257259a4b704daa6dcf732d33, R-sharp\R#\Runtime\Internal\internalInvokes\string\humanReadableFormatter.vb"

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

'   Total Lines: 13
'    Code Lines: 10
' Comment Lines: 0
'   Blank Lines: 3
'     File Size: 397.00 B


'     Module humanReadableFormatter
' 
'         Function: size
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace Runtime.Internal.Invokes

    Module humanReadableFormatter

        <ExportAPI("byte_size")>
        Public Function size(bytes As Double()) As String()
            Return bytes.SafeQuery.Select(AddressOf StringFormats.Lanudry).ToArray
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
                                       Optional delimiters As String = ";:,.-_&*!+'~") As String()

            Return text.SplitParagraph(len, delimiters:=delimiters).ToArray
        End Function
    End Module
End Namespace
