#Region "Microsoft.VisualBasic::672a4fd414b2262a5c05e2fb84e01b75, R-sharp\R#\System\Document\Inspector\ExpressionFormatter.vb"

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

    '   Total Lines: 30
    '    Code Lines: 19
    ' Comment Lines: 0
    '   Blank Lines: 11
    '     File Size: 724.00 B


    '     Class ExpressionFormatter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: (+2 Overloads) WriteScript
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Namespace Development.Inspector

    Public Class ExpressionFormatter

        Dim text As TextWriter
        Dim indent As Integer = 0

        Sub New(text As TextWriter)
            Me.text = text
        End Sub

        Public Sub WriteScript(expr As Expression)
            If TypeOf expr Is ValueAssignExpression Then

            End If

        End Sub



        Public Shared Sub WriteScript(expr As Expression, text As TextWriter)
            Call New ExpressionFormatter(text).WriteScript(expr)
        End Sub

    End Class
End Namespace
