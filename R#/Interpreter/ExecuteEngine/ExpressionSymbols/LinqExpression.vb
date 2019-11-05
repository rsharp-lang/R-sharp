Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Language.TokenIcer
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine

    ' from x in seq let y = ... where test select projection order by ... asceding distinct 

    Public Class LinqExpression : Inherits Expression

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return sequence.type
            End Get
        End Property

        ''' <summary>
        ''' in
        ''' </summary>
        Dim sequence As Expression
        ''' <summary>
        ''' from/let
        ''' </summary>
        ''' <remarks>
        ''' 第一个元素是from申明的
        ''' 剩余的元素都是let语句申明的
        ''' </remarks>
        Dim locals As New List(Of DeclareNewVariable)
        Dim program As ClosureExpression
        ''' <summary>
        ''' select
        ''' </summary>
        Dim projection As Expression

        Dim output As ClosureExpression

        Sub New(tokens As List(Of Token()))
            Dim variables As New List(Of String)
            Dim i As Integer = 0

            For i = 1 To tokens.Count - 1
                If tokens(i).isIdentifier Then
                    variables.Add(tokens(i)(Scan0).text)
                ElseIf tokens(i).isKeyword("in") Then
                    sequence = Expression.CreateExpression(tokens(i + 1))
                    Exit For
                End If
            Next

            If sequence Is Nothing Then
                Throw New SyntaxErrorException
            Else
                i += 2
                locals = New DeclareNewVariable With {
                    .names = variables.ToArray,
                    .hasInitializeExpression = False,
                    .value = Nothing
                }
            End If

            tokens = tokens _
                .Skip(i) _
                .IteratesALL _
                .SplitByTopLevelDelimiter(TokenType.keyword)

            Call New Pointer(Of Token())(tokens).DoCall(AddressOf doParseLINQProgram)
        End Sub

        Shared ReadOnly linqKeywordDelimiters As String() = {"where", "distinct", "select", "order", "group", "let"}

        Private Sub doParseLINQProgram(p As Pointer(Of Token()))
            Dim buffer As New List(Of Token())
            Dim token As Token()
            Dim program As New List(Of Expression)
            Dim expression As Expression
            Dim output As New List(Of Expression)

            Do While Not p.EndRead
                buffer *= 0
                token = ++p

                If token.isKeyword Then
                    Select Case token(Scan0).text
                        Case "let"
                            buffer += token

                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            Dim declares = buffer _
                                .IteratesALL _
                                .SplitByTopLevelDelimiter(TokenType.operator, True) _
                                .DoCall(Function(blocks)
                                            Return New DeclareNewVariable(blocks)
                                        End Function)

                            program += New ValueAssign(declares.names, declares.value)
                            locals += declares
                            declares.value = Nothing
                        Case "where"
                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            expression = buffer _
                                .IteratesALL _
                                .DoCall(AddressOf Expression.CreateExpression)
                            ' 需要取反才可以正常执行中断语句
                            ' 例如 where 5 < 2
                            ' if test的结果为false
                            ' 则当前迭代循环需要跳过
                            ' 即执行trueclosure部分
                            ' 或者添加一个else closure
                            expression = New BinaryExpression(expression, Literal.FALSE, "==")
                            program += New IfBranch(expression, {New ReturnValue(Literal.NULL)})
                        Case "distinct"
                            output += New FunctionInvoke("unique", New SymbolReference("$"))
                        Case "order"
                            ' order by xxx asc
                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            token = buffer.IteratesALL.ToArray

                            If Not token(Scan0).isKeyword("by") Then
                                Throw New SyntaxErrorException
                            End If

                            ' skip first by keyword
                            expression = token _
                                .Skip(1) _
                                .Take(token.Length - 2) _
                                .DoCall(AddressOf Expression.CreateExpression)
                            output += New FunctionInvoke("sort", expression, New Literal(token.Last.isKeyword("descending")))
                        Case "select"
                            If Not projection Is Nothing Then
                                Throw New SyntaxErrorException("Only allows one project function!")
                            End If

                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            projection = Expression.CreateExpression(buffer.IteratesALL)

                            If TypeOf projection Is VectorLiteral Then
                                projection = New FunctionInvoke("list", DirectCast(projection, VectorLiteral).ToArray)
                            End If
                        Case "group"
                            Do While Not p.EndRead AndAlso Not p.Current.isOneOfKeywords(linqKeywordDelimiters)
                                buffer += ++p
                            Loop

                            Throw New NotImplementedException
                        Case Else
                            Throw New SyntaxErrorException
                    End Select
                End If
            Loop

            Me.program = program.ToArray
            Me.output = output.ToArray
        End Sub

        Public Overrides Function Evaluate(parent As Environment) As Object
            Dim envir As New Environment(parent, "linq_closure")
            Dim sequence As Object
            Dim result As New Dictionary(Of String, Object)
            Dim from As String() = locals(Scan0).names
            Dim key$

            ' 20191105
            ' 序列的产生需要放在变量申明之前
            ' 否则linq表达式中的与外部环境中的同名变量会导致NULL错误出现
            sequence = Me.sequence.Evaluate(envir)

            For Each local As DeclareNewVariable In locals
                Call local.Evaluate(envir)
            Next

            If sequence.GetType Is GetType(Dictionary(Of String, Object)) Then
                sequence = DirectCast(sequence, Dictionary(Of String, Object)).ToArray
            Else
                sequence = Runtime.asVector(Of Object)(sequence)
            End If

            For i As Integer = 0 To sequence.Length - 1
                Dim item = sequence.GetValue(i)

                If TypeOf item Is KeyValuePair(Of String, Object) Then
                    key = DirectCast(item, KeyValuePair(Of String, Object)).Key
                    item = DirectCast(item, KeyValuePair(Of String, Object)).Value
                Else
                    key = i + 1
                End If

                ' from xxx in sequence
                ValueAssign.doValueAssign(envir, from, False, item)
                ' run program
                item = program.Evaluate(envir)

                If Not item Is Nothing AndAlso item.GetType Is GetType(ReturnValue) Then
                    Continue For
                Else
                    result.Add(key, projection.Evaluate(envir))
                End If
            Next

            If output.isEmpty Then
                Return result
            Else
                envir.Push("$", result)

                Return output.Evaluate(envir)
            End If
        End Function
    End Class
End Namespace