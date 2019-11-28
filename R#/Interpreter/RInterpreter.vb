#Region "Microsoft.VisualBasic::60b55f4ce76fa2d6a4d8d1795f9705f1, R#\Interpreter\RInterpreter.vb"

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

'     Class RInterpreter
' 
'         Properties: globalEnvir, Rsharp, warnings
' 
'         Constructor: (+1 Overloads) Sub New
' 
'         Function: (+2 Overloads) Evaluate, finalizeResult, FromEnvironmentConfiguration, InitializeEnvironment, Invoke
'                   printErrorInternal, Run, RunInternal, Source
' 
'         Sub: (+3 Overloads) Add, LoadLibrary, PrintMemory
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Interpreter

    Public Class RInterpreter

        ''' <summary>
        ''' Global runtime environment.(全局环境)
        ''' </summary>
        Public ReadOnly Property globalEnvir As GlobalEnvironment
        Public ReadOnly Property warnings As New List(Of Message)

        Default Public ReadOnly Property GetValue(name As String) As Object
            Get
                Return globalEnvir(name).value
            End Get
        End Property

        Public Const lastVariableName$ = "$"

        Sub New(Optional envirConf As Options = Nothing)
            If envirConf Is Nothing Then
                envirConf = New Options(ConfigFile.localConfigs)
            End If

            globalEnvir = New GlobalEnvironment(Me, envirConf)
            globalEnvir.Push(lastVariableName, Nothing, TypeCodes.generic)
        End Sub

        Public Sub PrintMemory(Optional dev As TextWriter = Nothing)
            Dim table$()() = globalEnvir _
                .Select(Function(v)
                            Dim value$ = Variable.GetValueViewString(v)

                            Return {
                                v.name,
                                v.typeCode.ToString,
                                v.typeof.FullName,
                                $"[{v.length}] {value}"
                            }
                        End Function) _
                .ToArray

            With dev Or App.StdOut
                Call .DoCall(Sub(device)
                                 Call table.PrintTable(
                                    dev:=device,
                                    leftMargin:=3,
                                    title:={"name", "mode", "typeof", "value"}
                                 )
                             End Sub)
            End With
        End Sub

        ''' <summary>
        ''' Load packages from package name or dll module file
        ''' </summary>
        ''' <param name="packageName">
        ''' package namespace or dll module file path
        ''' </param>
        Public Function LoadLibrary(packageName As String) As RInterpreter
            Dim result As Message = globalEnvir.LoadLibrary(packageName)

            If Not result Is Nothing Then
                If packageName.FileExists Then
                    ' is a dll file
                    Call [Imports].LoadLibrary(packageName, globalEnvir, {"*"})
                Else
                    Call Interpreter.printMessageInternal(result)
                End If
            End If

            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(name$, value As Object, Optional type As TypeCodes = TypeCodes.generic)
            Call globalEnvir.Push(name, value, type)
        End Sub

        Public Sub Add(name$, closure As [Delegate])
            globalEnvir.Push(name, New RMethodInfo(name, closure), TypeCodes.closure)
        End Sub

        Public Sub Add(name$, closure As MethodInfo, Optional target As Object = Nothing)
            globalEnvir.Push(name, New RMethodInfo(name, closure, target), TypeCodes.closure)
        End Sub

        Public Function Invoke(funcName$, ParamArray args As Object()) As Object
            Dim symbol = globalEnvir.FindSymbol(funcName)

            If symbol Is Nothing Then
                Throw New EntryPointNotFoundException($"No object named '{funcName}' could be found in global environment!")
            ElseIf symbol.typeCode <> TypeCodes.closure OrElse Not symbol.typeof.ImplementInterface(GetType(RFunction)) Then
                Throw New InvalidProgramException($"Object '{funcName}' is not a function!")
            End If

            Return DirectCast(symbol.value, RFunction).Invoke(globalEnvir, args)
        End Function

        ''' <summary>
        ''' Run R# script program from text data.
        ''' </summary>
        ''' <param name="script">The script text</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate(script As String) As Object
            Return RunInternal(script, Nothing, {})
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Run(program As Program) As Object
            Return finalizeResult(program.Execute(globalEnvir))
        End Function

        Private Function InitializeEnvironment(source$, arguments As NamedValue(Of Object)()) As Environment
            Dim envir As Environment

            If source Is Nothing Then
                envir = globalEnvir
            Else
                envir = New Environment(globalEnvir, source)
            End If

            For Each var As NamedValue(Of Object) In arguments
                Call envir.Push(var.Name, var.Value)
            Next

            Return envir
        End Function

        Private Function finalizeResult(result As Object) As Object
            Dim last As Variable = Me.globalEnvir(lastVariableName)

            ' set last variable in current environment
            last.value = result

            If Program.isException(result) Then
                Call printMessageInternal(message:=result)
            End If

            If globalEnvir.messages > 0 Then
                For Each message As Message In globalEnvir.messages
                    Call message.DoCall(AddressOf printMessageInternal)
                Next
            End If

            Return result
        End Function

        Private Function RunInternal(script$, source$, arguments As NamedValue(Of Object)()) As Object
            Dim globalEnvir As Environment = InitializeEnvironment(source, arguments)
            Dim program As Program = Code.ParseScript(script).DoCall(AddressOf Program.CreateProgram)
            Dim result As Object = program.Execute(globalEnvir)

            Return finalizeResult(result)
        End Function

        ''' <summary>
        ''' Run R# script program from a given script file.
        ''' (运行脚本的时候调用的是<see cref="globalEnvir"/>全局环境)
        ''' </summary>
        ''' <param name="filepath">The script file path.</param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Source(filepath$, ParamArray arguments As NamedValue(Of Object)()) As Object
            Return RunInternal(filepath.ReadAllText, filepath.ToFileURL, arguments)
        End Function

        Public Shared ReadOnly Property Rsharp As New RInterpreter

        Public Shared Function Evaluate(script$, ParamArray args As NamedValue(Of Object)()) As Object
            SyncLock Rsharp
                With Rsharp
                    If Not args.IsNullOrEmpty Then
                        Dim name$
                        Dim value As Object

                        For Each var As NamedValue(Of Object) In args
                            name = var.Name
                            value = var.Value

                            Call .globalEnvir.Push(name, value, NameOf(TypeCodes.generic))
                        Next
                    End If

                    Return .Evaluate(script)
                End With
            End SyncLock
        End Function

        Public Shared Function FromEnvironmentConfiguration(configs As String) As RInterpreter
            Return New RInterpreter(New Options(configs))
        End Function
    End Class
End Namespace
