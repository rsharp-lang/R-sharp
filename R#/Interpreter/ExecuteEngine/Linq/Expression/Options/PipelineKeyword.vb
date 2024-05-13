#Region "Microsoft.VisualBasic::94a8474865f9d561d4478817117b18a5, R#\Interpreter\ExecuteEngine\Linq\Expression\Options\PipelineKeyword.vb"

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

    '   Total Lines: 44
    '    Code Lines: 22
    ' Comment Lines: 14
    '   Blank Lines: 8
    '     File Size: 1.93 KB


    '     Class PipelineKeyword
    ' 
    '         Function: FixLiteral
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime.Components
Imports any = Microsoft.VisualBasic.Scripting
Imports RExpression = SMRUCC.Rsharp.Interpreter.ExecuteEngine.Expression

Namespace Interpreter.ExecuteEngine.LINQ

    Public MustInherit Class PipelineKeyword : Inherits LinqKeywordExpression

        Protected Friend message As Message

        Public MustOverride Overloads Function Exec(result As IEnumerable(Of JavaScriptObject), context As ExecutableContext) As IEnumerable(Of JavaScriptObject)

        ''' <summary>
        ''' 将字符串常量表示转换为变量引用
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 这个是因为list或者dataframe之中的对象名称可能含有非法字符
        ''' 需要使用字符串常量来表示，这个函数将字符串常量表示转换为变量
        ''' 对象引用
        ''' </remarks>
        Protected Shared Function FixLiteral(expr As Expression) As Expression
            If TypeOf expr Is BinaryExpression Then
                Dim bin As BinaryExpression = DirectCast(expr, BinaryExpression)

                bin.left = FixLiteral(bin.left)
                bin.right = FixLiteral(bin.right)
            ElseIf TypeOf expr Is Literal Then
                expr = New SymbolReference(any.ToString(DirectCast(expr, Literal).value))
            ElseIf TypeOf expr Is RunTimeValueExpression Then
                'Dim Rexpr As RExpression

                'DirectCast(expr, FunctionInvoke).parameters = DirectCast(expr, FunctionInvoke).parameters _
                '    .Select(AddressOf FixLiteral) _
                '    .ToArray
            End If

            Return expr
        End Function
    End Class
End Namespace
