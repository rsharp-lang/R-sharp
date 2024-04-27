#Region "Microsoft.VisualBasic::4d9a19d649daf7a08054156ceba1a5e7, G:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/string/strings.vb"

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

    '   Total Lines: 317
    '    Code Lines: 168
    ' Comment Lines: 124
    '   Blank Lines: 25
    '     File Size: 14.59 KB


    '     Module strings
    ' 
    '         Function: AscW, endsWith, GetStackValue, InStr, LCaseStrings
    '                   ltrim, MidString, rtrim, startsWith, textLines
    '                   Trim, UCase
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization
Imports baseString = Microsoft.VisualBasic.Strings

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' Simulation of the <see cref="baseString"/> module
    ''' </summary>
    ''' 
    <Package("strings")>
    Module strings

        <ExportAPI("stack_str")>
        Public Function GetStackValue(<RRawVectorArgument> str As Object, left$, right$) As String()
            Return CLRVector.asCharacter(str) _
                .Select(Function(s)
                            Return s.GetStackValue(left, right)
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' Returns a string that contains a specified number of characters starting from
        ''' a specified position in a string.
        ''' </summary>
        ''' <param name="strings">Required. String expression from which characters are returned.</param>
        ''' <param name="start">
        ''' Required. Integer expression. Starting position of the characters to return.
        ''' If Start is greater than the number of characters in str, the Mid function returns
        ''' a zero-length string (""). Start is one based.
        ''' </param>
        ''' <param name="len">
        ''' Optional. Integer expression. Number of characters to return. If omitted or if
        ''' there are fewer than Length characters in the text (including the character at
        ''' position Start), all characters from the start position to the end of the string
        ''' are returned.
        ''' </param>
        ''' <returns>
        ''' A string that consists of the specified number of characters starting from the
        ''' specified position in the string.
        ''' </returns>
        <ExportAPI("mid")>
        Public Function MidString(strings As String(), start As Integer, Optional len As Integer = -1) As String()
            Return strings _
                .Select(Function(str)
                            If len = 0 Then
                                Return ""
                            ElseIf len < 0 Then
                                Return baseString.Mid(str, start)
                            Else
                                Return baseString.Mid(str, start, len)
                            End If
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' Returns a string or character converted to lowercase.
        ''' </summary>
        ''' <param name="strings">Required. Any valid String or Char expression.</param>
        ''' <returns>
        ''' Returns a string or character converted to lowercase.
        ''' </returns>
        <ExportAPI("lcase")>
        Public Function LCaseStrings(strings As String()) As String()
            Return strings.SafeQuery.Select(AddressOf baseString.LCase).ToArray
        End Function

        ''' <summary>
        ''' Returns a string or character containing the specified string converted to uppercase.
        ''' </summary>
        ''' <param name="strings">Required. Any valid String or Char expression.</param>
        ''' <returns>
        ''' Returns a string or character containing the specified string converted to uppercase.
        ''' </returns>
        <ExportAPI("ucase")>
        Public Function UCase(strings As String()) As String()
            Return strings.SafeQuery.Select(AddressOf baseString.UCase).ToArray
        End Function

        ''' <summary>
        ''' Removes all leading and trailing occurrences of a set of characters specified
        ''' in an array from the current System.String object.
        ''' </summary>
        ''' <param name="strings">a character vector</param>
        ''' <param name="characters">An array of Unicode characters to remove, or null.</param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' The string that remains after all occurrences of the characters in the trimChars
        ''' parameter are removed from the start and end of the current string. If trimChars
        ''' is null or an empty array, white-space characters are removed instead. If no
        ''' characters can be trimmed from the current instance, the method returns the current
        ''' instance unchanged.
        ''' </returns>
        <ExportAPI("trim")>
        Public Function Trim(strings$(),
                             <RRawVectorArgument>
                             Optional characters As Object = " "c,
                             Optional env As Environment = Nothing) As Object

            If strings.IsNullOrEmpty Then
                Return Nothing
            ElseIf characters Is Nothing Then
                Return Internal.debug.stop("characters component part for trimmed could not be NULL!", env)
            End If

            Dim rawChars As String() = CLRVector.asCharacter(characters)
            Dim chars As Char() = rawChars _
                .Select(Function(str)
                            Return CLangStringFormatProvider.ReplaceMetaChars(str).ToArray
                        End Function) _
                .IteratesALL _
                .Distinct _
                .ToArray

            Return strings _
                .Select(Function(str)
                            If str.StringEmpty(whitespaceAsEmpty:=False) Then
                                Return ""
                            Else
                                Return str.Trim(chars)
                            End If
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' Returns a string containing a copy of a specified string with no leading spaces
        ''' (LTrim), no trailing spaces (RTrim), Or no leading Or trailing spaces (Trim).
        ''' </summary>
        ''' <param name="strings">Required. Any valid String expression.</param>
        ''' <returns>
        ''' A string containing a copy of a specified string with no leading spaces (LTrim),
        ''' no trailing spaces (RTrim), Or no leading Or trailing spaces (Trim).</returns>
        <ExportAPI("ltrim")>
        Public Function ltrim(strings$()) As Object
            Return strings _
                .Select(Function(str) baseString.LTrim(str)) _
                .ToArray
        End Function

        <ExportAPI("rtrim")>
        Public Function rtrim(strings As String()) As Object
            Return strings _
                .Select(Function(str) baseString.RTrim(str)) _
                .ToArray
        End Function

        ''' <summary>
        ''' Returns an integer specifying the start position of the first occurrence of one
        ''' string within another.
        ''' </summary>
        ''' <param name="strings">Required. String expression being searched.</param>
        ''' <param name="substr">Required. String expression sought.</param>
        ''' <param name="ignoreCase">
        ''' Optional. Specifies the type of string comparison. If Compare is omitted, the
        ''' Option Compare setting determines the type of comparison.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' If InStr returns String1 is zero length or Nothing0 String2 is zero length or
        ''' NothingThe starting position for the search, which defaults to the first character
        ''' position. String2 is not found0 String2 is found within String1Position where
        ''' match begins
        ''' </returns>
        <ExportAPI("instr")>
        Public Function InStr(strings As String(),
                              substr As String(),
                              Optional ignoreCase As Boolean = False,
                              Optional envir As Environment = Nothing) As Object

            If strings.IsNullOrEmpty Then
                Return Nothing
            ElseIf substr.IsNullOrEmpty Then
                Return Internal.debug.stop("sub-string component part could not be NULL!", envir)
            Else
                Dim method As CompareMethod

                If Not ignoreCase Then
                    method = CompareMethod.Binary
                Else
                    method = CompareMethod.Text
                End If

                If substr.Length = 1 Then
                    substr = Replicate(substr(Scan0), strings.Length).ToArray
                End If
                If strings.Length = 1 Then
                    strings = Replicate(strings(Scan0), substr.Length).ToArray
                End If

                Return strings _
                    .Select(Function(str, i)
                                Return baseString.InStr(str, substr(i), method)
                            End Function) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' Returns an Integer value representing the character code corresponding to a character.
        ''' </summary>
        ''' <param name="x">
        ''' Required. Any valid Char or String expression. If [String] is a String expression,
        ''' only the first character of the string is used for input. If [String] is Nothing
        ''' or contains no characters, an System.ArgumentException error occurs.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' Returns an Integer value representing the character code corresponding to a character.
        ''' </returns>
        <ExportAPI("asc")>
        Public Function AscW(<RRawVectorArgument> x As Object, envir As Environment) As Object
            If x Is Nothing Then
                Return Nothing
            End If

            Dim type As Type = x.GetType

            If type Is GetType(String) Then
                Return DirectCast(x, String).Select(AddressOf baseString.AscW).ToArray
            ElseIf type Is GetType(Char) Then
                Return {baseString.AscW(DirectCast(x, Char))}
            ElseIf type Is GetType(Char()) Then
                Return DirectCast(x, Char()).Select(AddressOf baseString.AscW).ToArray
            ElseIf type Is GetType(String()) Then
                Return New list With {
                    .slots = DirectCast(x, String()) _
                        .SeqIterator _
                        .ToDictionary(Function(i) CStr(i.i + 1),
                                      Function(i)
                                          Return CObj(i.value.Select(AddressOf baseString.AscW).ToArray)
                                      End Function)
                }
            Else
                Return Internal.debug.stop(New InvalidProgramException(type.FullName), envir)
            End If
        End Function

        ''' <summary>
        ''' ### Does String Start or End With Another String?
        ''' 
        ''' Determines if entries of x start or end with string (entries of) 
        ''' prefix or suffix respectively, where strings are recycled to 
        ''' common lengths.
        ''' </summary>
        ''' <param name="x">vector of character string whose ``starts`` are considered.</param>
        ''' <param name="prefix">character vector(often of length one).</param>
        ''' <returns>
        ''' A logical vector, of “common length” of x and prefix (or suffix), i.e., 
        ''' of the longer of the two lengths unless one of them is zero when the result 
        ''' is also of zero length. A shorter input is recycled to the output length.
        ''' </returns>
        <ExportAPI("startsWith")>
        <RApiReturn(GetType(Boolean))>
        Public Function startsWith(x As String(), prefix As String, Optional fixed As Boolean = True) As Object
            If fixed Then
                Return x.SafeQuery _
                    .Select(Function(str) str.StartsWith(prefix)) _
                    .ToArray
            Else
                Dim pattern As New Regex(prefix, RegexICSng)

                Return x.SafeQuery _
                    .Select(Function(str)
                                Dim substr As String = pattern.Match(str).Value

                                If substr.StringEmpty Then
                                    Return False
                                Else
                                    Return str.StartsWith(substr)
                                End If
                            End Function) _
                    .ToArray
            End If
        End Function

        ''' <summary>
        ''' ### Does String Start or End With Another String?
        ''' 
        ''' Determines if entries of x start or end with string (entries of) 
        ''' prefix or suffix respectively, where strings are recycled to 
        ''' common lengths.
        ''' </summary>
        ''' <param name="x">vector of character string whose ``starts`` are considered.</param>
        ''' <param name="suffix">character vector(often of length one).</param>
        ''' <returns>
        ''' A logical vector, of “common length” of x and prefix (or suffix), i.e., 
        ''' of the longer of the two lengths unless one of them is zero when the result 
        ''' is also of zero length. A shorter input is recycled to the output length.
        ''' </returns>
        <ExportAPI("endsWith")>
        <RApiReturn(GetType(Boolean))>
        Public Function endsWith(x As String(), suffix As String) As Object
            Return x.SafeQuery.Select(Function(str) str.EndsWith(suffix)).ToArray
        End Function

        ''' <summary>
        ''' split text to lines
        ''' </summary>
        ''' <param name="text"></param>
        ''' <param name="trim"></param>
        ''' <param name="escape"></param>
        ''' <returns></returns>
        <ExportAPI("textlines")>
        Public Function textLines(text As String, Optional trim As Boolean = True, Optional escape As Boolean = False) As String()
            Return text.LineTokens(trim, escape)
        End Function
    End Module
End Namespace
