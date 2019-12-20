#Region "Microsoft.VisualBasic::52c3381031c38485d6bc0f3a565dddd6, R#\Runtime\Internal\internalInvokes\string.vb"

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
'         Function: Csprintf, paste, replace, strsplit
' 
' 
' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Module stringr

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
                Return Microsoft.VisualBasic.Strings.Split(text(Scan0), delimiter)
            Else
                Return Internal.stop(New NotImplementedException, envir)
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
