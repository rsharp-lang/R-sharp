#Region "Microsoft.VisualBasic::890ad224867f8a4a6452ba192a77100f, D:/GCModeller/src/R-sharp/studio/Rsharp_IL/nts//SyntaxParser/ExpressionUtils.vb"

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

    '   Total Lines: 39
    '    Code Lines: 29
    ' Comment Lines: 6
    '   Blank Lines: 4
    '     File Size: 1.60 KB


    ' Module ExpressionUtils
    ' 
    '     Function: GetPackageModules, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Module ExpressionUtils

    Public Function GetPackageModules(mods As Expression) As VectorLiteral
        Select Case mods.GetType
            Case GetType(ClosureExpression)
                Dim closure As ClosureExpression = DirectCast(mods, ClosureExpression)
                Dim lines = closure.program _
                    .Select(AddressOf ExpressionUtils.ToString) _
                    .ToArray

                Return New VectorLiteral(lines)
            Case GetType(VectorLiteral)
                Return New VectorLiteral(DirectCast(mods, VectorLiteral).Select(AddressOf ExpressionUtils.ToString))
            Case Else
                Return New VectorLiteral({ExpressionUtils.ToString(mods)})
        End Select
    End Function

    ''' <summary>
    ''' convert any expression to string literal value
    ''' [works for the js imports expression]
    ''' </summary>
    ''' <param name="exp"></param>
    ''' <returns></returns>
    Private Function ToString(exp As Expression) As Literal
        If TypeOf exp Is Literal Then
            Return exp
        ElseIf TypeOf exp Is SymbolReference Then
            Return New Literal(ValueAssignExpression.GetSymbol(exp))
        Else
            Throw New NotImplementedException
        End If
    End Function
End Module
