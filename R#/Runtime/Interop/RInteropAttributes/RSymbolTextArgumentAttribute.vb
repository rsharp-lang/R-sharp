#Region "Microsoft.VisualBasic::c327d9e67f9862bb681ea4a31babf0d2, R-sharp\R#\Runtime\Interop\RInteropAttributes\RSymbolTextArgumentAttribute.vb"

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


     Code Statistics:

        Total Lines:   37
        Code Lines:    24
        Comment Lines: 8
        Blank Lines:   5
        File Size:     1.62 KB


    '     Class RSymbolTextArgumentAttribute
    ' 
    '         Function: getSymbolText
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    ''' <summary>
    ''' 表示当前的这个参数的参数值可以接受文本或者未找到符号的变量名作为参数值
    ''' </summary>
    ''' 
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RSymbolTextArgumentAttribute : Inherits RInteropAttribute

        ' 例如
        ' 接受文本参数值   func("text")
        ' 接受符号名参数值 func(text) 如果text不存在于环境中，则等价于func("text")
        ' 如果text存在于环境中，并且值为hello，则等价于func("hello")

        Public Shared Function getSymbolText(symbol As Expression, env As Environment) As String
            Select Case symbol.GetType
                Case GetType(Literal)
                    Return DirectCast(symbol, Literal).value
                Case GetType(SymbolReference)
                    Dim symbolName$ = DirectCast(symbol, SymbolReference).symbol
                    Dim var As Symbol = env.FindSymbol(symbolName)

                    If var Is Nothing Then
                        Return symbolName
                    Else
                        Return Scripting.ToString(var.value, Nothing)
                    End If
                Case Else
                    Return Scripting.ToString(symbol.Evaluate(env), Nothing)
            End Select
        End Function
    End Class
End Namespace
