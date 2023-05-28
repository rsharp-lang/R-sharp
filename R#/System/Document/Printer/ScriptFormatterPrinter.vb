#Region "Microsoft.VisualBasic::67246310e9053851660fc1ec5f26d537, F:/GCModeller/src/R-sharp/R#//System/Document/Printer/ScriptFormatterPrinter.vb"

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

    '   Total Lines: 96
    '    Code Lines: 82
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 4.40 KB


    '     Module ScriptFormatterPrinter
    ' 
    '         Function: (+5 Overloads) Format, formatLambda
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development

    Public Module ScriptFormatterPrinter

        Public Function Format(literal As Literal) As String
            Select Case literal.type
                Case TypeCodes.boolean : Return literal.Evaluate(Nothing).ToString.ToUpper
                Case TypeCodes.double, TypeCodes.integer : Return literal.Evaluate(Nothing).ToString
                Case TypeCodes.string : Return literal.Evaluate(Nothing).ToString
                Case Else

                    If literal.isNull Then
                        Return "NULL"
                    Else
                        Throw New InvalidCastException
                    End If

            End Select
        End Function

        Public Function Format(funCall As FunctionInvoke) As String
            Dim args As String() = funCall.parameters _
                .Select(AddressOf Format) _
                .ToArray
            Dim refName As String = Format(funCall.funcName)

            Return $"{refName}({args.JoinBy(", ")})"
        End Function

        Public Function Format(assign As ValueAssignExpression) As String
            Dim symbol As String() = assign.targetSymbols.Select(AddressOf Format).ToArray
            Dim symbolText As String

            If symbol.Length = 1 Then
                symbolText = symbol(Scan0)
            Else
                symbolText = $"[{symbol.JoinBy(", ")}]"
            End If

            Return $"{symbolText} = {Format(assign.value)}"
        End Function

        Public Function Format(ref As SymbolIndexer) As String
            Select Case ref.indexType
                Case SymbolIndexers.nameIndex
                    Return $"{Format(ref.symbol)}[[{Format(ref.index)}]]"
                Case SymbolIndexers.vectorIndex
                    Return $"{Format(ref.symbol)}[{Format(ref.index)}]"
                Case SymbolIndexers.dataframeRows
                    Return $"{Format(ref.symbol)}[{Format(ref.index)}, ]"
                Case SymbolIndexers.dataframeColumns
                    Return $"{Format(ref.symbol)}[, {Format(ref.index)}]"
                Case Else
                    Throw New NotImplementedException
            End Select
        End Function

        Public Function Format(expr As Expression) As String
            Select Case expr.GetType
                Case GetType(Literal) : Return Format(DirectCast(expr, Literal))
                Case GetType(VectorLiteral) : Return $"[{DirectCast(expr, VectorLiteral).Select(AddressOf Format).JoinBy(", ")}]"
                Case GetType(SymbolReference) : Return DirectCast(expr, SymbolReference).symbol
                Case GetType(FunctionInvoke) : Return Format(DirectCast(expr, FunctionInvoke))
                Case GetType(ValueAssignExpression) : Return Format(DirectCast(expr, ValueAssignExpression))
                Case GetType(SymbolIndexer) : Return Format(DirectCast(expr, SymbolIndexer))
                Case GetType(UnaryNumeric)
                    With DirectCast(expr, UnaryNumeric)
                        Return $"{ .operator}{Format(.numeric)}"
                    End With
                Case GetType(DeclareLambdaFunction) : Return formatLambda(expr)
                Case Else
                    If expr.GetType.ImplementInterface(Of IBinaryExpression) Then
                        With DirectCast(expr, IBinaryExpression)
                            Return $"{Format(.left)} { .operator} {Format(.right)}"
                        End With
                    Else
                        Throw New NotImplementedException(expr.ToString)
                    End If
            End Select
        End Function

        Private Function formatLambda(lambda As DeclareLambdaFunction) As String
            Dim symbols As String() = lambda.parameterNames
            Dim expr As String = Format(lambda.closure)

            Return $"[{symbols.JoinBy(", ")}] -> {expr}"
        End Function
    End Module
End Namespace
