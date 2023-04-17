#Region "Microsoft.VisualBasic::e2d42b98a00ef77b1689e7f98e547e8c, D:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxResult.vb"

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

    '   Total Lines: 98
    '    Code Lines: 64
    ' Comment Lines: 16
    '   Blank Lines: 18
    '     File Size: 3.72 KB


    '     Class SyntaxResult
    ' 
    '         Properties: isException
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: (+3 Overloads) CreateError, ToString
    '         Operators: (+2 Overloads) Like
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.LINQ
Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

<Assembly: InternalsVisibleTo("SMRUCC.Language.CodeDom")>

Namespace Language.Syntax.SyntaxParser

    ''' <summary>
    ''' The R# expression syntax build result, the result of this 
    ''' model could be an error or resulted expression model 
    ''' object. 
    ''' </summary>
    Public Class SyntaxResult

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

        Sub New(err As SyntaxError)
            Me.error = err
            Me.stackTrace = Environment.StackTrace
        End Sub

        Private Sub New(stackTrace As String, [error] As SyntaxError)
            Me.stackTrace = stackTrace
            Me.error = [error]
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateError(err As String, opts As SyntaxBuilderOptions) As SyntaxResult
            Return CreateError(New SyntaxErrorException(err), opts)
        End Function

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
            Dim syntaxErr As SyntaxError = SyntaxError.CreateError(opts, err, from, [to])

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
        Public Shared Widening Operator CType(syntax As RExpression) As SyntaxResult
            Return New SyntaxResult(syntax)
        End Operator

        ''' <summary>
        ''' check of the <see cref="SyntaxResult.expression"/> type
        ''' </summary>
        ''' <param name="syntax"></param>
        ''' <param name="type"></param>
        ''' <returns></returns>
        Public Shared Operator Like(syntax As SyntaxResult, type As Type) As Boolean
            If syntax Is Nothing OrElse syntax.expression Is Nothing Then
                Return False
            End If

            Return syntax.expression.GetType Is type
        End Operator
    End Class
End Namespace
