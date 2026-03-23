#Region "Microsoft.VisualBasic::5e0baad8f3e5cfb376334a373a596bde, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Annotation\HostSymbol.vb"

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

    '   Total Lines: 24
    '    Code Lines: 15 (62.50%)
    ' Comment Lines: 5 (20.83%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 4 (16.67%)
    '     File Size: 711 B


    '     Class HostSymbol
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
    ''' ``@HOST``
    ''' 
    ''' ``*.exe`` file path of the ``R#`` script host
    ''' </summary>
    Public Class HostSymbol : Inherits AnnotationSymbol

        Public Overrides ReadOnly Property symbol As String
            Get
                Return "@HOST"
            End Get
        End Property

        Public Overrides Function Evaluate(envir As Runtime.Environment) As Object
            Return App.ExecutablePath
        End Function

        Public Overrides Function ToString() As String
            Return $"@HOST('{App.ExecutablePath}')"
        End Function
    End Class
End Namespace
