#Region "Microsoft.VisualBasic::74e9ba95957c62c0604cb8893e5d1752, R#\Runtime\Internal\internalInvokes\string.vb"

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
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: replace
' 
'         Sub: pushEnvir
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Internal.Invokes

    Module stringr

        <ExportAPI("sprintf")>
        Public Function Csprintf(format As Array, arguments As Object, envir As Environment) As Object
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
        Friend Function strsplit(envir As Environment, params As Object()) As Object
            Dim text As String() = Runtime.asVector(Of String)(params(Scan0))
            Dim delimiter As String = Runtime.asVector(Of String)(params.ElementAtOrDefault(1, " ")) _
                .AsObjectEnumerator _
                .First

            If text.IsNullOrEmpty Then
                Return Nothing
            ElseIf text.Length = 1 Then
                Return Strings.Split(text(Scan0), delimiter)
            Else
                Throw New NotImplementedException
            End If
        End Function

        <ExportAPI("paste")>
        Friend Function paste(envir As Environment, params As Object()) As Object
            Dim strings As String() = Runtime.asVector(Of String)(params(Scan0))
            Dim deli As String = Runtime _
                .asVector(Of String)(params(1)) _
                .AsObjectEnumerator _
                .DefaultFirst(" ")

            Return strings.JoinBy(deli)
        End Function

        <ExportAPI("string.replace")>
        Friend Function replace(envir As Environment, params As Object()) As String()
            Dim subj As String() = Runtime.asVector(Of String)(params(Scan0))
            Dim search As String = Scripting.ToString(Runtime.getFirst(params(1)))
            Dim replaceAs As String = Scripting.ToString(Runtime.getFirst(params(2)), "")
            Dim regexp As Boolean = Runtime.asLogical(params.ElementAtOrDefault(3))(Scan0)

            If regexp Then
                Return subj.Select(Function(s) s.StringReplace(search, replaceAs)).ToArray
            Else
                Return subj.Select(Function(s) s.Replace(search, replaceAs)).ToArray
            End If
        End Function
    End Module
End Namespace
