﻿#Region "Microsoft.VisualBasic::34d0d776643cb46d40eaefdcc7937dfe, studio\Rsharp_kit\MLkit\NLP.vb"

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

    '   Total Lines: 359
    '    Code Lines: 264 (73.54%)
    ' Comment Lines: 35 (9.75%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 60 (16.71%)
    '     File Size: 13.92 KB


    ' Module NLPtools
    ' 
    '     Function: bigram_func, bigramTable, countMultipleParagraph, counts, CrawlerText
    '               exportWordVector, getText, ldaCorpus, split_to_sentences, stemmer_normalize
    '               TF_IDF, Tokenice, word2vec
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.Data.NLP
Imports Microsoft.VisualBasic.Data.NLP.LDA
Imports Microsoft.VisualBasic.Data.NLP.Model
Imports Microsoft.VisualBasic.Data.NLP.Word2Vec
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.[Object]
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports rDataframe = SMRUCC.Rsharp.Runtime.Internal.Object.dataframe
Imports RInternal = SMRUCC.Rsharp.Runtime.Internal

''' <summary>
''' NLP tools
''' </summary>
<Package("NLP")>
Module NLPtools

    Sub Main()
        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(VectorModel), AddressOf exportWordVector)
        Call RInternal.Object.Converts.makeDataframe.addHandler(GetType(Bigram()), AddressOf bigramTable)
    End Sub

    Private Function bigramTable(bi As Bigram(), args As list, env As Environment) As rDataframe
        Dim df As New rDataframe With {.columns = New Dictionary(Of String, Array)}

        Call df.add("i", From t In bi Select t.i)
        Call df.add("j", From t In bi Select t.j)
        Call df.add("n", From t In bi Select t.count)

        Return df
    End Function

    <RGenericOverloads("as.data.frame")>
    Private Function exportWordVector(vec As VectorModel, args As list, env As Environment) As rDataframe
        Dim mat = vec.wordMap.ToArray
        Dim df As New rDataframe With {
            .rownames = mat.Select(Function(t) t.Key).ToArray,
            .columns = New Dictionary(Of String, Array)
        }
        Dim prefix As String = "V"
        Dim colnames As String()
        Dim offset As Integer

        If args.hasName("prefix") Then
            prefix = CLRVector.asCharacter(args!prefix).ElementAtOrDefault(Scan0, [default]:="dim")
        End If

        colnames = Enumerable _
            .Range(0, vec.vectorSize) _
            .Select(Function(i) $"{prefix}{i + 1}") _
            .ToArray

        For i As Integer = 0 To vec.vectorSize - 1
            offset = i
            df.add(colnames(i), mat.Select(Function(v) v.Value(offset)))
        Next

        Return df
    End Function

    Private Function getText(text As Object, env As Environment) As [Variant](Of Message, Paragraph())
        If TypeOf text Is list AndAlso DirectCast(text, list).listOf(TypeCodes.string) Then
            Dim strings As New List(Of String())

            For Each val As Object In DirectCast(text, list).data
                If Not val Is Nothing Then
                    Call strings.Add(CLRVector.asCharacter(val))
                End If
            Next

            Dim pars As Paragraph() = New Paragraph(strings.Count - 1) {}

            If strings.All(Function(s) s.Length = 1) Then
                ' needs parse as tokens
                For i As Integer = 0 To strings.Count - 1
                    pars(i) = Paragraph.Segmentation(strings(i)(Scan0))
                Next
            Else
                ' has been parsed
                For i As Integer = 0 To strings.Count - 1
                    pars(i) = New Paragraph With {.sentences = {New Sentence(strings(i))}}
                Next
            End If

            Return pars
        End If

        Dim pull As pipeline = pipeline.TryCreatePipeline(Of String)(text, env)

        If pull.isError Then
            pull = pipeline.TryCreatePipeline(Of Paragraph)(text, env)

            If pull.isError Then
                Return pull.getError
            Else
                Return pull _
                    .populates(Of Paragraph)(env) _
                    .ToArray
            End If
        Else
            Return pull.populates(Of String)(env) _
                .Select(Function(si) Paragraph.Segmentation(text:=si)) _
                .IteratesALL _
                .ToArray
        End If
    End Function

    ''' <summary>
    ''' create a corpus for LDA modelling
    ''' </summary>
    ''' <param name="text">a vector of the character string data or a collection 
    ''' of the text <see cref="Paragraph"/> data.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("ldaCorpus")>
    <RApiReturn(GetType(Corpus))>
    Public Function ldaCorpus(<RRawVectorArgument> text As Object, Optional env As Environment = Nothing) As Object
        Dim pull = getText(text, env)

        If pull Like GetType(Message) Then
            Return pull.TryCast(Of Message)
        End If

        Dim rawdata As Paragraph() = pull
        Dim corpus As New Corpus

        For Each p As Paragraph In rawdata
            For Each line As Sentence In p.sentences
                Call corpus.addDocument(document:=line.GetWords)
            Next
        Next

        Return corpus
    End Function

    ''' <summary>
    ''' word2vec embedding
    ''' </summary>
    ''' <param name="text">a vector of the character string data or a collection 
    ''' of the text <see cref="Paragraph"/> data.</param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("word2vec")>
    <RApiReturn(GetType(VectorModel))>
    Public Function word2vec(<RRawVectorArgument> text As Object,
                             Optional dims As Integer = 10,
                             Optional method As TrainMethod = TrainMethod.Skip_Gram,
                             Optional freq As Integer = 3,
                             Optional win_size As Integer = 5,
                             Optional env As Environment = Nothing) As Object

        Dim pull = getText(text, env)

        If pull Like GetType(Message) Then
            Return pull.TryCast(Of Message)
        End If

        Dim rawdata As Paragraph() = pull
        Dim wv As Word2Vec = (New Word2VecFactory()) _
            .setMethod(method) _
            .setNumOfThread(1) _
            .setFreqThresold(freq) _
            .setWindow(win_size) _
            .setVectorSize(dims) _
            .build()

        For Each p As Paragraph In rawdata
            For Each line As Sentence In p.sentences
                Call wv.readTokens(line)
            Next
        Next

        Call wv.training()

        Return wv.outputVector
    End Function

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

    <ExportAPI("bigram")>
    <RApiReturn(GetType(Bigram))>
    Public Function bigram_func(<RRawVectorArgument> text As Object, Optional env As Environment = Nothing) As Object
        Dim pull As pipeline = pipeline.TryCreatePipeline(Of Paragraph)(text, env)
        Dim data As Paragraph()

        If pull.isError Then
            pull = pipeline.TryCreatePipeline(Of String)(text, env)

            If pull.isError Then
                Return pull.getError
            End If

            data = pull.populates(Of String)(env) _
                .Select(Function(si) Paragraph.Segmentation(si)) _
                .IteratesALL _
                .ToArray
        Else
            data = pull.populates(Of Paragraph)(env).ToArray
        End If

        Return Bigram.ParseText(data) _
            .OrderByDescending(Function(t) t.count) _
            .ToArray
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

    <ExportAPI("TF_IDF")>
    Public Function TF_IDF(<RRawVectorArgument> docs As Object,
                           <RRawVectorArgument>
                           Optional stopwords As Object = Nothing,
                           Optional env As Environment = Nothing) As Object

        Dim method As New TF_IDF(CLRVector.asCharacter(docs), CLRVector.asCharacter(stopwords))

        Throw New NotImplementedException
    End Function

    <ExportAPI("stemmer_normalize")>
    Public Function stemmer_normalize(<RRawVectorArgument> x As Object, Optional env As Environment = Nothing) As Object
        Return env.EvaluateFramework(Of String, String())(x, Function(s) NLP.StemmerNormalize(s).ToArray)
    End Function

    <ExportAPI("article")>
    Public Function CrawlerText(html As String,
                                Optional depth As Integer = 6,
                                Optional limitCount As Integer = 180,
                                Optional appendMode As Boolean = False) As Article

        Return Article.ParseText(html, depth, limitCount, appendMode)
    End Function

End Module
