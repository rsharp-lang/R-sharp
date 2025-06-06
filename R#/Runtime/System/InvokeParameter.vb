﻿#Region "Microsoft.VisualBasic::6150175675e424d18cd0234bc06acd5d, R#\Runtime\System\InvokeParameter.vb"

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

    '   Total Lines: 341
    '    Code Lines: 234 (68.62%)
    ' Comment Lines: 64 (18.77%)
    '    - Xml Docs: 81.25%
    ' 
    '   Blank Lines: 43 (12.61%)
    '     File Size: 13.43 KB


    '     Class InvokeParameter
    ' 
    '         Properties: haveSymbolName, index, isAcceptor, isErr, isFormula
    '                     isProbablyVectorNameTuple, isSymbolAssign, name, value
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: Create, CreateArguments, CreateLiterals, Evaluate, GetArgumentValue
    '                   GetLazyEvaluateExpression, GetSymbolName, PopulateDotDotDot, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components

    Public Class InvokeParameter

        Public Property value As Expression

        Public ReadOnly Property index As Integer

        Public ReadOnly Property name As String
            Get
                Return GetSymbolName(expr:=value)
            End Get
        End Property

        Public ReadOnly Property isAcceptor As Boolean
            Get
                Return TypeOf value Is AcceptorClosure
            End Get
        End Property

        ''' <summary>
        ''' is syntax of ``a &lt;- b``?
        ''' 
        ''' (主要是应用于生成list的参数列表)
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isSymbolAssign As Boolean
            Get
                If value Is Nothing Then
                    Return False
                Else
                    Return TypeOf value Is ValueAssignExpression
                End If
            End Get
        End Property

        Public ReadOnly Property isFormula As Boolean
            Get
                If value Is Nothing Then
                    Return False
                Else
                    Return TypeOf value Is FormulaExpression
                End If
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="hasObjectList">
        ''' If allows hasObjectList, then <see cref="SymbolReference"/> will be used as list slot symbol name
        ''' otherwise only allows get symbol name from parameter name reference pattern ``a &lt;- b``.
        ''' </param>
        ''' <returns></returns>
        Public ReadOnly Property haveSymbolName(hasObjectList As Boolean) As Boolean
            Get
                If value Is Nothing Then
                    Return False
                ElseIf TypeOf value Is ValueAssignExpression Then
                    Return True
                ElseIf TypeOf value Is SymbolReference Then
                    Return hasObjectList
                Else
                    Return False
                End If
            End Get
        End Property

        Public ReadOnly Property isProbablyVectorNameTuple As Boolean
            Get
                If TypeOf value Is VectorLiteral Then
                    Return DirectCast(value, VectorLiteral).type = TypeCodes.string
                Else
                    Return False
                End If
            End Get
        End Property

        Public ReadOnly Property isErr As Boolean
            Get
                If TypeOf value Is RuntimeValueLiteral Then
                    Return TypeOf DirectCast(value, RuntimeValueLiteral).value Is Message
                End If

                Return False
            End Get
        End Property

        Friend Sub New()
        End Sub

        ''' <summary>
        ''' Create a new parameter with given symbol name
        ''' </summary>
        ''' <param name="name">
        ''' the index name pattern number should be keeps the same order with the <paramref name="index"/>.
        ''' example as: name = $1, and the index must be 1
        ''' </param>
        ''' <param name="runtimeValue"></param>
        ''' <param name="index">1-based offset of the invoke arguments</param>
        ''' <remarks>
        ''' combine <paramref name="name"/> and <paramref name="runtimeValue"/> for 
        ''' create an expressin of <see cref="ValueAssignExpression"/>.
        ''' </remarks>
        Sub New(name As String, runtimeValue As Object, index As Integer)
            Me.index = index
            Me.value = New ValueAssignExpression({name}, New RuntimeValueLiteral(runtimeValue))
        End Sub

        Sub New(runtimeValue As Object, Optional index As Integer = Scan0)
            Me.index = index
            Me.value = New RuntimeValueLiteral(runtimeValue)
        End Sub

        Public Shared Function GetSymbolName(expr As Expression) As String
            If expr Is Nothing Then
                Return ""
            ElseIf TypeOf expr Is SymbolReference Then
                Return DirectCast(expr, SymbolReference).symbol
            ElseIf TypeOf expr Is ValueAssignExpression Then
                Return DirectCast(expr, ValueAssignExpression) _
                    .targetSymbols(Scan0) _
                    .DoCall(AddressOf ValueAssignExpression.GetSymbol)
            ElseIf TypeOf expr Is DeclareNewSymbol Then
                Return DirectCast(expr, DeclareNewSymbol).names.First
            ElseIf TypeOf expr Is VectorLiteral Then
                With DirectCast(expr, VectorLiteral)
                    If .length = 1 AndAlso TypeOf .First Is ValueAssignExpression Then
                        ' [a = b] :> func(...)
                        Return DirectCast(.First, ValueAssignExpression).targetSymbols(Scan0).ToString
                    Else
                        Return .ToString
                    End If
                End With
            ElseIf TypeOf expr Is DeclareLambdaFunction Then
                Return DirectCast(expr, DeclareLambdaFunction).parameterNames.ElementAtOrDefault(Scan0)
            Else
                Return ValueAssignExpression.GetSymbol(expr)
            End If
        End Function

        ''' <summary>
        ''' Just get the value part expression, not to evaluate it
        ''' </summary>
        ''' <returns></returns>
        Public Function GetLazyEvaluateExpression() As Expression
            If value Is Nothing Then
                Return Nothing
            ElseIf Not TypeOf value Is ValueAssignExpression Then
                Return value
            Else
                Return DirectCast(value, ValueAssignExpression).value
            End If
        End Function

        ''' <summary>
        ''' get value part
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate(envir As Environment) As Object
            Dim lazy As Expression = GetLazyEvaluateExpression()

            If lazy Is Nothing Then
                Return Nothing
            Else
                Return lazy.Evaluate(envir)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return name
        End Function

        ''' <summary>
        ''' Just wrap parameter value with index
        ''' </summary>
        ''' <param name="expressions"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Create(expressions As IEnumerable(Of Expression)) As InvokeParameter()
            Return expressions _
                .Select(Function(e, i)
                            Return New InvokeParameter With {
                                ._value = e,
                                ._index = i + 1
                            }
                        End Function) _
                .ToArray
        End Function

        Friend Shared Iterator Function PopulateDotDotDot(dotVals As Object) As IEnumerable(Of KeyValuePair(Of String, Object))
            If Not TypeOf dotVals Is Dictionary(Of String, Object) Then
                If TypeOf dotVals Is list Then
                    dotVals = DirectCast(dotVals, list).slots
                End If
            End If

            For Each par As KeyValuePair(Of String, Object) In DirectCast(dotVals, Dictionary(Of String, Object))
                Yield par
            Next
        End Function

        Public Shared Function GetArgumentValue(args As Expression(), name$, offset%, [default] As Object, env As Environment) As Object
            ' find by name at first
            For Each arg As Expression In args
                If TypeOf arg Is ValueAssignExpression Then
                    If GetSymbolName(DirectCast(arg, ValueAssignExpression).targetSymbols(0)) = name Then
                        Return DirectCast(arg, ValueAssignExpression).EvaluateValue(env)
                    End If
                End If
            Next

            ' get parameter by offsets
            Dim valExpr As Expression = args.ElementAtOrNull(offset)

            If valExpr Is Nothing Then
                Return [default]
            Else
                Return valExpr.Evaluate(env)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="arguments"></param>
        ''' <param name="hasObjectList">
        ''' If has object list argument, then use the symbol name as slot name
        ''' </param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateArguments(env As Environment,
                                               arguments As IEnumerable(Of InvokeParameter),
                                               hasObjectList As Boolean) As Dictionary(Of String, InvokeParameter)

            Dim argVals As New Dictionary(Of String, InvokeParameter)
            Dim allArgs As InvokeParameter() = arguments.ToArray
            Dim acceptor As AcceptorClosure = Nothing
            Dim dotdotdot As InvokeParameter = Nothing

            If allArgs.Length > 0 AndAlso allArgs(Scan0).isAcceptor Then
                acceptor = allArgs(Scan0).value
                allArgs = allArgs _
                    .Skip(1) _
                    .ToArray
            End If

            For Each arg As SeqValue(Of InvokeParameter) In allArgs.SeqIterator(offset:=If(acceptor Is Nothing, 0, 1))
                Dim keyName As String
                Dim argVal As InvokeParameter

                If arg.value.haveSymbolName(hasObjectList) Then
                    keyName = arg.value.name
                Else
                    keyName = "$" & arg.i
                End If

                argVal = arg.value ' .Evaluate(env)

                'If Program.isException(argVal) Then
                '    Return DirectCast(argVal, Message)
                'Else
                If Not argVals.ContainsKey(keyName) AndAlso argVal.name <> "..." Then
                    argVals.Add(keyName, argVal)
                ElseIf argVal.name = "..." Then
                    dotdotdot = argVal
                End If

                If Not acceptor Is Nothing Then
                    If arg.value.isSymbolAssign Then
                        Dim assign As ValueAssignExpression = arg.value.value
                        Dim assignVal = assign.value.Evaluate(env)

                        Call env _
                            .acceptorArguments _
                            .Add(keyName, assignVal)
                    End If
                End If
                ' End If
            Next

            If Not dotdotdot Is Nothing Then
                ' join the parameters from ... symbol
                Dim dotVals As Object = dotdotdot.Evaluate(env)

                If TypeOf dotVals Is Message Then
                    ' returns the error message
                    Return New Dictionary(Of String, InvokeParameter) From {{"...", New InvokeParameter(dotVals)}}
                End If

                For Each par As KeyValuePair(Of String, Object) In PopulateDotDotDot(dotVals)
                    If Not argVals.ContainsKey(par.Key) Then
                        Call argVals.Add(par.Key, New InvokeParameter(par.Value, argVals.Count))
                    End If
                Next
            End If

            If acceptor Is Nothing Then
                Return argVals
            End If

            Dim newAlignArgs As New Dictionary(Of String, InvokeParameter) From {
                {"$0", New InvokeParameter() With {.value = acceptor}}' .Evaluate(env)}
            }

            For Each keyName As String In argVals.Keys
                newAlignArgs.Add(keyName, argVals(keyName))
            Next

            Return newAlignArgs
        End Function

        ''' <summary>
        ''' create argument by runtime literal values.
        ''' </summary>
        ''' <param name="args"></param>
        ''' <returns></returns>
        Public Shared Function CreateLiterals(ParamArray args As Object()) As InvokeParameter()
            Return args _
                .Select(Function(a, i)
                            Return New InvokeParameter() With {
                                ._value = New RuntimeValueLiteral(a),
                                ._index = i + 1
                            }
                        End Function) _
                .ToArray
        End Function
    End Class
End Namespace
