#Region "Microsoft.VisualBasic::4c60e0b380d666e66be2ef9c05c0d998, R#\Interpreter\ExecuteEngine\ExpressionSymbols\DataSet\FormulaExpression.vb"

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

    '   Total Lines: 112
    '    Code Lines: 63 (56.25%)
    ' Comment Lines: 32 (28.57%)
    '    - Xml Docs: 96.88%
    ' 
    '   Blank Lines: 17 (15.18%)
    '     File Size: 3.92 KB


    '     Class FormulaExpression
    ' 
    '         Properties: expressionName, formula, type, var
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, (+2 Overloads) GetSymbols, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' var ~ formula
    ''' </summary>
    Public Class FormulaExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.formula
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.FormulaDeclare
            End Get
        End Property

        ''' <summary>
        ''' 因变量
        ''' </summary>
        Public ReadOnly Property var As String

        ''' <summary>
        ''' 只能够是符号引用或者双目运算符表达式
        ''' </summary>
        Public ReadOnly Property formula As Expression

        ''' <summary>
        ''' create a new formula expression, example like:
        ''' 
        ''' ```
        ''' y ~ x
        ''' ```
        ''' </summary>
        ''' <param name="y"></param>
        ''' <param name="formula">x factor variable in the formula expression</param>
        Sub New(y As String, formula As Expression)
            Me.var = y
            Me.formula = formula
        End Sub

        ''' <summary>
        ''' get all symbol names from <see cref="formula"/>
        ''' </summary>
        ''' <returns></returns>
        Public Function GetSymbols(env As Environment) As Object
            Dim result = GetSymbols(formula)

            If result Like GetType(String()) Then
                Return result.TryCast(Of String())
            Else
                Return Internal.debug.stop(result.TryCast(Of Exception), env)
            End If
        End Function

        ''' <summary>
        ''' get symbol names from a binary expression or a single symbol reference
        ''' </summary>
        ''' <param name="formula"></param>
        ''' <returns></returns>
        Public Shared Function GetSymbols(formula As Expression) As [Variant](Of String(), Exception)
            If TypeOf formula Is SymbolReference Then
                Return {DirectCast(formula, SymbolReference).symbol}
            ElseIf TypeOf formula Is BinaryExpression Then
                Dim bin As BinaryExpression = DirectCast(formula, BinaryExpression)
                Dim result As New List(Of String)
                Dim tmp As [Variant](Of String(), Exception)

                tmp = GetSymbols(bin.left)

                If Not tmp Like GetType(String()) Then
                    Return tmp
                Else
                    result.AddRange(tmp.TryCast(Of String()))
                End If

                tmp = GetSymbols(bin.right)

                If Not tmp Like GetType(String()) Then
                    Return tmp
                Else
                    result.AddRange(tmp.TryCast(Of String()))
                End If

                Return result.ToArray
            Else
                Return New ArgumentException($"{formula}: expression type {formula.GetType} is invalid! required symbol reference or binary expression!")
            End If
        End Function

        ''' <summary>
        ''' 这个只是用来表述关系的，并不会产生内容
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return $"{var} ~ {formula}"
        End Function
    End Class
End Namespace
