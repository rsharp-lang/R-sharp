#Region "Microsoft.VisualBasic::51267cffd620fe075ca4caa9bcf13411, R-sharp\R#\System\Document\MetaTextParser.vb"

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

    '   Total Lines: 57
    '    Code Lines: 46
    ' Comment Lines: 3
    '   Blank Lines: 8
    '     File Size: 2.16 KB


    '     Module MetaTextParser
    ' 
    '         Function: ParseTagData
    ' 
    '         Sub: ParserLoopStep
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Text

Namespace Development

    Module MetaTextParser

        <Extension>
        Public Function ParseTagData(lines As IEnumerable(Of String), Optional strict As Boolean = True) As Dictionary(Of String, String)
            Dim lastTag As String = Nothing
            Dim index As New Dictionary(Of String, String)

            For Each line As String In lines
                Call ParserLoopStep(line, lastTag, index, strict)
            Next

            For Each name As String In index.Keys.ToArray
                index(name) = index(name).Trim(ASCII.CR, ASCII.LF)
            Next

            Return index
        End Function

        Private Sub ParserLoopStep(line As String, ByRef lastTag$, index As Dictionary(Of String, String), strict As Boolean)
            Dim tag As NamedValue(Of String) = line.GetTagValue(":", trim:=True)
            Dim continuteLine As String
            Dim valueStr As String

            ' tag name string empty:         'xxxx'
            ' tag name contains whitespace:  'aaa bbb ccc ddd: xxxx yyyy'
            '
            If tag.Name.StringEmpty OrElse tag.Name.Contains(" "c) Then
                If lastTag.StringEmpty Then
                    If strict Then
                        Throw New SyntaxErrorException("invalid content format of the 'DESCRIPTION' meta data file!")
                    Else
                        lastTag = ""
                        GoTo Write
                    End If
                End If
Write:
                If index.ContainsKey(lastTag) Then
                    continuteLine = index(lastTag) & vbCrLf & line
                    index(lastTag) = continuteLine
                Else
                    valueStr = index.TryGetValue(lastTag)
                    continuteLine = valueStr & vbCrLf & line
                    index(lastTag) = continuteLine
                End If
            Else
                lastTag = tag.Name
                index(lastTag) = tag.Value
            End If
        End Sub
    End Module
End Namespace
