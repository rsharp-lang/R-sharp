#Region "Microsoft.VisualBasic::7361b1eac3689e12778a0bd07d82f85e, studio\Rsharp_IL\IronCodeDom\BlockTags\ForTag.vb"

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

    '   Total Lines: 29
    '    Code Lines: 23
    ' Comment Lines: 0
    '   Blank Lines: 6
    '     File Size: 1.27 KB


    ' Class ForTag
    ' 
    '     Properties: data, stackFrame, vars
    ' 
    '     Function: ToExpression
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components


Public Class ForTag : Inherits PythonCodeDOM

    Public Property vars As Expression()
    Public Property data As Expression
    Public Property stackFrame As StackFrame

    Public Overrides Function ToExpression() As Expression
        Dim varNames As String() = vars _
            .Select(Function(v)
                        Return ValueAssignExpression.GetSymbol(v)
                    End Function) _
            .ToArray
        Dim varSymbols = varNames.Select(Function(s) New DeclareNewSymbol({s}, Nothing, TypeCodes.generic, [readonly]:=False, stackFrame)).ToArray
        Dim loopBody As New DeclareNewFunction("for_loop", varSymbols, MyBase.ToExpression(), stackFrame)

        Return New ForLoop(varNames, data, loopBody, False, stackFrame)
    End Function

End Class
