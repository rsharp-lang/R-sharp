#Region "Microsoft.VisualBasic::0f6553b2c38546aba30181c9edff744e, R#\Interpreter\ExecuteEngine\Expression.vb"

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
    '         Properties: expressionName
    ' 
    '         Function: CreateExpression, getTupleSymbols, getValueAssign, keywordExpressionHandler, ParseExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public MustInherit Class Expression

        ''' <summary>
        ''' 推断出的这个表达式可能产生的值的类型
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride ReadOnly Property type As TypeCodes

        Public ReadOnly Property expressionName As String
            Get
                Return MyClass.GetType.Name
            End Get
        End Property

        Public MustOverride Function Evaluate(envir As Environment) As Object

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Friend Shared Function CreateExpression(code As IEnumerable(Of Token)) As SyntaxResult
            Return code _
                .SplitByTopLevelDelimiter(TokenType.operator, includeKeyword:=True) _
                .ParseExpression
        End Function
    End Class
End Namespace
