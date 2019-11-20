#Region "Microsoft.VisualBasic::882d2b9df0dfdc45689e4359814d8d4c, R#\Runtime\Internal\invoke.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Module invoke
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: invokeInternals, Rdataframe, Rlist
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Internal

    Delegate Function RFuncInvoke(envir As Environment, funcName$, paramVals As Object()) As Object

    Module invoke

        ReadOnly index As New Dictionary(Of String, RFuncInvoke)

        Sub New()

        End Sub

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
                    key = DirectCast(slot, ValueAssign).targetSymbols(Scan0)
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
            Select Case funcName
                Case "any" : Return base.any(paramVals(Scan0))
                Case "all" : Return base.all(paramVals(Scan0))
                Case "length" : Return DirectCast(paramVals(Scan0), Array).Length
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
                        Return envir.GlobalEnvironment _
                            .options _
                            .getOption(name, defaultVal)
                    End If
                Case "names"
                    Return base.names(paramVals(Scan0), Nothing, envir)
                Case "get"
                    Return base.get(paramVals(Scan0), envir)
                Case "print"
                    Return Internal.print(paramVals(Scan0), envir)
                Case "stop"
                    Return Internal.stop(paramVals(Scan0), envir)
                Case "warning"
                    Return Internal.warning(paramVals(Scan0), envir)
                Case "cat"
                    Return Internal.cat(paramVals(Scan0), paramVals.ElementAtOrDefault(1), paramVals.ElementAtOrDefault(2, " "))
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

                    Return Internal.lapply(paramVals(Scan0), paramVals(1), envir)
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
                Case "setwd"
                    Dim dir As String() = Runtime.asVector(Of String)(paramVals(Scan0))

                    If dir.Length = 0 Then
                        Return invoke.missingParameter(funcName, "dir", envir)
                    ElseIf dir(Scan0).StringEmpty Then
                        Return invoke.invalidParameter("cannot change working directory due to the reason of NULL value provided!", funcName, "dir", envir)
                    Else
                        App.CurrentDirectory = dir(Scan0)
                    End If

                    Return dir(Scan0)

                Case Else
                    Return Message.SymbolNotFound(envir, funcName, TypeCodes.closure)
            End Select
        End Function
    End Module
End Namespace
