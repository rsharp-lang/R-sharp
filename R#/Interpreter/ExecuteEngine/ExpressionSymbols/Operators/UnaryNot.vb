#Region "Microsoft.VisualBasic::41fdfae9fbc6aa4beab5a95267659ad1, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\UnaryNot.vb"

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

        Total Lines:   51
        Code Lines:    40
        Comment Lines: 0
        Blank Lines:   11
        File Size:     1.55 KB


    '     Class UnaryNot
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: [Not], Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports REnv = SMRUCC.Rsharp.Runtime

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class UnaryNot : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.boolean
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.UnaryNot
            End Get
        End Property

        Friend ReadOnly logical As Expression

        Sub New(logical As Expression)
            Me.logical = logical
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim logicals As Object = logical.Evaluate(envir)

            If Program.isException(logicals) Then
                Return logicals
            Else
                Return [Not](logicals)
            End If
        End Function

        Public Shared Function [Not](logical As Object) As Boolean()
            Dim logicals As Boolean() = REnv.asLogical(logical)
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
