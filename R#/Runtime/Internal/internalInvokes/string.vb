#Region "Microsoft.VisualBasic::037aa23fb0b4e71fbcc9d3cab9ea0d2a, R#\Runtime\Internal\internalInvokes\string.vb"

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

        <ExportAPI("sprintf")>
        Public Function Csprintf(format As Array, <RListObjectArgument> arguments As Object, envir As Environment) As Object
            Dim sprintf As Func(Of String, Object(), String) = AddressOf CLangStringFormatProvider.sprintf
            Dim result As String() = format _
                .AsObjectEnumerator _
                .Select(Function(str)
                            Return sprintf(Scripting.ToString(str, "NULL"), arguments)
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
