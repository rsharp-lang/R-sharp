#Region "Microsoft.VisualBasic::180e6152389a6722d4376ae0bf439787, R#\System\LambdaCompiler.vb"

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

    '   Total Lines: 52
    '    Code Lines: 41
    ' Comment Lines: 1
    '   Blank Lines: 10
    '     File Size: 2.41 KB


    '     Module LambdaCompiler
    ' 
    '         Function: Compile, CreateExpression
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development

    Public Module LambdaCompiler

        <Extension>
        Public Function Compile(lambda As DeclareLambdaFunction) As Expressions.LambdaExpression
            Dim parameters As Dictionary(Of String, Expressions.ParameterExpression) = lambda.parameterNames _
                .ToDictionary(Function(name) name,
                              Function(name)
                                  Return Expressions.Expression.Parameter(GetType(Double), name)
                              End Function)
            Dim body As Expressions.Expression = CreateExpression(parameters, lambda.closure)
            Dim run As Expressions.LambdaExpression = Expressions.Expression.Lambda(body, parameters.Values.ToArray)

            Return run
        End Function

        <Extension>
        Private Function CreateExpression(parameters As Dictionary(Of String, Expressions.ParameterExpression), model As Expression) As Expressions.Expression
            Select Case model.GetType
                Case GetType(Literal)
                    Dim literal As Literal = DirectCast(model, Literal)
                    Dim constVal As Object = literal.Evaluate(Nothing)
                    Dim type As Type = If(constVal Is Nothing, GetType(Object), constVal.GetType)

                    Return Expressions.Expression.Constant(constVal, type)
                Case GetType(SymbolReference)
                    Dim ref As SymbolReference = model

                    If parameters.ContainsKey(ref.symbol) Then
                        Return parameters(ref.symbol)
                    Else
                        ' 引用的是环境中的其他变量

                    End If
                Case GetType(BinaryExpression)
                    Dim bin As BinaryExpression = DirectCast(model, BinaryExpression)
                Case Else

            End Select

            Throw New NotImplementedException(model.GetType().FullName)
        End Function
    End Module
End Namespace
