#Region "Microsoft.VisualBasic::4cf4767cf8330ed4050c2bdddda83e86, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Annotation\HomeSymbol.vb"

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

    '   Total Lines: 22
    '    Code Lines: 15 (68.18%)
    ' Comment Lines: 3 (13.64%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (18.18%)
    '     File Size: 694 B


    '     Class HomeSymbol
    ' 
    '         Properties: symbol
    ' 
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Annotation

    ''' <summary>
    ''' ``@HOME`` for just get the directory path of the R# program file where it is.
    ''' </summary>
    Public Class HomeSymbol : Inherits AnnotationSymbol

        Public Overrides ReadOnly Property symbol As String
            Get
                Return "@HOME"
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Runtime.Environment) As Object
            Return App.HOME
        End Function

        Public Overrides Function ToString() As String
            Return $"@HOME('{App.HOME}')"
        End Function
    End Class
End Namespace
