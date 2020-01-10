Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    Public Class RArgumentList

        ''' <summary>
        ''' Create argument value for <see cref="MethodInfo.Invoke(Object, Object())"/>
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function CreateObjectListArguments([declare] As RMethodInfo, env As Environment, params As InvokeParameter()) As IEnumerable(Of Object)
            Dim parameterVals As Object() = New Object([declare].parameters.Length - 1) {}
            Dim declareArguments = [declare].parameters.ToDictionary(Function(a) a.name)
            Dim declareNameIndex As Index(Of String) = [declare].parameters.Keys.Indexing
            Dim listObject As New List(Of InvokeParameter)
            Dim i As Integer
            Dim sequenceIndex As Integer = Scan0
            Dim paramVal As Object

            For Each arg As InvokeParameter In params
                If declareArguments.ContainsKey(arg.name) OrElse Not arg.isSymbolAssign Then
                    i = declareNameIndex(arg.name)

                    If i > -1 Then
                        paramVal = RMethodInfo.getValue(
                            arg:=declareArguments(arg.name),
                            value:=arg.Evaluate(env),
                            trace:=[declare].name,
                            envir:=env,
                            trygetListParam:=False
                        )
                        parameterVals(i) = paramVal
                        declareArguments.Remove(arg.name)
                    Else
                        paramVal = declareArguments _
                            .First _
                            .DoCall(Function(a)
                                        Return RMethodInfo.getValue(
                                            arg:=a.Value,
                                            value:=arg.Evaluate(env),
                                            trace:=[declare].name,
                                            envir:=env,
                                            trygetListParam:=True
                                        )
                                    End Function)

                        If paramVal Is GetType(Void) Then
                            ' do nothing
                            ' this parameter input is possibly a list argument
                        Else
                            parameterVals(sequenceIndex) = paramVal
                            declareArguments.Remove(declareArguments.First.Key)
                        End If

                        listObject.Add(arg)
                    End If

                    If Not paramVal Is Nothing AndAlso paramVal.GetType Is GetType(Message) Then
                        Return {paramVal}
                    End If
                Else
                    Call listObject.Add(arg)
                End If

                sequenceIndex += 1
            Next

            ' get index of list argument
            i = [declare].parameters _
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
                    parameterVals(i) = env
                    declareArguments.Remove(envirArgument.name)
                End If
            End If

            If declareArguments.Count > 0 Then
                Return {
                    declareArguments.Values _
                        .First _
                        .DoCall(Function(a)
                                    Return RMethodInfo.missingParameter(a, env, [declare].name)
                                End Function)
                }
            Else
                Return parameterVals
            End If
        End Function
    End Class
End Namespace