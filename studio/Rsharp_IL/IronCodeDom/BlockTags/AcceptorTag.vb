#Region "Microsoft.VisualBasic::be3a71f44d4cfc1d98be6d01d1069534, R-sharp\studio\IronCodeDom\BlockTags\AcceptorTag.vb"

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

'   Total Lines: 14
'    Code Lines: 10
' Comment Lines: 0
'   Blank Lines: 4
'     File Size: 502.00 B


' Class AcceptorTag
' 
'     Properties: target
' 
'     Function: ToExpression
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser.SyntaxImplements

Public Class AcceptorTag : Inherits PythonCodeDOM

    Public Property target As FunctionInvoke

    Public Overrides Function ToExpression() As Expression
        Return target.CreateInvoke(script, Nothing).expression
    End Function

End Class
