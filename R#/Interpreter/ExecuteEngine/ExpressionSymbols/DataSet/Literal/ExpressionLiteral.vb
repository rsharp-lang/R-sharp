#Region "Microsoft.VisualBasic::4a3170767bb75a77ed6d11dcdc17cf6e, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/DataSet/Literal/ExpressionLiteral.vb"

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

    '   Total Lines: 41
    '    Code Lines: 32
    ' Comment Lines: 0
    '   Blank Lines: 9
    '     File Size: 1.37 KB


    '     Class ExpressionLiteral
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Development.Package.File

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    Public Class ExpressionLiteral : Inherits Expression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.formula
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.ExpressionLiteral
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Friend ReadOnly expression As Expression

        Sub New(expression As Expression, stackFrame As StackFrame)
            Me.expression = expression
            Me.stackFrame = stackFrame
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Return expression
        End Function

        Public Overrides Function ToString() As String
            Return $"~""{expression}"""
        End Function
    End Class
End Namespace
