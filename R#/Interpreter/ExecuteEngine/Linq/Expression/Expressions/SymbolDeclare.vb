#Region "Microsoft.VisualBasic::2bff541a1e19f02a99dade06504d5ebd, R#\Interpreter\ExecuteEngine\Linq\Expression\Expressions\SymbolDeclare.vb"

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

    '   Total Lines: 94
    '    Code Lines: 69
    ' Comment Lines: 12
    '   Blank Lines: 13
    '     File Size: 3.72 KB


    '     Class SymbolDeclare
    ' 
    '         Properties: isTuple, keyword, symbol, typeName
    ' 
    '         Function: Exec, SetValue, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.My.JavaScript
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.LINQ

    ''' <summary>
    ''' declare a new temp symbol: ``LET x = ...``
    ''' </summary>
    Public Class SymbolDeclare : Inherits LinqKeywordExpression

        ''' <summary>
        ''' is a <see cref="Literal"/> or <see cref="VectorLiteral"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property symbol As Expression
        Public Property typeName As String

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "LET"
            End Get
        End Property

        Public ReadOnly Property isTuple As Boolean
            Get
                If Not tupleNames.IsNullOrEmpty Then
                    Return True
                ElseIf TypeOf symbol Is VectorLiteral Then
                    Return True
                Else
                    Return False
                End If
            End Get
        End Property

        Dim symbolName As String
        Dim tupleNames As String()

        ''' <summary>
        ''' just create new symbol in the target environment
        ''' </summary>
        ''' <param name="context"></param>
        ''' <returns>returns nothing</returns>
        Public Overrides Function Exec(context As ExecutableContext) As Object
            If TypeOf symbol Is Literal Then
                symbolName = any.ToString(DirectCast(symbol, Literal).value)
                context.AddSymbol(symbolName, TypeCodes.generic)
            ElseIf TypeOf symbol Is VectorLiteral Then
                Dim symbols As New List(Of String)

                For Each element As Expression In DirectCast(symbol, VectorLiteral).elements
                    If TypeOf element Is SymbolReference Then
                        Call symbols.Add(DirectCast(element, SymbolReference).symbolName)
                        Call context.AddSymbol(symbols.Last, TypeCodes.generic)
                    ElseIf TypeOf element Is Literal Then
                        Call symbols.Add(any.ToString(DirectCast(element, Literal).value))
                        Call context.AddSymbol(symbols.Last, TypeCodes.generic)
                    Else
                        Return Internal.debug.stop("symbol expression in tuple vector should be symbol or literal text!", context)
                    End If
                Next

                tupleNames = symbols.ToArray
            Else
                Return Internal.debug.stop("symbol expression should be symbol or literal text!", context)
            End If

            Return Nothing
        End Function

        Public Function SetValue(value As Object, contex As ExecutableContext) As Message
            If Not isTuple Then
                Call contex.SetSymbol(symbolName, value)
            ElseIf TypeOf value Is JavaScriptObject Then
                For Each name As String In tupleNames
                    Call contex.SetSymbol(name, DirectCast(value, JavaScriptObject)(name))
                Next
            Else
                Return Internal.debug.stop("invalid data type!", contex)
            End If

            Return Nothing
        End Function

        Public Overrides Function ToString() As String
            Return $"LET {symbol} As {typeName}"
        End Function
    End Class
End Namespace
