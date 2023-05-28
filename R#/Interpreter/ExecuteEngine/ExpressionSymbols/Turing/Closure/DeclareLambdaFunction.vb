#Region "Microsoft.VisualBasic::7d45328638ee8f1b27d02503de1ad036, F:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/Turing/Closure/DeclareLambdaFunction.vb"

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

    '   Total Lines: 241
    '    Code Lines: 167
    ' Comment Lines: 40
    '   Blank Lines: 34
    '     File Size: 9.61 KB


    '     Class DeclareLambdaFunction
    ' 
    '         Properties: closure, expressionName, name, parameterNames, stackFrame
    '                     type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateLambda, Evaluate, getArguments, GetPrintContent, getReturns
    '                   getSingle, GetSymbolName, (+2 Overloads) Invoke, setValueHandler, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' 只允许简单的表达式出现在这里
    ''' 并且参数也只允许出现一个
    ''' </summary>
    ''' <remarks>
    ''' lambda函数与普通函数相比，lambda函数是没有environment的
    ''' 所以lambda函数会更加的轻量化
    ''' </remarks>
    Public Class DeclareLambdaFunction : Inherits SymbolExpression
        Implements RFunction
        Implements RPrint
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.LambdaDeclare
            End Get
        End Property

        Public ReadOnly Property name As String Implements RFunction.name
        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame
        Public ReadOnly Property closure As Expression

        Public ReadOnly Property parameterNames As String()
            Get
                Return parameter.names
            End Get
        End Property

        Dim parameter As DeclareNewSymbol

        Sub New(name$, parameter As DeclareNewSymbol, closure As Expression, stackframe As StackFrame)
            Me.name = name
            Me.parameter = parameter
            Me.closure = closure
            Me.stackFrame = stackframe
        End Sub

        Public Overrides Function GetSymbolName() As String
            Return name
        End Function

        ''' <summary>
        ''' lambda函数是不存在可选默认参数值的
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function getArguments() As IEnumerable(Of NamedValue(Of Expression)) Implements RFunction.getArguments
            For Each name As String In parameter.names
                Yield New NamedValue(Of Expression) With {
                    .Name = name
                }
            Next
        End Function

        ''' <summary>
        ''' 返回的是一个<see cref="RFunction"/>
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return Me
        End Function

        Public Function Invoke(parent As Environment, arguments() As InvokeParameter) As Object Implements RFunction.Invoke
            Using envir As New Environment(parent, stackFrame, isInherits:=False)
                If parameter.symbolSize = 0 Then
                    ' lambda function with no parameter
                    Return closure.Evaluate(envir)
                ElseIf arguments.Length = 0 AndAlso parameter.symbolSize > 0 Then
                    ' no value for the required parameter
                    Return DeclareNewFunction.MissingParameters(parameter, name, envir)
                Else
                    Return arguments _
                        .Select(Function(a) a.Evaluate(envir)) _
                        .ToArray _
                        .DoCall(Function(vals)
                                    Return Invoke(vals, envir)
                                End Function)
                End If
            End Using
        End Function

        Public Function Invoke(arguments() As Object, env As Environment) As Object Implements RFunction.Invoke
            ' lambda function only allows one parameter in 
            ' its declaration
            '
            ' example as:
            ' 
            ' x -> x + 1;
            ' [x, y] -> x + y;       # tuple element is still one parameter, so this is legal
            ' [x, y, z] -> x * y *z; # tuple element is still one parameter, so this is legal
            '
            ' function invoke of the lambda function should be in syntax of
            '
            ' lambda(x)
            ' lambda([x,y])
            ' lambda([x,y,z])
            '
            ' Due to the reason of syntax rule of only allows one parameter.
            '
            Dim argVal As Object

            For i As Integer = 0 To parameter.symbolSize - 1
                argVal = arguments(i)
                env.Push(parameter.names(i), argVal, True, TypeCodes.generic)
            Next

            Dim result As Object = closure.Evaluate(env)

            Return result
        End Function

        Public Function CreateLambda(Of T, Out)(parent As Environment) As Func(Of T, Out)
            Dim envir As New Environment(parent, stackFrame, isInherits:=False)
            Dim v As Symbol()
            Dim err As Message = Nothing

            Call DeclareNewSymbol _
                .PushNames(names:=parameter.names,
                           value:=Nothing,
                           type:=GetType(T).GetRTypeCode,
                           envir:=envir,
                           [readonly]:=False,
                           err:=err
            )

            v = parameter.names _
                .Select(Function(ref)
                            Return envir.FindSymbol(ref, [inherits]:=False)
                        End Function) _
                .ToArray

            Dim isNumbericOut = GetType(Out).GetRTypeCode = TypeCodes.double OrElse GetType(Out).GetRTypeCode = TypeCodes.integer
            Dim isStringOut = GetType(Out) Is GetType(String)
            Dim setValue = setValueHandler(Of T)(v, envir)

            Return Function(x As T) As Out
                       Dim result As Object

                       setValue(x)
                       result = closure.Evaluate(envir)

                       If Program.isException(result) Then
                           Return result
                       End If

                       ' 20200210 对于lambda函数而言，其是运行时创建的函数
                       ' 返回的值很可能是一个向量
                       If Not result Is Nothing Then
                           If Not result.GetType Is GetType(Out) Then
                               result = getSingle(Of Out)(result, isNumbericOut, isStringOut)
                           End If
                       End If

                       Return result
                   End Function
        End Function

        Private Shared Function setValueHandler(Of T)(v As Symbol(), env As Environment) As Action(Of T)
            If v.Length = 1 Then
                Return Sub(x) Call v(Scan0).setValue(x, env)
            ElseIf v.Length > 1 Then
                Dim typeTest As Type = GetType(T)

                If typeTest.IsValueType AndAlso typeTest.ImplementInterface(Of ITuple) Then
                    Return Sub(x)
                               Dim tuple As ITuple = DirectCast(CObj(x), ITuple)

                               For i As Integer = 0 To v.Length - 1
                                   Call v(i).setValue(tuple(i), env)
                               Next
                           End Sub
                Else
                    Throw New InvalidCastException
                End If
            Else
                ' v = 0
                Return Sub()
                           ' do nothing
                       End Sub
            End If
        End Function

        Private Shared Function getSingle(Of Out)(result As Object, isNumbericOut As Boolean, isStringOut As Boolean) As Object
RE0:
            Dim type As Type = result.GetType

            If type.IsArray Then
                If DirectCast(result, Array).Length = 0 Then
                    Return Nothing
                ElseIf type.GetElementType Is GetType(Out) Then
                    result = DirectCast(result, Array).GetValue(Scan0)
                ElseIf isStringOut Then
                    result = Scripting.ToString(DirectCast(result, Array).GetValue(Scan0))
                ElseIf isNumbericOut Then
                    type = type.GetElementType
                    result = Conversion.CTypeDynamic(DirectCast(result, Array).GetValue(Scan0), GetType(Out))
                End If
            ElseIf type Is GetType(vector) Then
                result = DirectCast(result, vector).data
                GoTo RE0
            End If

            Return result
        End Function

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return name
        End Function

        Public Function GetPrintContent() As String Implements RPrint.GetPrintContent
            Return $"``{name}``"
        End Function

        Public Function getReturns(env As Environment) As RType Implements RFunction.getReturns
            Return RType.GetRSharpType(GetType(Object))
        End Function
    End Class
End Namespace
