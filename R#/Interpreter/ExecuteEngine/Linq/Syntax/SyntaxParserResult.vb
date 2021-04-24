#Region "Microsoft.VisualBasic::5d7e00b7a37a32e6ef04ba10b02406fb, R#\Interpreter\ExecuteEngine\Linq\Syntax\SyntaxParserResult.vb"

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

    '     Class SyntaxParserResult
    ' 
    '         Properties: isError
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.ExecuteEngine.LINQ.Syntax

    Public Class SyntaxParserResult

        Public message As Exception
        Public expression As Expression

        Public ReadOnly Property isError As Boolean
            Get
                Return Not message Is Nothing
            End Get
        End Property

        Sub New(err As Exception)
            message = err
        End Sub

        Sub New(expr As Expression)
            expression = expr
        End Sub

        Public Shared Widening Operator CType(exp As Expression) As SyntaxParserResult
            Return New SyntaxParserResult(exp)
        End Operator
    End Class
End Namespace
