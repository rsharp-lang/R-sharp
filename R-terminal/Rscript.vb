#Region "Microsoft.VisualBasic::31597e5bdd581d6f83b4bd8df0688835, R-terminal\Rscript.vb"

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
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime
Imports RProgram = SMRUCC.Rsharp.Interpreter.Program

Module Rscript

    Dim echo As Index(Of String) = {"print", "cat", "echo", "q", "quit", "require", "library", "str"}

    Friend Function handleResult(result As Object, globalEnv As GlobalEnvironment, program As RProgram) As Integer
        Dim requirePrintErr As Boolean = False

        If RProgram.isException(result, globalEnv, isDotNETException:=requirePrintErr) Then
            If requirePrintErr Then
                Call REnv.Internal.debug.PrintMessageInternal(result)
            End If

            Return 500
        End If

        If program.EndWithFuncCalls(echo.Objects) Then
            ' do nothing
            Dim funcName As Literal = DirectCast(program.Last, FunctionInvoke).funcName

            If funcName = "cat" Then
                Call Console.WriteLine()
            End If
        ElseIf Not program.isValueAssign AndAlso Not program.isImports Then
            If Not isInvisible(result) Then
                Call base.print(result, globalEnv)
            End If
        End If

        Return 0
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
            Dim Rexp As Expression = program.First

            If TypeOf Rexp Is [Imports] OrElse TypeOf Rexp Is Require Then
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
        Return TypeOf program.Last Is ValueAssign OrElse TypeOf program.Last Is DeclareNewVariable
    End Function
End Module

