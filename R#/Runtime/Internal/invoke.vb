﻿#Region "Microsoft.VisualBasic::76ae1dff183719a0b0389408e7e2defa, R#\Runtime\Internal\invoke.vb"

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

    '   Total Lines: 193
    '    Code Lines: 134 (69.43%)
    ' Comment Lines: 40 (20.73%)
    '    - Xml Docs: 92.50%
    ' 
    '   Blank Lines: 19 (9.84%)
    '     File Size: 8.19 KB


    '     Class invoke
    ' 
    '         Properties: ls
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: [stop], getAllInternals, getFunction, invalidParameter, invokeInternals
    '                   isRInternal, missingParameter
    ' 
    '         Sub: pushEnvir
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Utils
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.Operator

<Assembly: InternalsVisibleTo("snowFall")>
<Assembly: InternalsVisibleTo("R#")>

Namespace Runtime.Internal

    ''' <summary>
    ''' The R internal function invoke helper module
    ''' </summary>
    Public NotInheritable Class invoke

        ''' <summary>
        ''' 内部函数索引
        ''' </summary>
        ''' <remarks>
        ''' a mapping of [R function name => function body]
        ''' </remarks>
        Shared ReadOnly index As New Dictionary(Of String, RMethodInfo)

        Public Shared ReadOnly Property ls As list
            Get
                Return index.Values _
                    .GroupBy(Function(api)
                                 Return api.GetNetCoreCLRDeclaration _
                                    .DeclaringType _
                                    .NamespaceEntry _
                                    .Namespace
                             End Function) _
                    .ToDictionary(Function(pkg) pkg.Key,
                                  Function(group)
                                      Return group _
                                         .Select(Function(api) api.name) _
                                         .ToArray _
                                         .DoCall(Function(l)
                                                     Return CObj(l)
                                                 End Function)
                                  End Function) _
                    .DoCall(Function(list)
                                Return New list With {
                                    .slots = list
                                }
                            End Function)
            End Get
        End Property

        Shared Sub New()
            Call GetType(base).DoCall(AddressOf pushEnvir)
            Call GetType(RConversion).DoCall(AddressOf pushEnvir)
            Call GetType(env).DoCall(AddressOf pushEnvir)
            Call GetType(linq).DoCall(AddressOf pushEnvir)
            Call GetType(Invokes.file).DoCall(AddressOf pushEnvir)
            Call GetType(stringr).DoCall(AddressOf pushEnvir)
            Call GetType(strings).DoCall(AddressOf pushEnvir)
            Call GetType(utils).DoCall(AddressOf pushEnvir)
            Call GetType(math).DoCall(AddressOf pushEnvir)
            Call GetType(etc).DoCall(AddressOf pushEnvir)
            Call GetType([set]).DoCall(AddressOf pushEnvir)
            ' Call GetType(graphics).DoCall(AddressOf pushEnvir)
            Call GetType(applys).DoCall(AddressOf pushEnvir)
            Call GetType(reflections).DoCall(AddressOf pushEnvir)
            Call GetType(devtools).DoCall(AddressOf pushEnvir)
            Call GetType(ranking).DoCall(AddressOf pushEnvir)
            Call GetType(TableJoint).DoCall(AddressOf pushEnvir)
            Call GetType(rust).DoCall(AddressOf pushEnvir)
            Call GetType(dataframe_methods).DoCall(AddressOf pushEnvir)
            Call GetType(bitView).DoCall(AddressOf pushEnvir)
            Call GetType(search).DoCall(AddressOf pushEnvir)
            Call GetType(humanReadableFormatter).DoCall(AddressOf pushEnvir)
            Call GetType(time).DoCall(AddressOf pushEnvir)
            Call GetType(RCurl).DoCall(AddressOf pushEnvir)
            Call GetType(reshape2).DoCall(AddressOf pushEnvir)
            Call GetType(dplyr).DoCall(AddressOf pushEnvir)
        End Sub

        Private Sub New()
        End Sub

        ''' <summary>
        ''' load an internal package module
        ''' </summary>
        ''' <param name="baseModule"></param>
        Public Shared Sub pushEnvir(baseModule As Type)
            Call ImportsPackage _
               .GetAllApi(baseModule, includesInternal:=True) _
               .Select(Function(m)
                           Return New RMethodInfo(m)
                       End Function) _
               .DoEach(Sub(m)
                           If index.ContainsKey(m.name) Then
                               Throw New DuplicateNameException($"duplicated function with name: '{m.name}'!")
                           End If

                           Call index.Add(m.name, m)
                       End Sub)

            ' run main function
            Dim main As MethodInfo = RunDllEntryPoint.GetDllMainFunc(dll:=baseModule,)

            If Not main Is Nothing Then
                Call main.Invoke(Nothing, {})
            End If

            Call BinaryOperatorEngine.ImportsOperators(baseModule, Nothing)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function isRInternal(name As String) As Boolean
            Return name = "options" OrElse index.ContainsKey(name)
        End Function

        ''' <summary>
        ''' get all internal functions from the R# runtime environment base module
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function getAllInternals() As IEnumerable(Of RMethodInfo)
            Return index.Values
        End Function

        ''' <summary>
        ''' get function by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' this query function will returns nothing if the target symbol is not found
        ''' </returns>
        Friend Shared Function getFunction(name As String) As RMethodInfo
            If index.ContainsKey(name) Then
                Return index(name)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' In debug mode, will throw exception at the caller site
        ''' </summary>
        ''' <param name="message"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <DebuggerStepThrough>
        Public Shared Function [stop](message As Object, envir As Environment) As Message
            Return base.stop(message, envir)
        End Function

        Friend Shared Function missingParameter(funcName$, paramName$, envir As Environment) As Message
            Return [stop]({
                $"missing parameter '{paramName}' for function {funcName}",
                $"parameter: {paramName}",
                $"function: {funcName}"
            }, envir)
        End Function

        Friend Shared Function invalidParameter(message$, funcName$, paramName$, envir As Environment) As Message
            Return [stop]({
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
        Public Shared Function invokeInternals(envir As Environment, funcName$, paramVals As InvokeParameter()) As Object
            If index.ContainsKey(funcName) Then
                Return index(funcName).Invoke(envir, paramVals)
            Else
                Return Message.SymbolNotFound(envir, funcName, TypeCodes.closure)
            End If
        End Function
    End Class
End Namespace
