Imports System.Text.RegularExpressions

Namespace Runtime.Interop

    ''' <summary>
    ''' 没有被找到的符号会被这个标记所指定的函数解析为对象参加运算
    ''' </summary>
    Public Class RSymbolLanguageMaskAttribute : Inherits RInteropAttribute

        ' 例如实现 H2O - OH 的化学符号运算

        Public ReadOnly Property Pattern As Regex
        Public ReadOnly Property CanBeCached As Boolean

        ''' <summary>
        ''' <see cref="ITestSymbolTarget"/>
        ''' </summary>
        ''' <returns></returns>
        Public Property Test As Type

        ''' <summary>
        ''' 创建一个自动符号映射标记 
        ''' </summary>
        ''' <param name="pattern">用于测试目标符号文本是否适合用于本解析器的处理</param>
        Sub New(pattern As String, Optional canBeCached As Boolean = False)
            Me.Pattern = New Regex(pattern, RegexOptions.Compiled Or RegexOptions.Singleline)
            Me.CanBeCached = canBeCached
        End Sub

        Public Function IsCurrentPattern(symbol As String) As Boolean
            Return symbol.IsPattern(Pattern)
        End Function

        Public Overrides Function ToString() As String
            Return Pattern.ToString
        End Function

    End Class

    Public Delegate Function ISymbolLanguageParser(symbol As String, env As Environment) As Object

    Public Interface ITestSymbolTarget
        Function Assert(symbol As Object) As Boolean

    End Interface
End Namespace