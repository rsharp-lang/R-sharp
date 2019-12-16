#Region "Microsoft.VisualBasic::a6a87dbf8adb4c0432febf502d2172a5, R#\Runtime\Internal\internalInvokes\strings.vb"

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

    '     Module strings
    ' 
    '         Function: AscW, InStr
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Interop
Imports BasicString = Microsoft.VisualBasic.Strings

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' Simulation of the <see cref="Microsoft.VisualBasic.Strings"/> module
    ''' </summary>
    Module strings

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
                              substr As String,
                              Optional ignoreCase As Boolean = False,
                              Optional envir As Environment = Nothing) As Object

            If strings.IsNullOrEmpty Then
                Return Nothing
            ElseIf substr Is Nothing Then
                Return Internal.stop("sub-string component part could not be NULL!", envir)
            Else
                Dim method As CompareMethod

                If ignoreCase Then
                    method = CompareMethod.Binary
                Else
                    method = CompareMethod.Text
                End If

                Return strings.Select(Function(str) BasicString.InStr(str, substr, method)).ToArray
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
                Return DirectCast(x, String).Select(AddressOf BasicString.AscW).ToArray
            ElseIf type Is GetType(Char) Then
                Return {BasicString.AscW(DirectCast(x, Char))}
            ElseIf type Is GetType(Char()) Then
                Return DirectCast(x, Char()).Select(AddressOf BasicString.AscW).ToArray
            ElseIf type Is GetType(String()) Then
                Return New list With {
                    .slots = DirectCast(x, String()) _
                        .SeqIterator _
                        .ToDictionary(Function(i) CStr(i.i + 1),
                                      Function(i)
                                          Return CObj(i.value.Select(AddressOf BasicString.AscW).ToArray)
                                      End Function)
                }
            Else
                Return Internal.stop(New InvalidProgramException(type.FullName), envir)
            End If
        End Function
    End Module
End Namespace
