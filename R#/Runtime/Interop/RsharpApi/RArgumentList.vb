﻿#Region "Microsoft.VisualBasic::1fe55f3fdb751c8262484b70defb9dea, R#\Runtime\Interop\RsharpApi\RArgumentList.vb"

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


    ' Code Statistics:

    '   Total Lines: 463
    '    Code Lines: 306 (66.09%)
    ' Comment Lines: 101 (21.81%)
    '    - Xml Docs: 60.40%
    ' 
    '   Blank Lines: 56 (12.10%)
    '     File Size: 20.17 KB


    '     Class RArgumentList
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: CreateLeftMarginArguments, CreateObjectListArguments, CreateRightMarginArguments, fillOptionalArguments, objectListArgumentIndex
    '                   objectListArgumentMargin, TryCastListObjects, TryCastListObjectsInternal
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
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
            Dim x As RMethodArgument
            Dim dotdotdot As Object = Nothing

            For Each arg As InvokeParameter In params
                If arg.isSymbolAssign AndAlso arg.name Like parameterNames Then
                    skipListObject = True
                    x = declareArguments(arg.name)
                    parameterVals(parameterNames(arg.name) + 1) = RMethodInfo.getValue(
                        arg:=x,
                        value:=If(x.islazyeval, arg.value, arg.Evaluate(env)),
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
                            x = [declare].parameters(sequenceIndex)
                            parameterVals(sequenceIndex) = RMethodInfo.getValue(
                                arg:=x,
                                value:=If(x.islazyeval, arg.value, arg.Evaluate(env)),
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
                    ' current parameter is list object
                    If arg.name = "..." Then
                        ' should join the list object with dotdotdot
                        dotdotdot = arg.Evaluate(env)
                    Else
                        Call listObject.Add(arg)
                    End If
                End If
            Next

            If Not dotdotdot Is Nothing Then
                For Each par As KeyValuePair(Of String, Object) In InvokeParameter.PopulateDotDotDot(dotdotdot)
                    Call listObject.Add(New InvokeParameter(par.Key, par.Value, listObject.Count))
                Next
            End If

            parameterVals(Scan0) = listObject.ToArray
            parameterVals = fillOptionalArguments(parameterVals, normalNames, declareArguments, parameterNames, [declare].name, 1, env)
            parameterVals = TryCastListObjects(parameterVals, Scan0, [declare].parameters(Scan0), env)

            Return parameterVals
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="parameterVals"></param>
        ''' <param name="listIndex"></param>
        ''' <param name="argument"></param>
        ''' <param name="env"></param>
        ''' <returns>
        ''' error message could be put into this return array, and the callee 
        ''' function will check the error message and then populate out to the
        ''' upper environment context
        ''' </returns>
        Private Shared Function TryCastListObjects(parameterVals As Object(), listIndex As Integer, argument As RMethodArgument, env As Environment) As Object()
            If parameterVals.Length = 1 AndAlso Program.isException(parameterVals(Scan0)) Then
                Return parameterVals
            End If

            If argument.type.mode = TypeCodes.list Then
                Dim values = parameterVals(listIndex)
                Dim result = TryCastListObjectsInternal(values, env, argument.islazyeval)

                If Program.isException(result) Then
                    Return New Object() {result}
                Else
                    parameterVals(listIndex) = result
                End If
            End If

            Return parameterVals
        End Function

        ''' <summary>
        ''' try to resolve the argument parameter list
        ''' </summary>
        ''' <param name="values"></param>
        ''' <param name="env"></param>
        ''' <param name="lazy"></param>
        ''' <returns>
        ''' should be a tuple list for the additional parameters, or error message
        ''' </returns>
        Private Shared Function TryCastListObjectsInternal(values As Object, env As Environment, lazy As Boolean) As Object
            Dim list As New list With {
                .slots = New Dictionary(Of String, Object)
            }
            Dim value As Object
            Dim listArgsSlotKey As String

            If TypeOf values Is list Then
                Return values
            End If

            ' push arguments to the list at here
            ' the symbol name is not so importants?
            For Each val As InvokeParameter In DirectCast(values, InvokeParameter())
                listArgsSlotKey = val.name

                If val.name.StringEmpty Then
                    'Return New Object() {
                    '    Internal.debug.stop({
                    '        $"invalid parameter name, name value could not be nothing!",
                    '        $"exp: {val.ToString}"
                    '    }, env)
                    '}

                    ' 20230214 the missing name should not crash
                    ' the scripting environment when construct a
                    ' new argument list object parameter
                    '
                    ' so we create an temp name by its list index
                    ' value at here
                    listArgsSlotKey = $"${val.index}"
                End If

                If Not lazy Then
                    value = val.Evaluate(env)
                Else
                    value = val.GetLazyEvaluateExpression
                End If

                If Program.isException(value) Then
                    Return value
                Else
                    Call list.slots.Add(listArgsSlotKey, value)
                End If
            Next

            Return list
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

                If listObject Is Nothing Then
                    Return New Object() {
                        Internal.debug.stop("invalid clr function declares for the list object arguments, list object argument should be the first argument or the last argument of the function parameters.", env)
                    }
                End If
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
                        ElseIf argv.islazyeval Then
                            argVal = arg.GetLazyEvaluateExpression
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
            If params.Length > 0 AndAlso params(Scan0).isAcceptor Then
                For Each par As InvokeParameter In params.Skip(1)
                    If par.isSymbolAssign Then
                        If [declare].GetArgumentOrdinal(par.name) = -1 Then
                            Dim assign As ValueAssignExpression = par.value
                            Dim eval As Object = assign.value.Evaluate(env)

                            If TypeOf eval Is Message Then
                                Return {eval}
                            End If

                            env.acceptorArguments(par.name) = eval
                        End If
                    End If
                Next
            End If

            If [declare].listObjectMargin = ListObjectArgumentMargin.left Then
                Return CreateLeftMarginArguments([declare], params, env)
            Else
                Return CreateRightMarginArguments([declare], params, env)
            End If
        End Function
    End Class
End Namespace
