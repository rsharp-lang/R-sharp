#Region "Microsoft.VisualBasic::7cbab11d3760b87580643abe05a145aa, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Operators\BinaryExpression.vb"

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
'         Properties: type
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: Evaluate, ToString, vectorCast
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Operators

    Public Class BinaryExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes

        Friend left, right As Expression
        Dim [operator] As String

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

            If tleft.raw Like RType.characters OrElse tright.raw Like RType.characters Then
                Return StringBinaryOperator(envir, a, b, [operator])
            ElseIf tleft.raw Like RType.logicals AndAlso tright.raw Like RType.logicals Then
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
            Return $"{left} {[operator]} {right}"
        End Function
    End Class
End Namespace
