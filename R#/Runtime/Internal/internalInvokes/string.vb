#Region "Microsoft.VisualBasic::1f4c6eb54fd150aa144a448275627955, R#\Runtime\Internal\internalInvokes\string.vb"

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

'     Module stringr
' 
'         Function: [string], Csprintf, html, json, nchar
'                   paste, regexp, replace, strsplit, xml
' 
' 
' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports VBStr = Microsoft.VisualBasic.Strings

Namespace Runtime.Internal.Invokes

    Module stringr

        <ExportAPI("html")>
        Public Function html(<RRawVectorArgument> x As Object, env As Environment) As Object
            If x Is Nothing Then
                Return Nothing
            Else
                Return htmlPrinter.GetHtml(x, env)
            End If
        End Function

        <ExportAPI("string")>
        Public Function [string](<RRawVectorArgument> x As Object, env As Environment) As Object
            If x Is Nothing Then
                Return ""
            Else
                Return printer.getStrings(x, env.globalEnvironment).ToArray
            End If
        End Function

        <ExportAPI("xml")>
        Public Function xml(<RRawVectorArgument> x As Object, env As Environment) As Object
            If x Is Nothing Then
                Return "<?xml version=""1.0"" encoding=""utf-16""?>"
            Else
                Try
                    Return XmlExtensions.GetXml(x, x.GetType)
                Catch ex As Exception
                    Return Internal.stop(ex, env)
                End Try
            End If
        End Function

        <ExportAPI("json")>
        Public Function json(<RRawVectorArgument> x As Object, env As Environment) As Object
            If x Is Nothing Then
                Return "null"
            Else
                Return JsonContract.GetObjectJson(x.GetType, x)
            End If
        End Function

        ''' <summary>
        ''' ### Count the Number of Characters (or Bytes or Width)
        ''' 
        ''' nchar takes a character vector as an argument and 
        ''' returns a vector whose elements contain the sizes 
        ''' of the corresponding elements of x.
        ''' </summary>
        ''' <param name="strs">
        ''' character vector, or a vector to be coerced to a character 
        ''' vector. Giving a factor is an error.</param>
        ''' <returns></returns>
        <ExportAPI("nchar")>
        <RApiReturn(GetType(Integer))>
        Public Function nchar(<RRawVectorArgument> strs As Object) As Object
            If strs Is Nothing Then
                Return 0
            End If

            If strs.GetType Is GetType(String) Then
                Return DirectCast(strs, String).Length
            Else
                Return Runtime.asVector(Of String)(strs) _
                    .AsObjectEnumerator(Of String) _
                    .Select(AddressOf VBStr.Len) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' Initializes a new instance of the ``RegularExpression`` class
        ''' for the specified regular expression <paramref name="pattern"/>.
        ''' </summary>
        ''' <param name="pattern">the specified regular expression</param>
        ''' <returns></returns>
        <ExportAPI("regexp")>
        Public Function regexp(pattern As String) As Object
            Return New Regex(pattern)
        End Function

        ''' <summary>
        ''' #### Use C-style String Formatting Commands
        ''' 
        ''' A wrapper for the C function sprintf, that returns a character 
        ''' vector containing a formatted combination of text and variable 
        ''' values.
        ''' </summary>
        ''' <param name="format"></param>
        ''' <param name="arguments"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("sprintf")>
        Public Function Csprintf(format As Array, <RListObjectArgument> arguments As Object, Optional envir As Environment = Nothing) As Object
            Dim sprintf As Func(Of String, Object(), String) = AddressOf CLangStringFormatProvider.sprintf
            Dim args As Array() = DirectCast(base.Rlist(arguments, envir), list).slots.Values _
                .Skip(1) _
                .Select(Function(a)
                            Return Runtime.asVector(Of Object)(a)
                        End Function) _
                .ToArray
            Dim inputTemplates As String() = format _
                .AsObjectEnumerator _
                .Select(AddressOf Scripting.ToString) _
                .ToArray

            If inputTemplates.Length = 1 Then
                Return sprintfSingle(inputTemplates(Scan0), args)
            Else
                Return New list With {
                    .slots = inputTemplates _
                        .ToDictionary(Function(key) key,
                                      Function(strformat)
                                          Return CObj(sprintfSingle(strformat, args))
                                      End Function)
                }
            End If
        End Function

        Private Function sprintfSingle(strformat$, args As Array()) As String()
            Dim result As String() = args(Scan0) _
                .AsObjectEnumerator _
                .Select(Function(o, i)
                            Dim aList As New List(Of Object) From {o}

                            For j As Integer = 1 To args.Length - 1
                                aList.Add(args(j).GetValue(i))
                            Next

                            Return sprintf(strformat, aList.ToArray)
                        End Function) _
                .ToArray

            Return result
        End Function

        <ExportAPI("strsplit")>
        Friend Function strsplit(text$(), Optional delimiter$ = " ", Optional envir As Environment = Nothing) As Object
            If text.IsNullOrEmpty Then
                Return Nothing
            ElseIf text.Length = 1 Then
                Return VBStr.Split(text(Scan0), delimiter)
            Else
                Return text.SeqIterator _
                    .ToDictionary(Function(i) $"[[{i.i + 1}]]",
                                  Function(i)
                                      Return VBStr.Split(i.value, delimiter)
                                  End Function)
            End If
        End Function

        <ExportAPI("paste")>
        Friend Function paste(strings$(), Optional deli$ = " ") As Object
            Return strings.JoinBy(deli)
        End Function

        <ExportAPI("string.replace")>
        Friend Function replace(subj$(), search$,
                                Optional replaceAs$ = "",
                                Optional regexp As Boolean = False) As Object
            If regexp Then
                Return subj.Select(Function(s) s.StringReplace(search, replaceAs)).ToArray
            Else
                Return subj.Select(Function(s) s.Replace(search, replaceAs)).ToArray
            End If
        End Function
    End Module
End Namespace
