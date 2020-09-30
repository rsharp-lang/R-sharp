#Region "Microsoft.VisualBasic::1ec1316e7a2311a069c94d2f38ca0d2d, Library\R.base\utils\strings.vb"

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

    ' Module strings
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: (+2 Overloads) createRObj, fromJSON, fromXML, Levenshtein, unescapeRRawstring
    '               unescapeRUnicode
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.DynamicProgramming.Levenshtein
Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports RHtml = SMRUCC.Rsharp.Runtime.Internal.htmlPrinter
Imports Rlang = Microsoft.VisualBasic.My.RlangInterop

<Package("stringr", Category:=APICategories.UtilityTools)>
Module stringr

    Sub New()
        RHtml.AttachHtmlFormatter(Of DistResult)(AddressOf ResultVisualize.HTMLVisualize)
    End Sub

    <ExportAPI("levenshtein")>
    Public Function Levenshtein(x$, y$) As DistResult
        Return LevenshteinDistance.ComputeDistance(x, y)
    End Function

    <ExportAPI("fromXML")>
    Public Function fromXML(str As String, Optional env As Environment = Nothing) As Object
        Return XmlElement.ParseXmlText(str).createRObj(env)
    End Function

    <Extension>
    Private Function createRObj(json As XmlElement, env As Environment) As Object

    End Function

    <ExportAPI("decode.R_unicode")>
    Public Function unescapeRUnicode(input As Object, Optional env As Environment = Nothing) As Object
        Return env.EvaluateFramework(Of String, String)(input, AddressOf Rlang.ProcessingRUniCode)
    End Function

    <ExportAPI("decode.R_rawstring")>
    Public Function unescapeRRawstring(input As Object, Optional encoding As Encodings = Encodings.Unicode, Optional env As Environment = Nothing) As Object
        Return env.EvaluateFramework(Of String, String)(input, Function(str) Rlang.ProcessingRRawUniCode(str, encoding))
    End Function
End Module
