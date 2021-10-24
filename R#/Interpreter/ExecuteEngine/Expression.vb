#Region "Microsoft.VisualBasic::631be5917fe1e6a3f042b0e6df5a1e8b, R#\Interpreter\ExecuteEngine\Expression.vb"

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

    '     Class Expression
    ' 
    '         Function: CreateExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ''' <summary>
    ''' An expression object model in R# language interpreter
    ''' </summary>
    Public MustInherit Class Expression

        ''' <summary>
        ''' 推断出的这个表达式可能产生的值的类型
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property type As TypeCodes

        ''' <summary>
        ''' debug used...
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property expressionName As ExpressionTypes

        ''' <summary>
        ''' Evaluate the R# expression for get its runtime value result.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public MustOverride Function Evaluate(envir As Environment) As Object

        ''' <summary>
        ''' Create a new expression model from the given R# code token collection.
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Friend Shared Function CreateExpression(code As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            Return code _
                .SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True) _
                .ParseExpression(opts)
        End Function

        Public Shared Function ParseLines(rscript As Rscript, Optional keepsCommentLines As Boolean = False) As IEnumerable(Of Expression)
            Return rscript.GetExpressions(New SyntaxBuilderOptions With {.keepsCommentLines = keepsCommentLines})
        End Function
    End Class
End Namespace
