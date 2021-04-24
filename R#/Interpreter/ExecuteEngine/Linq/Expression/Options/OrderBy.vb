Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My.JavaScript

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class OrderBy : Inherits PipelineKeyword

        Public Overrides ReadOnly Property keyword As String
            Get
                Return "Order By"
            End Get
        End Property

        Dim key As Expression
        Dim desc As Boolean

        Sub New(key As Expression, desc As Boolean)
            Me.key = FixLiteral(key)
            Me.desc = desc
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Exec(context As ExecutableContext) As Object
            Return key.Exec(context)
        End Function

        Private Function GetOrderKey(obj As JavaScriptObject, context As ExecutableContext) As Object
            For Each key As String In obj
                context.SetSymbol(key, obj(key))
            Next

            Return Exec(context)
        End Function

        Public Overrides Function Exec(result As IEnumerable(Of JavaScriptObject), context As ExecutableContext) As IEnumerable(Of JavaScriptObject)
            Dim raw As JavaScriptObject() = result.ToArray
            Dim keys As Object()
            Dim i As Integer()

            If TypeOf key Is Literal Then
                Dim keyName As String = key.Exec(Nothing)

                keys = raw.Select(Function(obj) obj(keyName)).ToArray
            Else
                keys = raw _
                    .Select(Function(obj)
                                Return GetOrderKey(obj, context)
                            End Function) _
                    .ToArray
            End If

            If keys.All(Function(xi) xi.GetType Is GetType(Double)) Then
                i = DoOrder(keys.Select(Function(k) DirectCast(k, Double)))
            ElseIf keys.All(Function(xi) xi.GetType Is GetType(Integer)) Then
                i = DoOrder(keys.Select(Function(k) DirectCast(k, Integer)))
            ElseIf keys.All(Function(xi) xi.GetType Is GetType(String)) Then
                i = DoOrder(keys.Select(Function(k) DirectCast(k, String)))
            Else
                Throw New NotImplementedException
            End If

            Return i.Select(Function(index) raw(index))
        End Function

        Private Function DoOrder(Of T As IComparable(Of T))(keys As IEnumerable(Of T)) As Integer()
            If desc Then
                Return keys.Select(Function(key, i) (key, i)).OrderByDescending(Function(ti) ti.key).Select(Function(ti) ti.i).ToArray
            Else
                Return keys.Select(Function(key, i) (key, i)).OrderBy(Function(ti) ti.key).Select(Function(ti) ti.i).ToArray
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"ORDER BY {key} {If(desc, "DESCENDING", "ASCENDING")}"
        End Function
    End Class
End Namespace