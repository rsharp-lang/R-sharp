#Region "Microsoft.VisualBasic::1a58303c841a697a134caa11332324c1, R#\Interpreter\ExecuteEngine\Linq\ExecutableContext.vb"

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

    '     Class ExecutableContext
    ' 
    '         Properties: stackFrame, symbolNames
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FindSymbol, ToString
    ' 
    '         Sub: AddSymbol, SetSymbol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Interpreter.ExecuteEngine.LINQ

    Public Class ExecutableContext

        ReadOnly environment As Environment

        Public ReadOnly Property stackFrame As StackFrame
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return environment.stackFrame
            End Get
        End Property

        Public ReadOnly Property symbolNames As String()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return environment.GetSymbolsNames.ToArray
            End Get
        End Property

        <DebuggerStepThrough>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(env As Environment)
            environment = env
            environment.isLINQContext = True
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AddSymbol(symbolName$, type As TypeCodes)
            Call environment.Push(symbolName, Nothing, [readonly]:=False, mode:=type)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindSymbol(symbolName As String) As Symbol
            Return environment.FindSymbol(symbolName)
        End Function

        ''' <summary>
        ''' set <paramref name="value"/> to target symbol <paramref name="name"/>.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetSymbol(name As String, value As Object)
            Call environment.FindSymbol(name).SetValue(value, Me)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return environment.ToString
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Narrowing Operator CType(context As ExecutableContext) As Environment
            Return context.environment
        End Operator
    End Class
End Namespace
