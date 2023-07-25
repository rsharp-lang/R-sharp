#Region "Microsoft.VisualBasic::edb06eaf583e1d7cf7d005d8269811de, D:/GCModeller/src/R-sharp/studio/Rsharp_IL/IronCodeDom//BlockTags/UsingTag.vb"

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

    '   Total Lines: 18
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 650 B


    ' Class UsingTag
    ' 
    '     Properties: auto, sourceMap, symbol
    ' 
    '     Function: ToExpression
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure

Public Class UsingTag : Inherits PythonCodeDOM

    Public Property symbol As String
    Public Property auto As Expression
    Public Property sourceMap As StackFrame

    Public Overrides Function ToExpression() As Expression
        Dim body As New ClosureExpression(script.ToArray)
        Dim auto As New DeclareNewSymbol(symbol, sourceMap, Me.auto)

        Return New UsingClosure(auto, body, sourceMap)
    End Function

End Class
