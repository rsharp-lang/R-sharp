﻿#Region "Microsoft.VisualBasic::b9cf10ff5aa5855b19f29b3e7b907aee, R#\Interpreter\ExecuteEngine\Expression.vb"

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

    '   Total Lines: 72
    '    Code Lines: 34 (47.22%)
    ' Comment Lines: 30 (41.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (11.11%)
    '     File Size: 2.89 KB


    '     Class Expression
    ' 
    '         Properties: isCallable
    ' 
    '         Function: CreateExpression, ParseLines
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.Syntax
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

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
        ''' is a function?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isCallable As Boolean
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Me.GetType.ImplementInterface(Of RFunction)
            End Get
        End Property

        ''' <summary>
        ''' Evaluate the R# expression for get its runtime value result.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns>
        ''' this function should returns the value of the current expression 
        ''' evaluation if the invoke has no error, or the <see cref="Message"/>
        ''' object value if some error happends.
        ''' </returns>
        Public MustOverride Function Evaluate(envir As Environment) As Object

        ''' <summary>
        ''' Create a new expression model from the given R# code token collection.
        ''' </summary>
        ''' <param name="code"></param>
        ''' <param name="opts"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Function CreateExpression(code As IEnumerable(Of Token), opts As SyntaxBuilderOptions) As SyntaxResult
            Return code _
                .SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True) _
                .ParseExpression(opts)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ParseLines(rscript As Rscript, Optional keepsCommentLines As Boolean = False) As IEnumerable(Of Expression)
            Return rscript.GetExpressions(Program.GetRLanguageDefaultOption(rscript, keepsCommentLines:=keepsCommentLines))
        End Function
    End Class
End Namespace
