Imports System.Reflection
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts

Namespace Runtime.Internal.Invokes.LinqPipeline

    Module lambda

        Public Function CreatePreidcateLambda(func As Object, env As Environment) As Func(Of Object, Integer, Boolean)
            Return CreateProjectLambda(Of Boolean)(func, env)
        End Function

        Public Function CreateProjectLambda(Of Out)(func As Object, env As Environment) As Func(Of Object, Integer, Out)
            If func.GetType.ImplementInterface(GetType(RFunction)) Then
                Dim declares As RFunction = DirectCast(func, RFunction)

                Return Function(x, i)
                           Return RConversion.CTypeDynamic(declares.Invoke(env, InvokeParameter.Create(x, i)), GetType(Out), env)
                       End Function
            ElseIf TypeOf func Is Func(Of Object, Out) Then
                Dim lambda As Func(Of Object, Out) = func

                Return Function(x, i)
                           Return lambda(x)
                       End Function
            ElseIf TypeOf func Is Func(Of Object, Integer, Out) Then
                Return func
            ElseIf TypeOf func Is Func(Of Out) Then
                Dim lambda As Func(Of Out) = func

                Return Function(x, i)
                           Return lambda()
                       End Function
            ElseIf TypeOf func Is MethodInfo Then
                Dim declares As MethodInfo = DirectCast(func, MethodInfo)

                If declares.ReturnType Is GetType(Out) AndAlso declares.GetParameters.Length <= 2 AndAlso Not declares.IsGenericMethod Then
                    Dim args = declares.GetParameters

                    If args.Length = 0 Then
                        Return Function(x, i)
                                   Return declares.Invoke(Nothing, {})
                               End Function
                    ElseIf args.Length = 1 AndAlso args(Scan0).ParameterType Is GetType(Object) Then
                        Return Function(x, i)
                                   Return declares.Invoke(Nothing, {x})
                               End Function
                    ElseIf args.Length = 2 AndAlso args(Scan0).ParameterType Is GetType(Object) AndAlso args(1).ParameterType Is GetType(Integer) Then
                        Return Function(x, i)
                                   Return declares.Invoke(Nothing, {x, i})
                               End Function
                    Else
                        ' do nothing
                    End If
                Else
                    ' do nothing
                End If
            End If

            Return Nothing
        End Function
    End Module
End Namespace