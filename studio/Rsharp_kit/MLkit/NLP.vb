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
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
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
    Public Function Tokenice(<RRawVectorArgument> text As Object,
                             Optional delimiter As String = ".?!",
                             Optional chemical_name_rule As Boolean = False,
                             Optional env As Environment = Nothing) As Object

        Return env.EvaluateFramework(Of String, Paragraph())(
            x:=text,
            eval:=Function(si)
                      Return Paragraph.Segmentation(si, delimiter, chemical_name_rule).ToArray
                  End Function)
    End Function

    <ExportAPI("split_to_sentences")>
    Public Function split_to_sentences(<RRawVectorArgument>
                                       text As Object,
                                       Optional delimiter As String = ".?!",
                                       Optional env As Environment = Nothing) As Object

        Dim dchars As Char() = delimiter.ToArray

        Return env.EvaluateFramework(Of String, String())(
            x:=text,
            eval:=Function(si)
                      Dim pars As String() = Paragraph.SplitParagraph(si).ToArray
                      Dim sentences As String() = pars _
                          .Select(Function(str) str.Split(dchars)) _
                          .IteratesALL _
                          .ToArray

                      Return sentences
                  End Function)
    End Function

    ''' <summary>
    ''' count tokens distribution
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("count")>
    Public Function counts(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Dim df As New dataframe With {.columns = New Dictionary(Of String, Array)}

        ' token
        ' total
        ' paragraph
        ' sentences
        ' source
        If x Is Nothing Then
            Return Nothing
        ElseIf TypeOf x Is Paragraph Then
            Dim tokens As Counter() = Counter.Count(x).ToArray

            df.add("token", tokens.Select(Function(a) a.token))
            df.add("nchar", tokens.Select(Function(a) a.nchar))
            df.add("total", tokens.Select(Function(a) a.total))
            df.add("paragraph", {1})
            df.add("sentences", tokens.Select(Function(a) a.sentences))
        ElseIf TypeOf x Is Paragraph() Then
            df = countMultipleParagraph(DirectCast(x, Paragraph()), df)
        ElseIf TypeOf x Is list Then
            Dim list = DirectCast(x, list).AsGeneric(Of Paragraph)(env)
            Dim tokens As New List(Of (src As String, tokens As Counter()))

            For Each par In list
                tokens.Add((par.Key, Counter.Count(par.Value).ToArray))
            Next

            Dim tokenSet = tokens _
                .Select(Function(a) a.tokens.Select(Function(i) (i, a.src))) _
                .IteratesALL _
                .GroupBy(Function(t) t.i.token.ToLower) _
                .Select(Function(ts)
                            Dim union As New Counter With {
                                .paragraph = ts.Sum(Function(t) t.i.paragraph),
                                .sentences = ts.Sum(Function(t) t.i.sentences),
                                .token = ts.Key,
                                .total = ts.Sum(Function(t) t.i.total)
                            }
                            Dim sources As String() = ts.Select(Function(i) i.src).Distinct.ToArray

                            Return (union, sources)
                        End Function) _
                .ToArray

            df.add("token", tokenSet.Select(Function(a) a.union.token))
            df.add("nchar", tokenSet.Select(Function(a) a.union.nchar))
            df.add("total", tokenSet.Select(Function(a) a.union.total))
            df.add("paragraph", tokenSet.Select(Function(a) a.union.paragraph))
            df.add("sentences", tokenSet.Select(Function(a) a.union.sentences))
            df.add("source", tokenSet.Select(Function(a) a.sources.Where(Function(si) Not si.StartsWith("NULL")).JoinBy("; ")))
        Else
            Dim list = pipeline.TryCreatePipeline(Of Paragraph)(x, env)

            If list.isError Then
                Return list.getError
            Else
                df = countMultipleParagraph(list.populates(Of Paragraph)(env), df)
            End If
        End If

        Return df
    End Function

    Private Function countMultipleParagraph(x As IEnumerable(Of Paragraph), df As dataframe) As dataframe
        Dim tokens As Counter() = Counter.Count(x).ToArray

        df.add("token", tokens.Select(Function(a) a.token))
        df.add("nchar", tokens.Select(Function(a) a.nchar))
        df.add("total", tokens.Select(Function(a) a.total))
        df.add("paragraph", tokens.Select(Function(a) a.paragraph))
        df.add("sentences", tokens.Select(Function(a) a.sentences))

        Return df
    End Function

    <ExportAPI("article")>
    Public Function CrawlerText(html As String,
                                Optional depth As Integer = 6,
                                Optional limitCount As Integer = 180,
                                Optional appendMode As Boolean = False) As Article

        Return Article.ParseText(html, depth, limitCount, appendMode)
    End Function

End Module
