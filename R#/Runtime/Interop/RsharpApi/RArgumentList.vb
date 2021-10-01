#Region "Microsoft.VisualBasic::fc8c5ddb396492fb3bb0df80f8bbf53e, R#\Runtime\Interop\RsharpApi\RArgumentList.vb"

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

    '     Class RArgumentList
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateLeftMarginArguments, CreateObjectListArguments, CreateRightMarginArguments, fillOptionalArguments, objectListArgumentIndex
    '                   objectListArgumentMargin, TryCastListObjects
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Runtime.Interop

    Public NotInheritable Class RArgumentList

        Private Sub New()
        End Sub

        ''' <summary>
        ''' get index of list argument
        ''' </summary>
        ''' <param name="[declare]"></param>
        ''' <returns></returns>
        Private Shared Function objectListArgumentIndex([declare] As RMethodInfo) As Integer
            Dim args As RMethodArgument() = [declare].parameters

            For i As Integer = 0 To args.Length - 1
                If args(i).isObjectList Then
                    Return i
                End If
            Next

            Return -1
        End Function

        Public Shared Function objectListArgumentMargin([declare] As RMethodInfo) As ListObjectArgumentMargin
            Dim index As Integer = objectListArgumentIndex([declare])

            If index = -1 Then
                Return ListObjectArgumentMargin.none
            ElseIf index = Scan0 Then
                Return ListObjectArgumentMargin.left
            ElseIf index = [declare].parameters.Length - 1 Then
                Return ListObjectArgumentMargin.right
            Else
                If [declare].parameters.Last.type.isEnvironment AndAlso [declare].parameters.Length > 2 Then
                    If index = [declare].parameters.Length - 2 Then
                        Return ListObjectArgumentMargin.right
                    End If
                End If
            End If

            Return ListObjectArgumentMargin.invalid
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="[declare]">
        ''' the first argument in api parameters is the list object argument
        ''' all of the argument 
        ''' </param>
        ''' <param name="env"></param>
        ''' <param name="params">
        ''' parameter declares example as: ``(..., file, env)``
        ''' 
        ''' valids syntax:
        ''' 
        ''' 1. ``(a,b,c,d,e,f,g, file = "...")``
        ''' 2. ``(a= 1, b=2, c=a, d= 55, bb, ee, file = "...")``
        ''' </param>
        ''' <returns></returns>
        Private Shared Function CreateLeftMarginArguments([declare] As RMethodInfo, params As InvokeParameter(), env As Environment) As IEnumerable(Of Object)
            Dim parameterVals As Object() = New Object([declare].parameters.Length - 1) {}
            ' the parameter name of the list object argument has been removed
            ' so all of the index value should be move forward 1 unit.
            Dim parameterNames As Index(Of String) = [declare].parameters.Skip(1).Keys
            Dim declareArguments = [declare].parameters.ToDictionary(Function(a) a.name)
            Dim listObject As New List(Of InvokeParameter)
            Dim skipListObject As Boolean = False
            Dim normalNames As New List(Of String)
            Dim sequenceIndex As Integer = Scan0

            For Each arg As InvokeParameter In params
                If arg.isSymbolAssign AndAlso arg.name Like parameterNames Then
                    skipListObject = True
                    parameterVals(parameterNames(arg.name) + 1) = RMethodInfo.getValue(
                        arg:=declareArguments(arg.name),
                        value:=arg.Evaluate(env),
                        trace:=[declare].name,
                        envir:=env,
                        trygetListParam:=False
                    )
                    normalNames.Add(arg.name)
                    sequenceIndex += 1
                ElseIf skipListObject Then
                    ' normal parameters
                    If arg.isSymbolAssign Then
                        ' but arg.name is not one of the parameterNames
                        ' syntax error?
                        Dim syntaxError = Internal.debug.stop({
                            "syntax error!",
                            "the argument is not expected at here!",
                            "argument name: " & arg.name
                        }, env)

                        Return New Object() {syntaxError}
                    Else
                        If Not parameterVals(sequenceIndex) Is Nothing Then
                            ' is already have value at here
                            ' syntax error
                            Dim syntaxError = Internal.debug.stop({
                                "syntax error!",
                                "the argument is not expected at here!",
                                "argument expression: " & arg.value.ToString
                            }, env)

                            Return New Object() {syntaxError}
                        Else
                            parameterVals(sequenceIndex) = RMethodInfo.getValue(
                                arg:=[declare].parameters(sequenceIndex),
                                value:=arg.Evaluate(env),
                                trace:=[declare].name,
                                envir:=env,
                                trygetListParam:=False
                            )
                            normalNames.Add([declare].parameters(sequenceIndex).name)
                            sequenceIndex += 1
                        End If
                    End If
                Else
                    ' still a list object argument
                    ' 当前的参数为list object
                    listObject.Add(arg)
                End If
            Next

            parameterVals(Scan0) = listObject.ToArray
            parameterVals = fillOptionalArguments(parameterVals, normalNames, declareArguments, parameterNames, [declare].name, 1, env)
            parameterVals = TryCastListObjects(parameterVals, Scan0, [declare].parameters(Scan0), env)

            Return parameterVals
        End Function

        Private Shared Function TryCastListObjects(parameterVals As Object(), listIndex As Integer, argument As RMethodArgument, env As Environment) As Object()
            Dim values = parameterVals(listIndex)

            If argument.type.mode = TypeCodes.list Then
                Dim list As New list With {
                    .slots = New Dictionary(Of String, Object)
                }
                Dim value As Object

                For Each val As InvokeParameter In DirectCast(values, InvokeParameter())
                    value = val.Evaluate(env)
                    list.slots.Add(val.name, value)
                Next

                parameterVals(listIndex) = list
            End If

            Return parameterVals
        End Function

        Private Shared Function fillOptionalArguments(parameterVals As Object(),
                                                      normalNames As IEnumerable(Of String),
                                                      declareArguments As Dictionary(Of String, RMethodArgument),
                                                      parameterNames As Index(Of String),
                                                      funcName$,
                                                      offset As Integer,
                                                      env As Environment) As Object()
            Dim listIndex As Integer = -1
            Dim keyNames As String() = parameterNames.Objects

            For Each name As String In normalNames
                Call declareArguments.Remove(name)
            Next

            For i As Integer = 0 To declareArguments.Count - 1
                If i = keyNames.Length Then
                    Exit For
                End If

                If declareArguments.ContainsKey(keyNames(i)) AndAlso declareArguments(keyNames(i)).isObjectList Then
                    listIndex = i
                    Exit For
                End If
            Next

            Dim listObject As InvokeParameter() = {}

            If listIndex > -1 Then
                listObject = parameterVals(listIndex)
            End If

            For Each arg As RMethodArgument In declareArguments.Values
                If arg.isOptional Then
                    If arg.type.isEnvironment Then
                        parameterVals(parameterNames(arg.name) + offset) = env
                    ElseIf Not arg.isObjectList Then
                        If TypeOf arg.default Is Expression Then
                            parameterVals(parameterNames(arg.name) + offset) = DirectCast(arg.default, Expression).Evaluate(env)
                        ElseIf listObject.Any(Function(kvp) kvp.name = arg.name) Then
                            parameterVals(parameterNames(arg.name) + offset) = listObject _
                                .Where(Function(kvp) kvp.name = arg.name) _
                                .First _
                                .Evaluate(env)
                            parameterVals(parameterNames(arg.name) + offset) = RMethodInfo.getValue(
                                arg:=arg,
                                value:=parameterVals(parameterNames(arg.name) + offset),
                                trace:=env.stackFrame.ToString,
                                envir:=env,
                                trygetListParam:=False
                            )
                        Else
                            parameterVals(parameterNames(arg.name) + offset) = arg.default
                        End If
                    Else
                        ' skip of the optional <argument_list>
                    End If
                ElseIf arg.type.isEnvironment Then
                    parameterVals(parameterNames(arg.name) + offset) = env
                ElseIf Not arg.isObjectList Then
                    Return New Object() {
                        RMethodInfo.missingParameter(arg, env, funcName)
                    }
                End If
            Next

            Return parameterVals
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="[declare]"></param>
        ''' <param name="env"></param>
        ''' <param name="params">
        ''' ```
        ''' (a,b,c = xxx, ...)
        ''' ```
        ''' 
        ''' 1. (1,c =2, b=33, d=5,cc=66)
        ''' 2. (1,2,3, d=5, cc=66)
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 处理参数在最后或者倒数第二个的情况
        ''' </remarks>
        Private Shared Function CreateRightMarginArguments([declare] As RMethodInfo, params As InvokeParameter(), env As Environment) As IEnumerable(Of Object)
            Dim parameterVals As Object() = New Object([declare].parameters.Length - 1) {}
            Dim declareArguments = [declare].parameters.ToDictionary(Function(a) a.name)
            Dim declareNameIndex As Index(Of String) = [declare].parameters.Keys.Indexing
            Dim listObject As New List(Of InvokeParameter)
            Dim sequenceIndex As Integer = Scan0
            Dim listIndex As Integer = parameterVals.Length - 1

            If [declare].parameters.Last.type.isEnvironment Then
                listIndex = listIndex - 1
            End If

            Dim i As Integer
            Dim arg As InvokeParameter
            Dim normalNames As New List(Of String)
            Dim argv As RMethodArgument
            Dim argVal As Object

            For i = 0 To params.Length - 1
                arg = params(i)

                If arg.isSymbolAssign Then
                    If arg.name Like declareNameIndex Then
                        argv = declareArguments(arg.name)

                        If argv.requireRawExpression AndAlso arg.isAcceptor Then
                            argVal = arg.value
                        Else
                            argVal = arg.Evaluate(env)
                        End If

                        parameterVals(declareNameIndex(arg.name)) = RMethodInfo.getValue(
                            arg:=argv,
                            value:=argVal,
                            trace:=[declare].name,
                            envir:=env,
                            trygetListParam:=False
                        )
                        normalNames.Add(arg.name)
                    Else
                        listObject.Add(arg)
                    End If
                Else
                    argv = [declare].parameters(sequenceIndex)

                    ' make bugs fixed for the required raw expression
                    ' in parallel api, example as:
                    '
                    ' parallel(x = seqVals, z = bbb, n_threads = 2) {
                    '    print(x);
                    '    x +5 + z;
                    ' };
                    '
                    If argv.requireRawExpression AndAlso arg.isAcceptor Then
                        argVal = arg.value
                    Else
                        argVal = arg.Evaluate(env)
                    End If

                    parameterVals(sequenceIndex) = RMethodInfo.getValue(
                        arg:=argv,
                        value:=argVal,
                        trace:=[declare].name,
                        envir:=env,
                        trygetListParam:=False
                    )
                    normalNames.Add([declare].parameters(sequenceIndex).name)
                End If

                sequenceIndex += 1

                If sequenceIndex = listIndex Then
                    Exit For
                End If
            Next

            For j As Integer = i + 1 To params.Length - 1
                listObject.Add(params(j))
            Next

            If parameterVals(listIndex) IsNot Nothing AndAlso listObject.Count = 0 Then
                ' skip
            Else
                parameterVals(listIndex) = listObject.ToArray
            End If

            parameterVals = fillOptionalArguments(parameterVals, normalNames, declareArguments, declareNameIndex, [declare].name, 0, env)
            parameterVals = TryCastListObjects(parameterVals, listIndex, [declare].parameters(listIndex), env)

            Return parameterVals
        End Function

        ''' <summary>
        ''' Create argument value for <see cref="MethodInfo.Invoke(Object, Object())"/>
        ''' </summary>
        ''' <param name="params">
        ''' required of replace dot(.) to underline(_)?
        ''' </param>
        ''' <returns></returns>
        Friend Shared Function CreateObjectListArguments([declare] As RMethodInfo, env As Environment, params As InvokeParameter()) As IEnumerable(Of Object)
            If [declare].listObjectMargin = ListObjectArgumentMargin.left Then
                Return CreateLeftMarginArguments([declare], params, env)
            Else
                Return CreateRightMarginArguments([declare], params, env)
            End If
        End Function
    End Class
End Namespace
