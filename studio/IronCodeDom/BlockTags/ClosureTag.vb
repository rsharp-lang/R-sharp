#Region "Microsoft.VisualBasic::f5cc63403fcdc1eb4da2edb635af7221, studio\IronCodeDom\BlockTags\ClosureTag.vb"

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

    ' Class ClosureTag
    ' 
    '     Properties: assignTarget
    ' 
    '     Function: ToExpression
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Public Class ClosureTag : Inherits PythonCodeDOM

    Public Property assignTarget As Expression()

    Public Overrides Function ToExpression() As Expression
        Dim value As New ClosureExpression(script.ToArray)

        If assignTarget.IsNullOrEmpty Then
            Return value
        Else
            Return New ValueAssignExpression(assignTarget, value)
        End If
    End Function

End Class

