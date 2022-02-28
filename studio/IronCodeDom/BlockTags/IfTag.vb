#Region "Microsoft.VisualBasic::0e9740f7fdf743d4b52740b0819c8ca2, studio\IronCodeDom\BlockTags\IfTag.vb"

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

    ' Class IfTag
    ' 
    '     Properties: stackframe, test
    ' 
    '     Function: ToExpression
    ' 
    ' Class ElseTag
    ' 
    '     Properties: stackframe
    ' 
    '     Function: ToExpression
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Public Class IfTag : Inherits PythonCodeDOM

    Public Property test As Expression
    Public Property stackframe As StackFrame

    Public Overrides Function ToExpression() As Expression
        Return New IfBranch(test, DirectCast(MyBase.ToExpression(), ClosureExpression), stackframe)
    End Function

End Class

Public Class ElseTag : Inherits PythonCodeDOM

    Public Property stackframe As StackFrame

    Public Overrides Function ToExpression() As Expression
        Return New ElseBranch(MyBase.ToExpression(), stackframe)
    End Function

End Class
