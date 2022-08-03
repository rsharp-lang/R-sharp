#Region "Microsoft.VisualBasic::94c9d047a71e0f06ae510ae8df352e06, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\DeclareNewFunction.vb"

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

    '   Total Lines: 302
    '    Code Lines: 197
    ' Comment Lines: 61
    '   Blank Lines: 44
    '     File Size: 12.60 KB


    '     Class DeclareNewFunction
    ' 
    '         Properties: [Namespace], body, expressionName, funcName, parameters
    '                     stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Evaluate, getArguments, getReturns, InitializeEnvironment, (+2 Overloads) Invoke
    '                   MissingParameters, ToString
    ' 
    '         Sub: SetSymbol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Language.Syntax.SyntaxParser
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' 普通的函数定义模型
    ''' 
    ''' 普通的函数与lambda函数<see cref="DeclareLambdaFunction"/>在结构上是一致的，
    ''' 但是有一个区别就是lambda函数<see cref="DeclareLambdaFunction"/>是没有<see cref="Environment"/>的，
    ''' 所以lambda函数会更加的轻量化，不容易产生内存溢出的问题
    ''' </summary>
    Public Class DeclareNewFunction : Inherits SymbolExpression
        Implements RFunction
        Implements IRuntimeTrace
        Implements INamespaceReferenceSymbol

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.FunctionDeclare
            End Get
        End Property

        ''' <summary>
        ''' 这个是当前的这个函数的程序包来源名称，在运行时创建的函数的命令空间为``SMRUCC/R#_runtime``
        ''' </summary>
        ''' <returns></returns>
        Public Property [Namespace] As String = SyntaxBuilderOptions.R_runtime Implements INamespaceReferenceSymbol.namespace

        Public ReadOnly Property funcName As String Implements RFunction.name
        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame
        Public ReadOnly Property parameters As DeclareNewSymbol()
        Public ReadOnly Property body As ClosureExpression

        ''' <summary>
        ''' The environment of current function closure
        ''' </summary>
        Friend envir As Environment

        Sub New(funcName$, parameters As DeclareNewSymbol(), body As ClosureExpression, stackframe As StackFrame)
            Me.funcName = funcName
            Me.parameters = parameters
            Me.body = body
            Me.body.program.Rscript = Rscript.FromText(funcName)
            Me.stackFrame = stackframe
        End Sub

        Friend Sub SetSymbol(newName As String)
            _funcName = newName
        End Sub

        Public Iterator Function getArguments() As IEnumerable(Of NamedValue(Of Expression)) Implements RFunction.getArguments
            For Each arg As DeclareNewSymbol In Me.parameters
                For Each name As String In arg.names
                    Yield New NamedValue(Of Expression) With {
                        .Name = name,
                        .Value = arg.value
                    }
                Next
            Next
        End Function

        Friend Shared Function MissingParameters(var As DeclareNewSymbol, funcName$, envir As Environment) As Object
            Dim names As String() = var.names.ToArray
            Dim message As String() = {
                $"argument ""{names.GetJson}"" is missing, with no default",
                $"function: {funcName}",
                $"parameterName: {names.GetJson}",
                $"type: {var.type.Description}"
            }

            Return Internal.debug.stop(message, envir)
        End Function

        ''' <summary>
        ''' set parameters
        ''' </summary>
        ''' <param name="parent"></param>
        ''' <param name="params"></param>
        ''' <param name="runDispose"></param>
        ''' <returns></returns>
        Private Function InitializeEnvironment(parent As Environment, params As InvokeParameter(), ByRef runDispose As Boolean) As [Variant](Of Message, Environment)
            Dim var As DeclareNewSymbol
            Dim value As Object
            Dim arguments As Dictionary(Of String, InvokeParameter)
            Dim envir As Environment = Me.envir

            If envir Is Nothing Then
                runDispose = False
                envir = parent
            Else
                runDispose = True
                envir = parent & envir
            End If

            Dim argumentKeys As String()
            Dim key$

            ' function parameter should be evaluate 
            ' from the parent environment.
            'With InvokeParameter.CreateArguments(parent, params, hasObjectList:=True)
            '    If .GetUnderlyingType Is GetType(Message) Then
            '        Return .TryCast(Of Message)
            '    Else
            '        arguments = .TryCast(Of Dictionary(Of String, Object))
            '        argumentKeys = arguments.Keys.ToArray
            '    End If
            'End With

            arguments = InvokeParameter.CreateArguments(parent, params, hasObjectList:=True)
            argumentKeys = arguments.Keys.ToArray

            ' initialize environment
            For i As Integer = 0 To Me.parameters.Length - 1
                var = Me.parameters(i)

                If arguments.ContainsKey(var.names(Scan0)) Then
                    value = arguments(var.names(Scan0)).Evaluate(envir)
                ElseIf i >= params.Length Then
                    ' missing, use default value
                    If var.hasInitializeExpression Then
                        value = var.value.Evaluate(envir)

                        If Program.isException(value) Then
                            Return DirectCast(value, Message)
                        End If
                    Else
                        Return DirectCast(MissingParameters(var, funcName, envir), Message)
                    End If
                Else
                    key = "$" & i

                    If arguments.ContainsKey(key) Then
                        value = arguments(key).Evaluate(envir)
                    ElseIf var.hasInitializeExpression Then
                        value = var.value.Evaluate(envir)
                    ElseIf TypeOf params(i).value Is ValueAssignExpression Then
                        ' symbol :> func
                        ' will cause parameter name as symbol name
                        ' produce key not found error
                        ' try to fix such bug
                        ' value = arguments(argumentKeys(i))
                        Return Internal.debug.stop({$"argument '{var.names.First}' is required, but missing!", $"name: {var.names.First}"}, envir)
                    Else
                        key = argumentKeys(i)
                        value = arguments(key).Evaluate(envir)
                    End If
                End If

                ' removes global symbol to makes
                ' local symbols overrides of the
                ' global symbols
                For Each name As String In var.names
                    Call envir.Delete(name, seekParent:=False)
                Next

                ' 20191120 对于函数对象而言，由于拥有自己的环境，在构建闭包之后
                ' 多次调用函数会重复利用之前的环境参数
                ' 所以在这里只需要判断一下更新值或者插入新的变量
                'If var.names.Any(AddressOf envir.symbols.ContainsKey) Then
                '    ' 只检查自己的环境中的变量
                '    ' 因为函数参数是只属于自己的环境之中的符号
                '    Dim names As Literal() = var.names _
                '        .Select(Function(name) New Literal(name)) _
                '        .ToArray

                '    Call ValueAssignExpression.doValueAssign(envir, names, True, value)
                'Else
                Dim err As Message = Nothing

                ' 不存在，则插入新的
                Call DeclareNewSymbol.PushNames(var.names, value, var.type, False, envir, err:=err)

                If Not err Is Nothing Then
                    Return err
                End If
                ' End If
            Next

            Return envir
        End Function

        Public Function Invoke(parent As Environment, params As InvokeParameter()) As Object Implements RFunction.Invoke
            Dim runDispose As Boolean = False
            Dim env = InitializeEnvironment(parent, params, runDispose)
            Dim envir As Environment

            If env Like GetType(Message) Then
                Return env.TryCast(Of Message)
            Else
                ' unsure for the symbol conflicts
                envir = New Environment(env.TryCast(Of Environment), stackFrame, isInherits:=False)
            End If

            If runDispose Then
                Using envir
                    Return body.Evaluate(envir)
                End Using
            Else
                Return body.Evaluate(envir)
            End If
        End Function

        ''' <summary>
        ''' direct invoke of this R# function
        ''' </summary>
        ''' <param name="arguments"></param>
        ''' <param name="parent"></param>
        ''' <returns></returns>
        Public Function Invoke(arguments() As Object, parent As Environment) As Object Implements RFunction.Invoke
            Dim envir As Environment = Me.envir
            Dim argVal As Object
            Dim runDispose As Boolean = False

            If envir Is Nothing Then
                envir = parent
            Else
                runDispose = True
                envir = New Environment(parent, stackFrame, isInherits:=False) & envir
            End If

            For i As Integer = 0 To parameters.Length - 1
                Dim names As Literal() = parameters(i).names _
                    .Select(Function(name) New Literal(name)) _
                    .ToArray

                For Each name As String In parameters(i).names
                    Call envir.Push(name, Nothing, [readonly]:=False)
                Next

                If i >= arguments.Length Then
                    If parameters(i).hasInitializeExpression Then
                        argVal = parameters(i).value.Evaluate(envir)

                        If Program.isException(argVal) Then
                            Return argVal
                        End If

                        Call ValueAssignExpression.doValueAssign(envir, names, True, argVal)
                    Else
                        Return MissingParameters(parameters(i), funcName, envir)
                    End If
                Else
                    Call ValueAssignExpression.doValueAssign(envir, names, True, arguments(i))
                End If
            Next

            If runDispose Then
                Using envir
                    Return body.Evaluate(envir)
                End Using
            Else
                Return body.Evaluate(envir)
            End If
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim symbol As Symbol = envir.FindFunction(funcName)

            If symbol Is Nothing Then
                envir.funcSymbols(funcName) = New Symbol(Me, TypeCodes.closure) With {
                    .name = funcName,
                    .[readonly] = False
                }
            Else
                symbol.SetValue(Me, envir)
            End If

            Me.envir = New Environment(envir, stackFrame, isInherits:=True)

            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return $"declare function '${funcName}'({parameters.Select(AddressOf DeclareNewSymbol.getParameterView).JoinBy(", ")}) {{
    # function_internal
    {body}
}}"
        End Function

        Public Function getReturns(env As Environment) As RType Implements RFunction.getReturns
            Return RType.GetRSharpType(GetType(Object))
        End Function
    End Class
End Namespace
