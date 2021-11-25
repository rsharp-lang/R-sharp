#Region "Microsoft.VisualBasic::bdb4cdcb98fbd51da350050e3b9a9433, R#\Interpreter\Syntax\SyntaxResult.vb"

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

'     Class SyntaxResult
' 
'         Properties: isException
' 
'         Constructor: (+3 Overloads) Sub New
'         Function: CreateExpression, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.LINQ
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.LINQ.Syntax
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

Namespace Interpreter.SyntaxParser

    Public Class SyntaxError

        Public Property upstream As String
        Public Property errorBlock As String
        Public Property downstream As String
        Public Property from As CodeSpan
        Public Property [to] As CodeSpan
        Public Property exception As Exception
        Public Property file As String

        Public Sub Print()

        End Sub

        Public Overrides Function ToString() As String
            Return MyBase.ToString()
        End Function

    End Class

    ''' <summary>
    ''' The R# expression syntax build result, the result of this 
    ''' model could be an error or resulted expression model 
    ''' object. 
    ''' </summary>
    Friend Class SyntaxResult

        Public ReadOnly [error] As SyntaxError
        Public ReadOnly expression As RExpression

        ''' <summary>
        ''' the .NET stacktrace in R# interpreter
        ''' </summary>
        Public ReadOnly stackTrace As String

        Public ReadOnly Property isException As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Not [error] Is Nothing
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(syntax As RExpression)
            Me.expression = syntax
        End Sub

        Private Sub New(stackTrace As String, [error] As SyntaxError)
            Me.stackTrace = stackTrace
            Me.error = [error]
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateError(err As Exception, opts As SyntaxBuilderOptions) As SyntaxResult
            Return CreateError(opts, err, opts.fromSpan, opts.toSpan)
        End Function

        ''' <summary>
        ''' create a syntax error
        ''' </summary>
        ''' <param name="opts"></param>
        ''' <param name="err"></param>
        ''' <param name="from">The code span of line start where the syntax error occurs</param>
        ''' <param name="to">the code span of line ends where the syntax error occurs</param>
        ''' <returns></returns>
        Public Shared Function CreateError(opts As SyntaxBuilderOptions,
                                           err As Exception,
                                           from As CodeSpan,
                                           [to] As CodeSpan) As SyntaxResult

            Dim stackTrace As String = Environment.StackTrace
            Dim syntaxErr As New SyntaxError With {
                .exception = err,
                .from = from,
                .[to] = [to],
                .file = opts.source.ToString
            }
            Dim scriptLines As String() = opts.source.script.LineTokens

            syntaxErr.upstream = scriptLines.Skip(from.line - 3).Take(3).JoinBy(vbCrLf)
            syntaxErr.downstream = scriptLines.Skip([to].line).Take(3).JoinBy(vbCrLf)
            syntaxErr.errorBlock = scriptLines.Skip(from.line).Take([to].line - from.line + 1).JoinBy(vbCrLf)

            Return New SyntaxResult(stackTrace, syntaxErr)
        End Function

        Public Overrides Function ToString() As String
            If isException Then
                Return stackTrace
            Else
                Return expression.ToString
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateExpression(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            Return RExpression.CreateExpression(tokens, opts)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(syntax As RExpression) As SyntaxResult
            Return New SyntaxResult(syntax)
        End Operator
    End Class
End Namespace
