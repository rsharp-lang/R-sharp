#Region "Microsoft.VisualBasic::9d6833a0347447dbdad75fab29a378ed, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\BinaryExpression.vb"

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

    '     Class BinaryExpression
    ' 
    '         Properties: expressionName, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString, vectorCast
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class BinaryExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Binary
            End Get
        End Property

        Friend left, right As Expression
        Friend ReadOnly [operator] As String

        Sub New(left As Expression, right As Expression, op$)
            Me.left = left
            Me.right = right
            Me.operator = op
        End Sub

        Private Shared Function vectorCast(x As vector, env As Environment) As Array
            Dim type As Type = Runtime.MeasureArrayElementType(x.data)
            Dim data As Array = Runtime.asVector(x.data, type, env)

            Return data
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim a As Object = left.Evaluate(envir)
            Dim b As Object = right.Evaluate(envir)

            ' > NULL / NULL
            ' numeric(0)
            If a Is Nothing OrElse b Is Nothing Then
                Return New Object() {}
            End If

            If TypeOf a Is invisible Then
                a = DirectCast(a, invisible).value
            End If
            If TypeOf b Is invisible Then
                b = DirectCast(b, invisible).value
            End If

            If Program.isException(a) Then
                Return a
            ElseIf Program.isException(b) Then
                Return b
            End If

            Dim tleft, tright As RType

            If TypeOf a Is vector Then
                tleft = DirectCast(a, vector).elementType
            Else
                tleft = RType.GetRSharpType(a.GetType)
            End If
            If TypeOf b Is vector Then
                tright = DirectCast(b, vector).elementType
            Else
                tright = RType.GetRSharpType(b.GetType)
            End If

            ' tleft/tright will be nothing if the element type is void type
            If tleft?.raw Like RType.characters OrElse tright?.raw Like RType.characters Then
                Return StringBinaryOperator(envir, a, b, [operator])
            ElseIf tleft?.raw Like RType.logicals AndAlso tright?.raw Like RType.logicals Then
                Select Case [operator]
                    Case "=="
                        Return Runtime.Core.BinaryCoreInternal(Of Boolean, Boolean, Boolean)(
                            x:=Runtime.asVector(Of Boolean)(a),
                            y:=Runtime.asVector(Of Boolean)(b),
                            [do]:=Function(x, y) x = y
                        ).ToArray
                    Case "!="
                        Return Runtime.Core.BinaryCoreInternal(Of Boolean, Boolean, Boolean)(
                            x:=Runtime.asVector(Of Boolean)(a),
                            y:=Runtime.asVector(Of Boolean)(b),
                            [do]:=Function(x, y) x <> y
                        ).ToArray
                    Case "&&"
                        Return Runtime.Core _
                            .BinaryCoreInternal(Of Boolean, Boolean, Boolean)(
                                x:=Core.asLogical(a),
                                y:=Core.asLogical(b),
                                [do]:=Function(x, y) x AndAlso y
                            ).ToArray
                End Select
            End If

            Dim handleResult = BinaryOperatorEngine.getOperator([operator], envir)

            If handleResult Like GetType(Message) Then
                Return handleResult.TryCast(Of Message)
            Else
                Return handleResult.TryCast(Of BinaryIndex).Evaluate(a, b, envir)
            End If
        End Function

        Public Overrides Function ToString() As String
            If [operator] = "-" Then
                If TypeOf left Is Literal AndAlso any.ToString(DirectCast(left, Literal).value) = "0" Then
                    Return $"-{right}"
                Else
                    Return $"{left} {[operator]} {right}"
                End If
            Else
                Return $"{left} {[operator]} {right}"
            End If
        End Function
    End Class
End Namespace
