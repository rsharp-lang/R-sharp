#Region "Microsoft.VisualBasic::38e36721d070450c0b2759bc3f99429b, R#\Runtime\Internal\invoke.vb"

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
    '         Function: [stop], getFunction, invalidParameter, invokeInternals, missingParameter
    ' 
    '         Sub: pushEnvir
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.System.Package

Namespace Runtime.Internal

    ''' <summary>
    ''' The R internal function invoke helper module
    ''' </summary>
    Module invoke

        ''' <summary>
        ''' 内部函数索引
        ''' </summary>
        ReadOnly index As New Dictionary(Of String, RMethodInfo)

        Sub New()
            Call GetType(base).pushEnvir
            Call GetType(RConversion).pushEnvir
            Call GetType(env).pushEnvir
            Call GetType(linq).pushEnvir
            Call GetType(Invokes.file).pushEnvir
            Call GetType(stringr).pushEnvir
            Call GetType(strings).pushEnvir
            Call GetType(utils).pushEnvir
            Call GetType(math).pushEnvir
            Call GetType(help).pushEnvir
            Call GetType(etc).pushEnvir
            Call GetType([set]).pushEnvir
        End Sub

        <Extension>
        Private Sub pushEnvir(baseModule As Type)
            Call ImportsPackage _
                .GetAllApi(baseModule, includesInternal:=True) _
                .Select(Function(m)
                            Return New RMethodInfo(m)
                        End Function) _
                .DoEach(Sub(m)
                            Call index.Add(m.name, m)
                        End Sub)
        End Sub

        Friend Function getFunction(name As String) As RMethodInfo
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
        Public Function [stop](message As Object, envir As Environment) As Message
            Return base.stop(message, envir)
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
        Public Function invokeInternals(envir As Environment, funcName$, paramVals As InvokeParameter()) As Object
            If index.ContainsKey(funcName) Then
                Return index(funcName).Invoke(envir, paramVals)
            Else
                Return Message.SymbolNotFound(envir, funcName, TypeCodes.closure)
            End If
        End Function
    End Module
End Namespace
