#Region "Microsoft.VisualBasic::c7f95e4ba4c69987bdb11512355978ab, R#\Runtime\Internal\invoke.vb"

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

    '     Module invoke
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [stop], invalidParameter, invokeInternals, missingParameter, Rdataframe
    '                   Rlist
    ' 
    '         Sub: (+3 Overloads) add
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline

Namespace Runtime.Internal

    ''' <summary>
    ''' The R internal function invoke helper module
    ''' </summary>
    Module invoke

        ''' <summary>
        ''' 内部函数索引
        ''' </summary>
        ReadOnly index As New Dictionary(Of String, RInternalFuncInvoke)

        Sub New()
            Call RConversion.pushEnvir()
            Call base.pushEnvir()
            Call linq.pushEnvir()
            Call Invokes.file.pushEnvir()
            Call Invokes.stringr.pushEnvir()
        End Sub

        Friend Function getFunction(name As String) As RInternalFuncInvoke
            If index.ContainsKey(name) Then
                Return index(name)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' Add internal invoke handle
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub add(handle As RInternalFuncInvoke)
            index(handle.funcName) = handle
        End Sub

        ''' <summary>
        ''' Add internal invoke handle
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub add(name$, handle As Func(Of Environment, Object(), Object))
            index(name) = New GenericInternalInvoke(name, handle)
        End Sub

        ''' <summary>
        ''' Add internal invoke handle
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Friend Sub add(name$, handle As Func(Of Object, Object))
            index(name) = New GenericInternalInvoke(name, handle)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function [stop](message As Object, envir As Environment) As Message
            Return base.stop(message, envir)
        End Function

        Public Function Rdataframe(envir As Environment, parameters As List(Of Expression)) As Object
            Dim dataframe As New dataframe With {
                .columns = InvokeParameter _
                    .CreateArguments(envir, InvokeParameter.Create(parameters)) _
                    .ToDictionary(Function(a) a.Key,
                                  Function(a)
                                      Return Runtime.asVector(Of Double)(a.Value)
                                  End Function)
            }

            Return dataframe
        End Function

        Public Function Rlist(envir As Environment, parameters As List(Of Expression)) As Object
            Dim list As New Dictionary(Of String, Object)
            Dim slot As Expression
            Dim key As String
            Dim value As Object

            For i As Integer = 0 To parameters.Count - 1
                slot = parameters(i)

                If TypeOf slot Is ValueAssign Then
                    ' 不支持tuple
                    key = DirectCast(slot, ValueAssign) _
                        .targetSymbols(Scan0) _
                        .DoCall(AddressOf ValueAssign.GetSymbol)
                    value = DirectCast(slot, ValueAssign).value.Evaluate(envir)
                Else
                    key = i + 1
                    value = slot.Evaluate(envir)
                End If

                Call list.Add(key, value)
            Next

            Return New list With {.slots = list}
        End Function

        Friend Function missingParameter(funcName$, paramName$, envir As Environment) As Message
            Return Internal.stop({
                $"missing parameter '{paramName}' for function {funcName}",
                $"parameter: {paramName}",
                $"function: {funcName}"
            }, envir)
        End Function

        Friend Function invalidParameter(message$, funcName$, paramName$, envir As Environment) As Message
            Return Internal.stop({
                $"invalid parameter value of '{paramName}' for function {funcName}",
                $"details: {message}",
                $"parameter: {paramName}",
                $"function: {funcName}"
            }, envir)
        End Function

        ''' <summary>
        ''' Invoke the runtime internal functions
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="funcName$"></param>
        ''' <param name="paramVals"></param>
        ''' <returns></returns>
        Public Function invokeInternals(envir As Environment, funcName$, paramVals As Object()) As Object
            If index.ContainsKey(funcName) Then
                Return index(funcName).invoke(envir, paramVals)
            End If

            Select Case funcName
                Case "any" : Return base.any(paramVals(Scan0))
                Case "all" : Return base.all(paramVals(Scan0))
                Case "length" : Return base.length(paramVals(Scan0))
                Case "round"
                    Dim x As Object = paramVals(Scan0)
                    Dim decimals As Integer = Runtime.getFirst(paramVals(1))

                    If x.GetType.IsInheritsFrom(GetType(Array)) Then
                        Return (From element As Object In DirectCast(x, Array).AsQueryable Select Math.Round(CDbl(element), decimals)).ToArray
                    Else
                        Return Math.Round(CDbl(x), decimals)
                    End If
                Case "getOption"
                    Dim name$ = Scripting.ToString(Runtime.getFirst(paramVals.ElementAtOrDefault(Scan0)), Nothing)
                    Dim defaultVal$ = Scripting.ToString(paramVals.ElementAtOrDefault(1))

                    If name.StringEmpty Then
                        Return invoke.missingParameter(funcName, "name", envir)
                    Else
                        Return envir.globalEnvironment _
                            .options _
                            .getOption(name, defaultVal)
                    End If
                Case "source"
                    If paramVals.IsNullOrEmpty Then
                        Return invoke.missingParameter("source", "file", envir)
                    Else
                        Dim file As String = Scripting.ToString(Runtime.getFirst(paramVals(Scan0)))
                        ' run external script
                        Return base.source(file,, envir)
                    End If
                Case "get"
                    Return base.get(paramVals(Scan0), envir)
                Case "print"
                    Return base.print(paramVals(Scan0), envir)
                Case "str"
                    Return base.str(paramVals(Scan0), envir)
                Case "stop"
                    Return Internal.stop(paramVals(Scan0), envir)
                Case "warning"
                    Return base.warning(paramVals(Scan0), envir)
                Case "cat"
                    Return base.cat(paramVals(Scan0), paramVals.ElementAtOrDefault(1), paramVals.ElementAtOrDefault(2, " "))
                Case "lapply"
                    If paramVals.ElementAtOrDefault(1) Is Nothing Then
                        Return Internal.stop({"Missing apply function!"}, envir)
                    ElseIf Not paramVals(1).GetType.ImplementInterface(GetType(RFunction)) Then
                        Return Internal.stop({"Target is not a function!"}, envir)
                    End If

                    If Program.isException(paramVals(Scan0)) Then
                        Return paramVals(Scan0)
                    ElseIf Program.isException(paramVals(1)) Then
                        Return paramVals(1)
                    End If

                    Return base.lapply(paramVals(Scan0), paramVals(1), envir)
                Case "require"
                    Dim libraryNames As String() = paramVals _
                        .Select(AddressOf Scripting.ToString) _
                        .ToArray

                    Throw New NotImplementedException
                Case "install.packages"
                    Dim libraryNames As String() = paramVals _
                        .Select(AddressOf Scripting.ToString) _
                        .ToArray

                    Return utils.installPackages(libraryNames, envir)
                Case "sprintf"
                    Dim format As Array = Runtime.asVector(Of String)(paramVals(Scan0))
                    Dim arguments = paramVals.Skip(1).ToArray
                    Dim sprintf As Func(Of String, Object(), String) = AddressOf CLangStringFormatProvider.sprintf
                    Dim result As String() = format _
                        .AsObjectEnumerator _
                        .Select(Function(str)
                                    Return sprintf(Scripting.ToString(str, "NULL"), arguments)
                                End Function) _
                        .ToArray

                    Return result
                Case "getwd"
                    Return App.CurrentDirectory
                Case Else
                    Return Message.SymbolNotFound(envir, funcName, TypeCodes.closure)
            End Select
        End Function
    End Module
End Namespace
