Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter.ExecuteEngine

    Public Class FunctionInvoke : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return TypeCodes.generic
            End Get
        End Property

        ''' <summary>
        ''' The source location of current function invoke calls
        ''' </summary>
        Dim span As CodeSpan

        Public ReadOnly Property funcName As String

        Friend ReadOnly parameters As List(Of Expression)

        Sub New(tokens As Token())
            Dim params = tokens _
                .Skip(2) _
                .Take(tokens.Length - 3) _
                .ToArray

            funcName = tokens(Scan0).text
            span = tokens(Scan0).span
            parameters = params _
                .SplitByTopLevelDelimiter(TokenType.comma) _
                .Where(Function(t) Not t.isComma) _
                .Select(Function(param)
                            Return Expression.CreateExpression(param)
                        End Function) _
                .ToList
        End Sub

        ''' <summary>
        ''' Use for create pipeline calls from identifier target
        ''' </summary>
        ''' <param name="funcName"></param>
        ''' <param name="parameters"></param>
        Sub New(funcName$, ParamArray parameters As Expression())
            Me.funcName = funcName
            Me.parameters = parameters.ToList
        End Sub

        Public Overrides Function ToString() As String
            Return $"Call {funcName}({parameters.JoinBy(", ")})"
        End Function

        ''' <summary>
        ''' These function create from script text in runtime
        ''' </summary>
        ReadOnly runtimeFuncs As Index(Of Type) = {
            GetType(DeclareNewFunction),
            GetType(DeclareLambdaFunction)
        }

        Public Overrides Function Evaluate(envir As Environment) As Object
            ' 当前环境中的函数符号的优先度要高于
            ' 系统环境下的函数符号
            Dim funcVar As Variable = envir.FindSymbol(funcName)

            If funcVar Is Nothing Then
                ' 可能是一个系统的内置函数
                If funcName = "list" Then
                    Return Runtime.Internal.Rlist(envir, parameters)
                Else
                    Return Runtime.Internal.invokeInternals(envir, funcName, envir.Evaluate(parameters))
                End If
            ElseIf funcVar.value.GetType Like runtimeFuncs Then
                ' invoke method create from R# script
                Return DirectCast(funcVar.value, RFunction).Invoke(envir, InvokeParameter.Create(parameters))
            Else
                ' invoke .NET method
                Return DirectCast(funcVar.value, RMethodInfo).Invoke(envir, InvokeParameter.Create(parameters))
            End If
        End Function
    End Class
End Namespace