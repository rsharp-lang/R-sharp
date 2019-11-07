Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Runtime.Components

    Friend Class MethodInvoke

        Public method As MethodInfo
        Public target As Object

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Invoke(parameters As Object()) As Object
            Return method.Invoke(target, parameters)
        End Function

    End Class

    Public Class RMethodInfo : Implements RFunction

        ''' <summary>
        ''' The function name
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property name As String Implements RFunction.name
        Public ReadOnly Property returns As RType
        Public ReadOnly Property parameters As RMethodArgument()

        ReadOnly api As [Variant](Of MethodInvoke, [Delegate])

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="closure">
        ''' Runtime generated .NET method
        ''' </param>
        Sub New(name$, closure As [Delegate])
            Me.name = name
            Me.api = closure
            Me.returns = New RType(closure.Method.ReturnType)
            Me.parameters = closure.Method.DoCall(AddressOf parseParameters)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="closure"><see cref="MethodInfo"/> from parsing .NET dll module file.</param>
        ''' <param name="target"></param>
        Sub New(name$, closure As MethodInfo, target As Object)
            Me.name = name
            Me.api = New MethodInvoke With {.method = closure, .target = target}
            Me.returns = New RType(closure.ReturnType)
            Me.parameters = closure.DoCall(AddressOf parseParameters)
        End Sub

        Private Shared Function parseParameters(method As MethodInfo) As RMethodArgument()
            Return method _
                .GetParameters _
                .Select(AddressOf RMethodArgument.ParseArgument) _
                .ToArray
        End Function

        Public Function Invoke(envir As Environment, params As InvokeParameter()) As Object Implements RFunction.Invoke
            Dim parameters As Object()

            If Me.parameters.Any(Function(a) a.isObjectList) Then
                parameters = createObjectListArguments(envir, params).ToArray
            Else
                parameters = createNormalArguments(envir, InvokeParameter.CreateArguments(envir, params)).ToArray
            End If

            For Each arg In parameters
                If Not arg Is Nothing AndAlso arg.GetType Is GetType(Message) Then
                    Return arg
                End If
            Next

            Dim result As Object

            If api Like GetType(MethodInvoke) Then
                result = api.TryCast(Of MethodInvoke).Invoke(parameters)
            Else
                result = api.VB.Method.Invoke(Nothing, parameters.ToArray)
            End If

            Return result
        End Function

        Private Function createObjectListArguments(envir As Environment, params As InvokeParameter()) As IEnumerable(Of Object)
            Dim parameterVals As Object() = New Object(Me.parameters.Length - 1) {}
            Dim declareArguments = Me.parameters.ToDictionary(Function(a) a.name)
            Dim declareNameIndex As Index(Of String) = Me.parameters.Keys.Indexing
            Dim listObject As New List(Of InvokeParameter)
            Dim i As Integer

            For Each arg As InvokeParameter In params
                If declareArguments.ContainsKey(arg.name) Then
                    i = declareNameIndex(arg.name)
                    parameterVals(i) = getValue(declareArguments(arg.name), arg.Evaluate(envir))
                    declareArguments.Remove(arg.name)
                Else
                    listObject.Add(arg)
                End If
            Next

            ' get index of list argument
            i = Me.parameters _
                .First(Function(a) a.isObjectList).name _
                .DoCall(Function(a)
                            Call declareArguments.Remove(a)
                            Return declareNameIndex.IndexOf(a)
                        End Function)
            parameterVals(i) = listObject.ToArray

            If declareArguments.Count > 0 Then
                Dim envirArgument As RMethodArgument = declareArguments _
                    .Values _
                    .Where(Function(a)
                               Return a.type.raw Is GetType(Environment)
                           End Function) _
                    .FirstOrDefault

                If Not envirArgument Is Nothing Then
                    i = declareNameIndex(envirArgument.name)
                    parameterVals(i) = envir
                    declareArguments.Remove(envirArgument.name)
                End If
            End If

            If declareArguments.Count > 0 Then
                Return {
                    Internal.stop({$"Missing parameters value for '{declareArguments.Keys.GetJson}'!", "function: " & name, "environment: " & envir.ToString}, envir)
                }
            Else
                Return parameterVals
            End If
        End Function

        Private Iterator Function createNormalArguments(envir As Environment, arguments As Dictionary(Of String, Object)) As IEnumerable(Of Object)
            Dim arg As RMethodArgument

            For i As Integer = 0 To Me.parameters.Length - 1
                arg = Me.parameters(i)

                If arguments.ContainsKey(arg.name) Then
                    Yield getValue(arg, arguments(arg.name))
                ElseIf i >= arguments.Count Then
                    ' default value
                    If arg.type.raw Is GetType(Environment) Then
                        Yield envir
                    ElseIf Not arg.isOptional Then
                        Yield Internal.stop({$"Missing parameter value for '{arg.name}'!", "function: " & name, "environment: " & envir.ToString}, envir)
                    Else
                        Yield arg.default
                    End If
                Else
                    Yield getValue(arg, arguments("$" & i))
                End If
            Next
        End Function

        Private Shared Function getValue(arg As RMethodArgument, value As Object) As Object
            If arg.type.isArray Then
                Return CObj(Runtime.asVector(value, arg.type.raw.GetElementType))
            Else
                Return Runtime.getFirst(value)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"Dim {name} As {api.ToString}"
        End Function
    End Class
End Namespace