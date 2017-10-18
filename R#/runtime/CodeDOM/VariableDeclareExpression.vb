#Region "Microsoft.VisualBasic::31ad75c0f85e6b35476f165ce8637372, ..\R-sharp\R#\runtime\CodeDOM\VariableDeclareExpression.vb"

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

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter.Language

Namespace Runtime.CodeDOM

    ''' <summary>
    ''' 创建申明一个内存变量的表达式
    ''' 
    ''' ```R
    ''' var x &lt;- 123;
    ''' var s &lt;- "Hello world!";
    ''' ```
    ''' </summary>
    Public Class VariableDeclareExpression : Inherits PrimitiveExpression

        ''' <summary>
        ''' 左边的变量名称
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Name As String
        ''' <summary>
        ''' type constraint
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Type As TypeCodes = TypeCodes.generic
        Public ReadOnly Property Value As PrimitiveExpression

        Sub New(name$, type$, initialize As PrimitiveExpression)
            Me.Name = name
            Me.Type = type.GetRTypeCode
            Me.Value = initialize
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            With Value.Evaluate(envir)
                Call envir.Push(Name, .value, Type)
                Return .ref
            End With
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {Name} As {Type.Description} = {Value}"
        End Function
    End Class

    Public Class TupleDeclareExpression : Inherits VariableDeclareExpression

        Public Property Members As TupleMember()

        Sub New(members As Statement(Of Tokens)(), initialize As PrimitiveExpression)
            Call MyBase.New("", "list", initialize)

            Me.Members = members _
                .Select(Function(x) New TupleMember(x)) _
                .ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Dim values = Value.Evaluate(envir)

            If TypeOf values.value Is Dictionary(Of String, Object) Then
                Dim table = DirectCast(values.value, Dictionary(Of String, Object))

                For Each member As TupleMember In Members
                    Dim value As Object = member.GetValue(table)
                    Dim var$ = member.Name

                    Call envir.Push(var, value, NameOf(TypeCodes.generic))
                Next
            End If

            Return values
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim [{Members.JoinBy(", ")}] As System.Tuple = {Value}"
        End Function
    End Class

    Public Structure TupleMember

        ''' <summary>
        ''' Member name, can be the same as source name.
        ''' </summary>
        Dim Name$
        ''' <summary>
        ''' source name
        ''' </summary>
        Dim Alias$

        Sub New(declares As Statement(Of Tokens))
            Dim t = declares.tokens

            If t.Length = 1 Then
                Name = t.First.Value
                [Alias] = Nothing
            ElseIf t.Length = 3 AndAlso t(1).Value.TextEquals("As") Then
                ' new_name as old_name
                Name = t(0).Value
                [Alias] = t(2).Value
            End If
        End Sub

        ''' <summary>
        ''' 先使用<see cref="Name"/>进行查找，如果没有结果，则使用原始名称查找，否则抛出错误
        ''' </summary>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Public Function GetValue(table As Dictionary(Of String, Object)) As Object
            If table.ContainsKey(Name) Then
                Return table(Name)
            ElseIf [Alias].StringEmpty OrElse Not table.ContainsKey([Alias]) Then
                Throw New Exception($"Tuple member '{Name}' is not exists in the source!")
            Else
                Return table([Alias])
            End If
        End Function

        Public Overrides Function ToString() As String
            If [Alias].StringEmpty Then
                Return Name
            Else
                Return $"{Name} As ""{[Alias]}"""
            End If
        End Function
    End Structure

    Public Class VariableReference : Inherits PrimitiveExpression

        Public Property ref As String

        Sub New()
        End Sub

        Sub New(ref As Token(Of Tokens))
            Me.ref = ref.Text
        End Sub

        Sub New(var$)
            Me.ref = var
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As TempValue
            Return New TempValue With {
                .type = TypeCodes.ref,
                .value = envir(ref)
            }
        End Function

        Public Overrides Function ToString() As String
            Return "ref " & ref
        End Function
    End Class
End Namespace
