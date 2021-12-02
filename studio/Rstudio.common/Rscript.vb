#Region "Microsoft.VisualBasic::5587c32bd3b94d4e2a57af14d81cbcf4, studio\Rstudio.common\Rscript.vb"

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

    ' Module Rscript
    ' 
    '     Function: handleResult, isImports, isInvisible, isValueAssign
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Rscript

    Dim echo As Index(Of String) = {"print", "cat", "echo", "q", "quit", "require", "library", "str"}

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="result"></param>
    ''' <param name="globalEnv"></param>
    ''' <param name="program">
    ''' function will not echo of the last result value 
    ''' if this program parameter value is missing
    ''' </param>
    ''' <returns></returns>
    Friend Function handleResult(result As Object, globalEnv As GlobalEnvironment, Optional program As RProgram = Nothing) As Integer
        Dim requirePrintErr As Boolean = False
        Dim code As Integer = 0

        If RProgram.isException(result, globalEnv, isDotNETException:=requirePrintErr) Then
            Call REnv.Internal.debug.PrintMessageInternal(DirectCast(result, Message), globalEnv)
            code = 500
            GoTo FINAL
        End If

        If Not program Is Nothing Then
            If program.EndWithFuncCalls(echo.Objects) Then
                ' do nothing
                Dim funcName As Literal = DirectCast(DirectCast(program.Last, FunctionInvoke).funcName, Literal)

                If funcName = "cat" Then
                    Call Console.WriteLine()
                End If
            ElseIf program.Count = 0 Then
                ' do nothing 
            ElseIf Not program.isValueAssign AndAlso Not program.isImports Then
                If Not isInvisible(result) Then
                    Call base.print(result, , globalEnv)
                End If
            End If
        End If
FINAL:
        If globalEnv.messages > 0 Then
            Call REnv.Internal.debug.PrintWarningMessages(globalEnv.messages, globalEnv)
            Call globalEnv.messages.Clear()
        End If

        Return code
    End Function

    Private Function isInvisible(result As Object) As Boolean
        If result Is Nothing Then
            Return False
        ElseIf result.GetType Is GetType(RReturn) Then
            Return DirectCast(result, RReturn).invisible
        ElseIf result.GetType Is GetType(invisible) Then
            Return True
        Else
            Return False
        End If
    End Function

    <DebuggerStepThrough>
    <Extension>
    Private Function isImports(program As RProgram) As Boolean
        If program.Count <> 1 Then
            Return False
        Else
            Dim rexp As Expression = program.First

            If TypeOf rexp Is [Imports] OrElse TypeOf rexp Is Require Then
                Return True
            Else
                Return False
            End If
        End If
    End Function

    <DebuggerStepThrough>
    <Extension>
    Private Function isValueAssign(program As RProgram) As Boolean
        ' 如果是赋值表达式的话，也不会在终端上打印结果值
        Return TypeOf program.Last Is ValueAssignExpression OrElse TypeOf program.Last Is DeclareNewSymbol
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Friend Function FromFile(script As String) As Components.Rscript
        Return SMRUCC.Rsharp.Runtime.Components.Rscript.FromFile(script)
    End Function
End Module
