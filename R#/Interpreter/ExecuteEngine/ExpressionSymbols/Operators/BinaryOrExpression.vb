#Region "Microsoft.VisualBasic::986a0f914fa670d9e329adc73455d653, E:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/Operators/BinaryOrExpression.vb"

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

    '   Total Lines: 76
    '    Code Lines: 58
    ' Comment Lines: 5
    '   Blank Lines: 13
    '     File Size: 2.64 KB


    '     Class BinaryOrExpression
    ' 
    '         Properties: [operator], expressionName, left, right, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class BinaryOrExpression : Inherits Expression
        Implements IBinaryExpression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Binary
            End Get
        End Property

        Public ReadOnly Property left As Expression Implements IBinaryExpression.left
        Public ReadOnly Property right As Expression Implements IBinaryExpression.right

        Public ReadOnly Property [operator] As String Implements IBinaryExpression.operator
            Get
                Return "||"
            End Get
        End Property

        Sub New(a As Expression, b As Expression)
            left = a
            right = b
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            ' 20191216
            ' fix for
            ' value || stop(xxxx)
            Dim a As Object = left.Evaluate(envir)
            Dim ta As Type = RType.TypeOf(a)

            If ta Like RType.logicals Then
                Dim flags As Boolean() = CLRVector.asLogical(a)

                If flags.Length = 1 AndAlso flags(Scan0) = True Then
                    Return True
                End If

                ' boolean = a || b
                Dim b As Object = right.Evaluate(envir)

                If Program.isException(b) Then
                    Return b
                Else
                    Return Vectorization _
                        .BinaryCoreInternal(Of Boolean, Boolean, Boolean)(
                            x:=CLRVector.asLogical(a),
                            y:=CLRVector.asLogical(b),
                            [do]:=Function(x, y, env2) x OrElse y,
                            env:=envir
                        )
                End If
            Else
                ' let arg as string = ?"--opt" || default;
                If Internal.Invokes.base.isEmpty(a) Then
                    Return right.Evaluate(envir)
                Else
                    Return a
                End If
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({left} || {right})"
        End Function
    End Class
End Namespace
