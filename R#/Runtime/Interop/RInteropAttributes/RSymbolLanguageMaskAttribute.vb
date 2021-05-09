Imports System.Text.RegularExpressions

Namespace Runtime.Interop

    ''' <summary>
    ''' 没有被找到的符号会被这个标记所指定的函数解析为对象参加运算
    ''' </summary>
    Public Class RSymbolLanguageMaskAttribute : Inherits RInteropAttribute

        ' 例如实现 H2O - OH 的化学符号运算

        Public ReadOnly Property Pattern As Regex

        ''' <summary>
        ''' 创建一个自动符号映射标记 
        ''' </summary>
        ''' <param name="pattern">用于测试目标符号文本是否适合用于本解析器的处理</param>
        Sub New(pattern As String)
            Me.Pattern = New Regex(pattern, RegexOptions.Compiled Or RegexOptions.Singleline)
        End Sub

        Public Function IsCurrentPattern(symbol As String) As Boolean
            Return symbol.IsPattern(Pattern)
        End Function

        Public Overrides Function ToString() As String
            Return Pattern.ToString
        End Function

    End Class

    Public Delegate Function ISymbolLanguageParser(symbol As String, env As Environment) As Object

End Namespace