#Region "Microsoft.VisualBasic::b4fc2dc65962c44729ef56f6094ec19f, ..\sciBASIC#\Data_science\Mathematical\Math\Scripting\Arithmetic.Expression\MetaExpression.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Xml.Serialization

''' <summary>
''' 在<see cref="SimpleExpression.Calculator"></see>之中由于移位操作的需要，需要使用类对象可以修改属性的特性来进行正常的计算，所以请不要修改为Structure类型
''' </summary>
''' <remarks></remarks>
Public Class MetaExpression

    ''' <summary>
    ''' 因为逻辑操作符会是一个英文单词，所以在这里不再是一个char符号了
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property [Operator] As String

    ''' <summary>
    ''' 自动根据类型来计算出结果
    ''' </summary>
    ''' <returns></returns>
    <XmlAttribute> Public Property LEFT As Object
        Get
            If __left Is Nothing Then
                Return _left
            Else
                Return __left()
            End If
        End Get
        Set(value)
            _left = value
            __left = Nothing
            _ReferenceDepth = 0
        End Set
    End Property

    ''' <summary>
    ''' Does the expression value is a constant.
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsValue As Boolean
        Get
            Return __left Is Nothing
        End Get
    End Property

    ''' <summary>
    ''' Does the expression Value comes from a lambda expression?
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property IsExpression As Boolean
        Get
            Return Not __left Is Nothing
        End Get
    End Property

    ''' <summary>
    ''' Value is a constant.
    ''' </summary>
    Dim _left As Object
    ''' <summary>
    ''' Value comes from a lambda expression
    ''' </summary>
    Dim __left As Func(Of Object)

    ''' <summary>
    ''' 默认是0的引用深度
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property ReferenceDepth As Integer

    Sub New()
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="n"></param>
    ''' <param name="opr$">operator 操作符</param>
    Sub New(n As Object, opr$)
        LEFT = n
        [Operator] = opr
    End Sub

    ''' <summary>
    ''' object reference value
    ''' </summary>
    ''' <param name="n"></param>
    Sub New(n)
        LEFT = n
    End Sub

    Sub New(handle As Func(Of Object))
        __left = handle
        ReferenceDepth = 1
    End Sub

    Sub New(simple As SimpleExpression)
        Call Me.New(AddressOf simple.Evaluate)
        ReferenceDepth = simple.ReferenceDepth
    End Sub

    Public Overrides Function ToString() As String
        If __left Is Nothing Then
            Return String.Format("{0} {1}", LEFT, [Operator])
        Else
            Return $"{__left.ToString} {[Operator]}"
        End If
    End Function
End Class
