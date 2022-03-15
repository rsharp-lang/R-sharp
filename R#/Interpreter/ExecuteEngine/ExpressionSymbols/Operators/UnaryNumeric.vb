#Region "Microsoft.VisualBasic::d49c1fec0c427d5244f831f54e5bf6e8, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\UnaryNumeric.vb"

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

        Total Lines:   59
        Code Lines:    45
        Comment Lines: 3
        Blank Lines:   11
        File Size:     2.04 KB


    '     Class UnaryNumeric
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop.Operator

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class UnaryNumeric : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.double
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.UnaryNot
            End Get
        End Property

        Friend ReadOnly [operator] As String
        ''' <summary>
        ''' 可能是一个符号，也可以能是一个对象引用
        ''' </summary>
        Friend ReadOnly numeric As Expression

        Sub New(op As String, number As Expression)
            Me.operator = op
            Me.numeric = number
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim value As Object = numeric.Evaluate(envir)

            If Program.isException(value) Then
                Return value
            End If

            Select Case [operator]
                Case "-"
                    Dim handleResult As [Variant](Of BinaryIndex, Message) = BinaryOperatorEngine.getOperator([operator], envir)

                    If handleResult Like GetType(Message) Then
                        Return handleResult.TryCast(Of Message)
                    Else
                        Return handleResult.TryCast(Of BinaryIndex).Evaluate(0, value, envir)
                    End If
                Case Else
                    Throw New NotImplementedException([operator])
            End Select
        End Function

        Public Overrides Function ToString() As String
            Return $"{[operator]}{numeric}"
        End Function
    End Class
End Namespace
