#Region "Microsoft.VisualBasic::be2d4dfd3864ff0044c3e7aae4951e2f, R#\Interpreter\Syntax\SyntaxBuilderOptions.vb"

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

    '     Class SyntaxBuilderOptions
    ' 
    '         Properties: haveSyntaxErr
    ' 
    '         Function: Clone, GetStackTrace, UsingVectorBuilder
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.SyntaxParser

    Friend Class SyntaxBuilderOptions

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

        ''' <summary>
        ''' this property will returns true if the error message string is not empty
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property haveSyntaxErr As Boolean
            Get
                Return Not [error].StringEmpty
            End Get
        End Property

        Public Function Clone() As SyntaxBuilderOptions
            Return New SyntaxBuilderOptions With {
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
