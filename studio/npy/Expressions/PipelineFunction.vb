Imports System.Text.RegularExpressions
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

''' <summary>
''' obj.func(xxx)
''' </summary>
Public Class PipelineFunction : Inherits Expression

    Public Overrides ReadOnly Property type As TypeCodes
        Get
            Return TypeCodes.generic
        End Get
    End Property

    Public Overrides ReadOnly Property expressionName As ExpressionTypes
        Get
            Return ExpressionTypes.FunctionCall
        End Get
    End Property

    Public ReadOnly Property callFunc As FunctionInvoke

    Public ReadOnly Property pipeNextInvoke As Boolean
        Get
            Return TypeOf callFunc.funcName Is Literal AndAlso DirectCast(callFunc.funcName, Literal).ValueStr.Contains("."c)
        End Get
    End Property

    Sub New(calls As FunctionInvoke)
        callFunc = calls
    End Sub

    Public Overrides Function Evaluate(envir As Environment) As Object
        Dim result As Object
        Dim checked As Boolean = False
        Dim callFunc As Object = Me.callFunc.CheckInvoke(envir, checked)
        Dim target As Object

        If TypeOf callFunc Is Message Then
            ' method not found?
            If pipeNextInvoke Then
                result = doPipNextInvoke(DirectCast(Me.callFunc.funcName, Literal).ValueStr, envir)
                Return FunctionInvoke.HandleResult(result, envir)
            Else
                Return callFunc
            End If
        ElseIf TypeOf callFunc Is String AndAlso Not Internal.invoke.isRInternal(callFunc) Then
            result = doPipNextInvoke(callFunc, envir)
            Return FunctionInvoke.HandleResult(result, envir)
        Else
            target = callFunc
        End If

        Using env As New Environment(envir, Me.callFunc.stackFrame, isInherits:=True)
            If TypeOf target Is Regex Then
                ' regexp match
                result = Regexp.Matches(target, Me.callFunc.parameters(Scan0), env)
            ElseIf TypeOf target Is String Then
                ' 可能是一个系统的内置函数
                result = Me.callFunc.invokeRInternal(DirectCast(target, String), envir)
            Else
                ' is runtime function
                result = Me.callFunc.doInvokeFuncVar(target, env)
            End If

            Return FunctionInvoke.HandleResult(result, envir)
        End Using
    End Function

    Private Function doPipNextInvoke(nameStr As String, env As Environment) As Object
        Dim objMethod = nameStr.GetTagValue(".")
        Dim obj As String = objMethod.Name
        Dim method As String = objMethod.Value
    End Function

    Public Overrides Function ToString() As String
        Return callFunc.ToString
    End Function
End Class
