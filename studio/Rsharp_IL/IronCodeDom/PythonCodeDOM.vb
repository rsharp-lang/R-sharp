#Region "Microsoft.VisualBasic::519c001e6e23f91e567c09413fbbdd90, studio\Rsharp_IL\IronCodeDom\PythonCodeDOM.vb"

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

    '   Total Lines: 32
    '    Code Lines: 24 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (25.00%)
    '     File Size: 953 B


    ' Class PythonCodeDOM
    ' 
    '     Properties: keyword, level, script
    ' 
    '     Function: ToExpression, ToString
    ' 
    '     Sub: (+2 Overloads) Add
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser

<Assembly: InternalsVisibleTo("njl")>
<Assembly: InternalsVisibleTo("npy")>

Public Class PythonCodeDOM

    Public Property keyword As String
    Public Property level As Integer
    Public Property script As List(Of Expression)

    Friend Sub Add(line As SyntaxResult)
        script.Add(line.expression)
    End Sub

    Friend Sub Add(line As Expression)
        script.Add(line)
    End Sub

    Public Overrides Function ToString() As String
        Return $"[{level}] {keyword}: {script.JoinBy("; ")}"
    End Function

    Public Overridable Function ToExpression() As Expression
        Return New ClosureExpression(script.ToArray)
    End Function

End Class
