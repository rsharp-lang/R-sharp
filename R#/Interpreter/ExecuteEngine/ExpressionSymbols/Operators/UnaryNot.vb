#Region "Microsoft.VisualBasic::185c14cc602ab703f60b63e68f61a2f5, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\UnaryNot.vb"

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

    '     Class UnaryNot
    ' 
    '         Properties: type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    Public Class UnaryNot : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.boolean
            End Get
        End Property

        ReadOnly logical As Expression

        Sub New(logical As Expression)
            Me.logical = logical
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim logicals As Boolean() = Runtime.asLogical(logical.Evaluate(envir))
            Dim nots As Boolean() = logicals _
                .Select(Function(b) Not b) _
                .ToArray

            Return nots
        End Function

        Public Overrides Function ToString() As String
            Return $"(NOT {logical})"
        End Function
    End Class
End Namespace
