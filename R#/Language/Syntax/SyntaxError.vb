#Region "Microsoft.VisualBasic::9d5002536ca1a3bbd3bfc147e7188499, D:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxError.vb"

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

    '   Total Lines: 62
    '    Code Lines: 50
    ' Comment Lines: 0
    '   Blank Lines: 12
    '     File Size: 2.59 KB


    '     Class SyntaxError
    ' 
    '         Properties: [to], downstream, errorBlock, exception, file
    '                     from, upstream
    ' 
    '         Function: (+2 Overloads) CreateError, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Language.Syntax.SyntaxParser

    Public Class SyntaxError

        Public Property upstream As String
        Public Property errorBlock As String
        Public Property downstream As String
        Public Property from As CodeSpan
        Public Property [to] As CodeSpan
        Public Property exception As Exception
        Public Property file As String

        Public Overrides Function ToString() As String
            Dim rawText As String = errorBlock
            Dim err As Exception = exception
            Dim message As String = err.ToString
            Dim nlen As Integer = rawText.LineTokens.MaxLengthString.Length
            Dim errorsPromptLine = New String("~"c, nlen)

            message &= vbCrLf & vbCrLf & "Syntax error nearby:"
            message &= vbCrLf & upstream
            message &= vbCrLf & vbCrLf & "-->>>"
            message &= vbCrLf & rawText
            message &= vbCrLf & errorsPromptLine
            message &= vbCrLf & "<<<--" & vbCrLf
            message &= vbCrLf & downstream
            message &= vbCrLf & vbCrLf & $"Range from {from.start} at line {from.line}, to {[to].stops} at line {[to].line}."
            message &= vbCrLf & "Rscript: " & file

            Return message
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateError(opts As SyntaxBuilderOptions, err As Exception) As SyntaxError
            Return CreateError(opts, err, opts.fromSpan, opts.toSpan)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Shared Function CreateError(opts As SyntaxBuilderOptions,
                                           err As String,
                                           from As CodeSpan,
                                           [to] As CodeSpan) As SyntaxError

            Return CreateError(opts, New SyntaxErrorException(err), [from], [to])
        End Function

        Friend Shared Function CreateError(opts As SyntaxBuilderOptions,
                                           err As Exception,
                                           from As CodeSpan,
                                           [to] As CodeSpan) As SyntaxError

            Dim syntaxErr As New SyntaxError With {
                .exception = err,
                .from = from,
                .[to] = [to],
                .file = opts.source.ToString
            }
            Dim scriptLines As String() = opts.source.script.LineTokens

            syntaxErr.upstream = scriptLines.Skip(from.line - 3).Take(2).JoinBy(vbCrLf)
            syntaxErr.downstream = scriptLines.Skip([to].line).Take(3).JoinBy(vbCrLf)
            syntaxErr.errorBlock = scriptLines.Skip(from.line - 1).Take([to].line - from.line + 1).JoinBy(vbCrLf)

            Return syntaxErr
        End Function

    End Class
End Namespace
