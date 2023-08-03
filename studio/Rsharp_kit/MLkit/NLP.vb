#Region "Microsoft.VisualBasic::ac1c81cb1fc55a81a638b53439a06544, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/MLkit//NLP.vb"

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

'   Total Lines: 26
'    Code Lines: 18
' Comment Lines: 3
'   Blank Lines: 5
'     File Size: 836 B


' Module NLP
' 
'     Function: CrawlerText, Tokenice
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.NLP
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Interop

''' <summary>
''' NLP tools
''' </summary>
<Package("NLP")>
Module NLP

    ''' <summary>
    ''' split the given text into multiple parts
    ''' </summary>
    ''' <param name="text"></param>
    ''' <returns></returns>
    <ExportAPI("segmentation")>
    <RApiReturn(GetType(Paragraph))>
    Public Function Tokenice(<RRawVectorArgument> text As Object, Optional env As Environment = Nothing) As Paragraph()
        Return env.EvaluateFramework(Of String, Paragraph())(text, eval:=Function(si) Paragraph.Segmentation(si).ToArray)
    End Function

    <ExportAPI("article")>
    Public Function CrawlerText(html As String,
                                Optional depth As Integer = 6,
                                Optional limitCount As Integer = 180,
                                Optional appendMode As Boolean = False) As Article

        Return Article.ParseText(html, depth, limitCount, appendMode)
    End Function

End Module
