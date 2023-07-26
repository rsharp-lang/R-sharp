#Region "Microsoft.VisualBasic::e5e85795734a0cfc25f2290518a0cceb, D:/GCModeller/src/R-sharp/R#//Runtime/Interop/RInteropAttributes/RSymbolLanguageMaskAttribute.vb"

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

    '   Total Lines: 46
    '    Code Lines: 22
    ' Comment Lines: 12
    '   Blank Lines: 12
    '     File Size: 1.57 KB


    '     Class RSymbolLanguageMaskAttribute
    ' 
    '         Properties: CanBeCached, Pattern, Test
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: IsCurrentPattern, ToString
    ' 
    '     Delegate Function
    ' 
    ' 
    '     Interface ITestSymbolTarget
    ' 
    '         Function: Assert
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
