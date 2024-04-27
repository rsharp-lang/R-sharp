#Region "Microsoft.VisualBasic::23319830e9c7cd048cf9f0ae5b01f137, E:/GCModeller/src/R-sharp/R#//Language/Syntax/SyntaxBuilderOptions.vb"

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

    '   Total Lines: 110
    '    Code Lines: 80
    ' Comment Lines: 11
    '   Blank Lines: 19
    '     File Size: 3.98 KB


    '     Delegate Function
    ' 
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Class SyntaxBuilderOptions
    ' 
    '         Properties: fromSpan, haveSyntaxErr, isPythonPipelineSymbol, toSpan
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Clone, GetStackTrace, SetCurrentRange, UsingVectorBuilder
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Text.Parser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Language.Syntax.SyntaxParser

    Public Delegate Function GetLanguageScanner(buffer As CharPtr, stringInterpolateParser As Boolean) As Scanner
    Public Delegate Function ParseExpression(tokens As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult

    Public Class SyntaxBuilderOptions

        Public debug As Boolean = False
        Public source As Rscript
        Public [error] As String
        ''' <summary>
        ''' 是否保留下来所有的单行注释信息
        ''' </summary>
        Public keepsCommentLines As Boolean = False

        Public isBuildVector As Boolean
        Public currentLine As Integer
        Public annotations As New List(Of NamedValue(Of String))

        Public pipelineSymbols As String() = {"|>", ":>", "→"}

        Public ReadOnly ParseExpression As ParseExpression
        Public ReadOnly NewScanner As GetLanguageScanner

        Dim currentRange As Token()

        ''' <summary>
        ''' this property will returns true if the error message string is not empty
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property haveSyntaxErr As Boolean
            Get
                Return Not [error].StringEmpty
            End Get
        End Property

        Public ReadOnly Property fromSpan As CodeSpan
            Get
                Return currentRange.First.span
            End Get
        End Property

        Public ReadOnly Property toSpan As CodeSpan
            Get
                Return currentRange.Last.span
            End Get
        End Property

        ''' <summary>
        ''' operator of <see cref="pipelineSymbols"/> is ``.``?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isPythonPipelineSymbol As Boolean
            Get
                Return pipelineSymbols.Any(Function(t) t = ".")
            End Get
        End Property

        Public Function SetCurrentRange(range As Token()) As SyntaxBuilderOptions
            currentRange = range _
                .Where(Function(t) Not t.span Is Nothing) _
                .ToArray

            Return Me
        End Function

        Public Function Clone() As SyntaxBuilderOptions
            Return New SyntaxBuilderOptions(ParseExpression, NewScanner) With {
                .debug = debug,
                .[error] = [error],
                .isBuildVector = isBuildVector,
                .source = source
            }
        End Function

        Public Function UsingVectorBuilder(produce As Func(Of SyntaxBuilderOptions, SyntaxResult)) As SyntaxResult
            Dim newClone As SyntaxBuilderOptions = Clone()
            newClone.isBuildVector = True
            Dim result As SyntaxResult = produce(newClone)
            Return result
        End Function

        Public Const R_runtime As String = "SMRUCC/R#_runtime"

        Public Sub New(ParseExpression As ParseExpression, Scanner As GetLanguageScanner)
            Me.NewScanner = Scanner
            Me.ParseExpression = ParseExpression
        End Sub

        <DebuggerStepThrough>
        Public Function GetStackTrace(token As Token, Optional name$ = Nothing) As StackFrame
            Return New StackFrame With {
                .File = source.fileName,
                .Line = token.span.line,
                .Method = New Method With {
                    .Method = If(name, token.text),
                    .[Module] = "n/a",
                    .[Namespace] = R_runtime
                }
            }
        End Function
    End Class
End Namespace
