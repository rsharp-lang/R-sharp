#Region "Microsoft.VisualBasic::c7e2a79ad25be7234647ec0b455d3a59, studio\Rsharp_IL\IronCodeDom\BlockTags\FunctionTag.vb"

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

    '   Total Lines: 19
    '    Code Lines: 14 (73.68%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: NaN%
    ' 
    '   Blank Lines: 5 (26.32%)
    '     File Size: 718 B


    ' Class FunctionTag
    ' 
    '     Properties: arguments, funcName, stackframe
    ' 
    '     Function: ToExpression, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Public Class FunctionTag : Inherits PythonCodeDOM

    Public Property funcName As String
    Public Property arguments As Expression()
    Public Property stackframe As StackFrame

    Public Overrides Function ToExpression() As Expression
        Return New DeclareNewFunction(funcName, arguments, MyBase.ToExpression(), stackframe)
    End Function

    Public Overrides Function ToString() As String
        Return $"[{level}] {keyword} {funcName}(): {script.JoinBy("; ")}"
    End Function

End Class
