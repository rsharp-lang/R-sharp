#Region "Microsoft.VisualBasic::b788b71b2303c3e89da40fbf4508d0a4, R-sharp\R#\Interpreter\ExecuteEngine\Linq\Syntax\SyntaxParserResult.vb"

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


     Code Statistics:

        Total Lines:   44
        Code Lines:    35
        Comment Lines: 0
        Blank Lines:   9
        File Size:     1.25 KB


    '     Class SyntaxParserResult
    ' 
    '         Properties: isError
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.SyntaxParser

Namespace Interpreter.ExecuteEngine.LINQ.Syntax

    Public Class SyntaxParserResult

        Public message As SyntaxError
        Public expression As Expression

        Public ReadOnly Property isError As Boolean
            Get
                Return Not message Is Nothing
            End Get
        End Property

        Sub New(err As SyntaxError)
            message = err
        End Sub

        Sub New(expr As Expression)
            expression = expr
        End Sub

        Friend Sub New(result As SyntaxResult)
            If result.isException Then
                message = result.error
            Else
                expression = New RunTimeValueExpression(result.expression)
            End If
        End Sub

        Public Overrides Function ToString() As String
            If isError Then
                Return "Error: " & message.ToString
            Else
                Return expression.ToString
            End If
        End Function

        Public Shared Widening Operator CType(exp As Expression) As SyntaxParserResult
            Return New SyntaxParserResult(exp)
        End Operator
    End Class
End Namespace
