#Region "Microsoft.VisualBasic::27c1d382d09aae10eb84f2a38768836d, R-sharp\R#\Runtime\Interop\RsharpOperator\BinaryOperatorEngine.vb"

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

    '   Total Lines: 196
    '    Code Lines: 140
    ' Comment Lines: 30
    '   Blank Lines: 26
    '     File Size: 15.60 KB


    '     Module BinaryOperatorEngine
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getOperator
    ' 
    '         Sub: addBinary, addEtcTypeCompres, addFloatOperators, addIntegerOperators, addMixedOperators
    '              arithmeticOperators, dateTimeOperators, ImportsOperators
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Interop.Operator

    ''' <summary>
    ''' engine of binary operator in R#
    ''' </summary>
    Public Module BinaryOperatorEngine

        ReadOnly index As New Dictionary(Of String, BinaryIndex)

        Sub New()
            Call arithmeticOperators()
            Call addEtcTypeCompres()
            Call dateTimeOperators()
        End Sub

        ''' <summary>
        ''' add arithmetic operators
        ''' </summary>
        Private Sub arithmeticOperators()
            Dim numerics As RType() = New Type() {
                GetType(Long),
                GetType(Double)
            }.Select(AddressOf RType.GetRSharpType) _
             .ToArray

            Call addIntegerOperators()
            Call addFloatOperators()

            For Each left As RType In numerics
                For Each right As RType In numerics
                    Call addMixedOperators(left, right)
                Next
            Next
        End Sub

        Private Sub addEtcTypeCompres()
            Dim dateType As RType = RType.GetRSharpType(GetType(Date))
            Dim equalsTo As IBinaryOperator =
                Function(a, b, env)
                    Return BinaryCoreInternal(Of Date, Date, Boolean)(
                            x:=asVector(Of Date)(a),
                            y:=asVector(Of Date)(b),
                            [do]:=Function(x, y)
                                      Dim dx = DirectCast(x, Date)
                                      Dim dy = DirectCast(y, Date)

                                      If dx.Year <> dy.Year Then
                                          Return False
                                      ElseIf dx.Month <> dy.Month Then
                                          Return False
                                      ElseIf dx.Day <> dy.Day Then
                                          Return False
                                      Else
                                          Return True
                                      End If
                                  End Function
                        ) _
                        .ToArray
                End Function

            Call addBinary(dateType, dateType, "==", equalsTo, Nothing)
        End Sub

        ' 20221016 为了更加高效率的实现SIMD相关的功能
        ' 在这里数学计算的的运算符调用就不使用泛型函数来执行了
        ' 在这里的泛型函数可能会带来比较大的性能损失

        Private Sub addFloatOperators()
            Dim left As RType = RType.GetRSharpType(GetType(Double))
            Dim right As RType = RType.GetRSharpType(GetType(Double))

            Call addBinary(left, right, "+", AddressOf Vectorization.Add.f64_op_add_f64, Nothing)
            Call addBinary(left, right, "-", AddressOf Vectorization.Subtract.f64_op_subtract_f64, Nothing)
            Call addBinary(left, right, "*", AddressOf Vectorization.Multiply.f64_op_multiply_f64, Nothing)
            Call addBinary(left, right, "/", AddressOf Vectorization.Divide.f64_op_divide_f64, Nothing)
            Call addBinary(left, right, "%", AddressOf Vectorization.Modulo.f64_op_modulo_f64, Nothing)
            Call addBinary(left, right, "^", AddressOf Vectorization.Exponent.f64_op_exponent_f64, Nothing)
            Call addBinary(left, right, "<", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) < DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, ">", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) > DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "<=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) <= DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, ">=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) >= DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "==", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) = DirectCast(y, Double)).ToArray, Nothing)
            Call addBinary(left, right, "!=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) <> DirectCast(y, Double)).ToArray, Nothing)
        End Sub

        Private Sub addIntegerOperators()
            Dim left As RType = RType.GetRSharpType(GetType(Long))
            Dim right As RType = RType.GetRSharpType(GetType(Long))

            Call addBinary(left, right, "+", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) + DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "-", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) - DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "*", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) * DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "/", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Double)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) / DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "%", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Long)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) Mod DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "^", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Double)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) ^ DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "<", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) < DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, ">", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) > DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "<=", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) <= DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, ">=", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) >= DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "==", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) = DirectCast(y, Long)).ToArray, Nothing)
            Call addBinary(left, right, "!=", Function(a, b, env) BinaryCoreInternal(Of Long, Long, Boolean)(asVector(Of Long)(a), asVector(Of Long)(b), Function(x, y) DirectCast(x, Long) <> DirectCast(y, Long)).ToArray, Nothing)
        End Sub

        ''' <summary>
        ''' left and right should be in different type
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        Private Sub addMixedOperators(left As RType, right As RType)
            Call addBinary(left, right, "+", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) + DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "-", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) - DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "*", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) * DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "/", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) / DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "%", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) Mod DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "^", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Double)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) ^ DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "<", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) < DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, ">", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) > DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "<=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) <= DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, ">=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) >= DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "==", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) = DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(left, right, "!=", Function(a, b, env) BinaryCoreInternal(Of Double, Double, Boolean)(asVector(Of Double)(a), asVector(Of Double)(b), Function(x, y) DirectCast(x, Double) <> DirectCast(y, Double)).ToArray, Nothing, [overrides]:=False)
        End Sub

        Private Sub dateTimeOperators()
            Dim time As RType = RType.GetRSharpType(GetType(Date))
            Dim span As RType = RType.GetRSharpType(GetType(TimeSpan))

            Call addBinary(time, span, "+", Function(a, b, env) BinaryCoreInternal(Of Date, TimeSpan, Date)(asVector(Of Date)(a), asVector(Of TimeSpan)(b), Function(x, y) DirectCast(x, Date) + DirectCast(y, TimeSpan)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(time, span, "-", Function(a, b, env) BinaryCoreInternal(Of Date, TimeSpan, Date)(asVector(Of Date)(a), asVector(Of TimeSpan)(b), Function(x, y) DirectCast(x, Date) - DirectCast(y, TimeSpan)).ToArray, Nothing, [overrides]:=False)
            Call addBinary(time, time, "-", Function(a, b, env) BinaryCoreInternal(Of Date, Date, TimeSpan)(asVector(Of Date)(a), asVector(Of Date)(b), Function(x, y) DirectCast(x, Date) - DirectCast(y, Date)).ToArray, Nothing, [overrides]:=False)
        End Sub

        Public Function getOperator(symbol As String, env As Environment, Optional suppress As Boolean = False) As [Variant](Of BinaryIndex, Message)
            If index.ContainsKey(symbol) Then
                Return index(symbol)
            Else
                Return Internal.debug.stop({$"missing operator '{symbol}'", $"symbol: {symbol}"}, env, suppress)
            End If
        End Function

        ''' <summary>
        ''' add a new operator
        ''' </summary>
        ''' <param name="left"></param>
        ''' <param name="right"></param>
        ''' <param name="symbol"></param>
        ''' <param name="op"></param>
        ''' <param name="env"></param>
        ''' <param name="overrides">
        ''' Overrides of the existed operator evaluation when the operator symbol and the binary type is matched?
        ''' </param>
        Public Sub addBinary(left As RType, right As RType,
                             symbol As String,
                             op As IBinaryOperator,
                             env As Environment,
                             Optional [overrides] As Boolean = True)

            If Not index.ContainsKey(symbol) Then
                index.Add(symbol, New BinaryIndex(symbol))
            End If

            If Not [overrides] AndAlso index(symbol).hasOperator(left, right) Then
                Return
            Else
                Call index(symbol).addOperator(left, right, op, env)
            End If
        End Sub

        ''' <summary>
        ''' imports user defined operator
        ''' </summary>
        ''' <param name="package"></param>
        ''' <param name="env"></param>
        Public Sub ImportsOperators(package As Type, env As Environment)
            Dim methods As MethodInfo() = package.GetMethods _
                .Where(Function(m) m.IsStatic) _
                .ToArray

            For Each method As MethodInfo In methods
                Dim opTag As ROperatorAttribute = method.GetAttribute(Of ROperatorAttribute)
                Dim args = method.GetParameters

                If Not opTag Is Nothing Then
                    Dim left As RType = RType.GetRSharpType(args(Scan0).ParameterType)
                    Dim right As RType = RType.GetRSharpType(args(1).ParameterType)
                    Dim op As New ROperatorInvoke(left, right, method) With {.op = opTag}
                    Dim invoke As IBinaryOperator = op.GetInvoke(argsN:=args.Length)

                    Call addBinary(left, right, opTag.operator, invoke, env, [overrides]:=True)
                End If
            Next
        End Sub

    End Module
End Namespace
