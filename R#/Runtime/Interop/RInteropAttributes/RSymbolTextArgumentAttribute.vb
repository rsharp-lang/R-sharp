Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    ''' <summary>
    ''' 表示当前的这个参数的参数值可以接受文本或者未找到符号的变量名作为参数值
    ''' </summary>
    ''' 
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RSymbolTextArgumentAttribute : Inherits RInteropAttribute

        ' 例如
        ' 接受文本参数值   func("text")
        ' 接受符号名参数值 func(text) 如果text不存在于环境中，则等价于func("text")
        ' 如果text存在于环境中，并且值为hello，则等价于func("hello")

        Public Shared Function getSymbolText(symbol As Expression, env As Environment) As String
            Select Case symbol.GetType
                Case GetType(Literal)
                    Return DirectCast(symbol, Literal).value
                Case GetType(SymbolReference)
                    Dim symbolName$ = DirectCast(symbol, SymbolReference).symbol
                    Dim var As Variable = env.FindSymbol(symbolName)

                    If var Is Nothing Then
                        Return symbolName
                    Else
                        Return Scripting.ToString(var.value, Nothing)
                    End If
                Case Else
                    Return Scripting.ToString(symbol.Evaluate(env), Nothing)
            End Select
        End Function
    End Class
End Namespace