#Region "Microsoft.VisualBasic::c6aa44f23570dd488c8159813efd13cd, G:/GCModeller/src/R-sharp/R#//Interpreter/ExecuteEngine/ExpressionSymbols/DataSet/NamespaceFunctionSymbolReference.vb"

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

    '   Total Lines: 137
    '    Code Lines: 100
    ' Comment Lines: 15
    '   Blank Lines: 22
    '     File Size: 5.65 KB


    '     Class NamespaceFunctionSymbolReference
    ' 
    '         Properties: [namespace], expressionName, stackFrame, symbol, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, getFuncNameSymbolText, getPackageApiImpl, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports RPkg = SMRUCC.Rsharp.Development.Package.Package

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.DataSets

    ''' <summary>
    ''' ``namespace::function(xxx)``
    ''' </summary>
    Public Class NamespaceFunctionSymbolReference : Inherits Expression
        Implements IRuntimeTrace

        Public ReadOnly Property [namespace] As String
        Public ReadOnly Property symbol As Expression

        Public Overrides ReadOnly Property type As TypeCodes
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return TypeCodes.closure
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return ExpressionTypes.SymbolNamespaceReference
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Public Sub New(namespace$, symbol As Expression, stackFrame As StackFrame)
            Me.symbol = symbol
            Me.namespace = [namespace]
            Me.stackFrame = stackFrame
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Evaluate(envir As Environment) As Object
            Return getPackageApiImpl(envir, [namespace], symbol)
        End Function

        Friend Shared Function getFuncNameSymbolText(funcNameSymbol As Expression, env As Environment) As Object
            Select Case funcNameSymbol.GetType
                Case GetType(Literal)
                    Return DirectCast(funcNameSymbol, Literal).value
                Case GetType(SymbolReference)
                    Return DirectCast(funcNameSymbol, SymbolReference).symbol
                Case Else
                    Return Message.InCompatibleType(GetType(Literal), funcNameSymbol.GetType, env)
            End Select
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="namespace$"></param>
        ''' <param name="funcNameSymbol"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' find in runtime environment at first
        ''' and then installed packages
        ''' </remarks>
        Friend Shared Function getPackageApiImpl(env As Environment, namespace$, funcNameSymbol As Expression) As Object
            Dim funcName As Object = getFuncNameSymbolText(funcNameSymbol, env)

            If TypeOf funcName Is Message Then
                Return funcName
            End If

            Dim globalEnv As GlobalEnvironment = env.globalEnvironment
            Dim method As Symbol = globalEnv.FindFunction($"{[namespace]}::{funcName}")

            If method IsNot Nothing AndAlso method.value IsNot Nothing Then
                If method.value.GetType.ImplementInterface(Of INamespaceReferenceSymbol) Then
                    If DirectCast(method.value, INamespaceReferenceSymbol).namespace = [namespace] Then
                        Return method.value
                    End If
                End If
            End If

            ' find package and then load method
            Dim [error] As Exception = Nothing
            Dim pkg As RPkg = globalEnv.packages.FindPackage([namespace], env.globalEnvironment, [error])

            If Not [error] Is Nothing Then
                Return Internal.debug.stop([error], env)
            End If

            If pkg Is Nothing Then
                Dim nsText As String
                Dim fsymbol As Symbol = env.FindFunction(funcName)

                If fsymbol Is Nothing Then
                    Return Internal.debug.stop({
                        $"we can not found any namespace called: '{[namespace]}'!",
                        $"namespace: {[namespace]}",
                        $"function symbol: {funcNameSymbol}<{fsymbol}>"
                    }, env)
                End If

                nsText = DirectCast(fsymbol.value, INamespaceReferenceSymbol).namespace

                If nsText <> [namespace] Then
                    Return Internal.debug.stop({
                        $"we can not found any namespace called: '{[namespace]}'!",
                        $"namespace: {[namespace]}",
                        $"function symbol: {funcNameSymbol}<{funcName}>"
                    }, env)
                End If

                Return fsymbol.value
            Else
                Dim funcNameText As String = funcName.ToString
                Dim api As RMethodInfo = pkg.GetFunction(funcNameText)

                If api Is Nothing Then
                    Return Message.SymbolNotFound(env, $"{[namespace]}::{funcNameText}", TypeCodes.closure)
                Else
                    Return DirectCast(api, RFunction)
                End If
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"&{[namespace]}::<{symbol.ToString}>"
        End Function
    End Class
End Namespace
