Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq

Namespace Runtime.Components

    Public Class RMethodInfo : Implements RFunction

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String Implements RFunction.name
        Public ReadOnly Property api As [Delegate]
        Public ReadOnly Property returns As RType
        Public ReadOnly Property parameters As NamedValue(Of (type As RType, [default] As Object))()

        Sub New(name$, closure As [Delegate])
            Me.name = name
            Me.api = closure
            Me.returns = New RType(closure.Method.ReturnType)
            Me.parameters = closure.Method.DoCall(AddressOf parseParameters)
        End Sub

        Sub New(name$, closure As MethodInfo, target As Object)
            Me.name = name
            Me.api = Function(params As Object())
                         Return closure.Invoke(target, params)
                     End Function
            Me.returns = New RType(closure.ReturnType)
            Me.parameters = closure.DoCall(AddressOf parseParameters)
        End Sub

        Private Shared Function parseParameters(method As MethodInfo) As NamedValue(Of (RType, Object))()
            Return method.GetParameters _
                .Select(Function(p)
                            Return New NamedValue(Of (RType, Object)) With {
                                .Name = p.Name,
                                .Value = (New RType(p.ParameterType), p.DefaultValue),
                                .Description = p.HasDefaultValue
                            }
                        End Function) _
                .ToArray
        End Function

        Public Function Invoke(envir As Environment, arguments As Object()) As Object Implements RFunction.Invoke
            Dim result As Object
            Dim parameters As New List(Of Object)

            For i As Integer = 0 To Me.parameters.Length - 1
                If i >= arguments.Length Then
                    ' default value
                    If Not Me.parameters(i).Description.ParseBoolean Then
                        Return Internal.stop({$"Missing parameter value for '{Me.parameters(i).Name}'!", "function: " & name, "environment: " & envir.ToString}, envir)
                    Else
                        parameters.Add(Me.parameters(i).Value.default)
                    End If
                Else
                    If Me.parameters(i).Value.type.isArray Then
                        parameters.Add(CObj(Runtime.asVector(arguments(i), Me.parameters(i).Value.type.raw.GetElementType)))
                    Else
                        parameters.Add(Runtime.getFirst(arguments(i)))
                    End If
                End If
            Next

            result = api.Method.Invoke(api.Target, parameters.ToArray)

            Return result
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {api.ToString}"
        End Function
    End Class

    Public Interface RFunction

        ''' <summary>
        ''' 函数名
        ''' </summary>
        ''' <returns></returns>
        ReadOnly Property name As String

        ''' <summary>
        ''' 执行当前的这个函数对象然后获取得到结果值
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        Function Invoke(envir As Environment, arguments As Object()) As Object

    End Interface
End Namespace