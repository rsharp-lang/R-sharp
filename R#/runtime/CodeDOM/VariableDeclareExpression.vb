Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Interpreter

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

        Public Overrides Function ToString() As String
            Return $"Dim {Name} As {Type.Description} = "
        End Function
    End Class

    Public Class TupleDeclareExpression : Inherits VariableDeclareExpression

        Public Property Members As TupleMember()

        Sub New(members As Statement(Of LanguageTokens)(), initialize As PrimitiveExpression)
            Call MyBase.New("", "list", initialize)
            Me.Members = members _
            .Select(Function(x) New TupleMember(x)) _
            .ToArray
        End Sub

        Public Overrides Function Evaluate(envir As Environment) As Object
            Dim values = Value.Evaluate(envir)

        End Function

        Public Overrides Function ToString() As String
            Return $"Dim [{Members.JoinBy(", ")}] As System.Tuple = "
        End Function
    End Class

    Public Structure TupleMember

        Dim Name$
        Dim Alias$

        Sub New(declares As Statement(Of LanguageTokens))
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

        Public Overrides Function ToString() As String
            If [Alias].StringEmpty Then
                Return Name
            Else
                Return $"{Name} As ""{[Alias]}"""
            End If
        End Function
    End Structure

    Public Class VariableReference : Inherits PrimitiveExpression

        Public Property ref As Token(Of LanguageTokens)

    End Class
End Namespace