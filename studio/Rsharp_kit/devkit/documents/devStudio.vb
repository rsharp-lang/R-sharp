#Region "Microsoft.VisualBasic::b32bcbcec00728f553ad504fb356b0e3, studio\Rsharp_kit\devkit\documents\devStudio.vb"

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

    '   Total Lines: 94
    '    Code Lines: 67 (71.28%)
    ' Comment Lines: 6 (6.38%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 21 (22.34%)
    '     File Size: 4.23 KB


    ' Module devStudio
    ' 
    '     Function: getRequiredPackages
    ' 
    '     Sub: getRequiredPackages
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Blocks
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators

Module devStudio

    ''' <summary>
    ''' require(xxxx)
    ''' </summary>
    ''' <param name="script"></param>
    ''' <returns></returns>
    Public Function getRequiredPackages(script As String) As String()
        Dim program As SMRUCC.Rsharp.Interpreter.Program = SMRUCC.Rsharp.Interpreter.Program.BuildProgram(script)
        Dim list As New List(Of String)
        Dim symbols As New List(Of SymbolReference)

        For Each line As Expression In program
            Call getRequiredPackages(line, packageNames:=list, solveSymbols:=symbols)
        Next

        Return list.ToArray
    End Function

    Private Sub getRequiredPackages(code As Expression, packageNames As List(Of String), solveSymbols As List(Of SymbolReference))
        Select Case code.GetType
            Case GetType(Require)
                For Each name As Expression In DirectCast(code, Require).packages
                    If TypeOf name Is Literal Then
                        packageNames.Add(DirectCast(name, Literal).ToString)
                    ElseIf TypeOf name Is SymbolReference Then
                        solveSymbols.Add(name)
                    Else
                        ' ignores
                    End If
                Next
            Case GetType(ValueAssignExpression)
                Call getRequiredPackages(DirectCast(code, ValueAssignExpression).value, packageNames, solveSymbols)
            Case GetType(MemberValueAssign)
                Call getRequiredPackages(DirectCast(code, MemberValueAssign).value, packageNames, solveSymbols)
            Case GetType(FunctionInvoke)
                Dim funcCalls As FunctionInvoke = DirectCast(code, FunctionInvoke)

                For Each arg As Expression In funcCalls.EnumerateInvokedParameters
                    Call getRequiredPackages(arg, packageNames, solveSymbols)
                Next
            Case GetType(DeclareLambdaFunction)
                Call getRequiredPackages(DirectCast(code, DeclareLambdaFunction).closure, packageNames, solveSymbols)
            Case GetType(DeclareNewFunction)
                Dim newFunc As DeclareNewFunction = DirectCast(code, DeclareNewFunction)

                For Each a As DeclareNewSymbol In newFunc.parameters
                    Call getRequiredPackages(a, packageNames, solveSymbols)
                Next

                Call getRequiredPackages(newFunc.body, packageNames, solveSymbols)

            Case GetType(ClosureExpression)

                Dim closure As ClosureExpression = DirectCast(code, ClosureExpression)

                For Each line As Expression In closure.EnumerateCodeLines
                    Call getRequiredPackages(line, packageNames, solveSymbols)
                Next

            Case GetType(IfBranch)

                Dim ifClosure As IfBranch = DirectCast(code, IfBranch)

                Call getRequiredPackages(ifClosure.ifTest, packageNames, solveSymbols)
                Call getRequiredPackages(ifClosure.trueClosure, packageNames, solveSymbols)

            Case GetType(ForLoop)

                Dim forLoop As ForLoop = DirectCast(code, ForLoop)

                Call getRequiredPackages(forLoop.sequence, packageNames, solveSymbols)
                Call getRequiredPackages(forLoop.body, packageNames, solveSymbols)

            Case GetType(DeclareNewSymbol)

                Dim letSymbol As DeclareNewSymbol = DirectCast(code, DeclareNewSymbol)

                If letSymbol.hasInitializeExpression Then
                    Call getRequiredPackages(letSymbol.value, packageNames, solveSymbols)
                End If

            Case Else
                Throw New NotImplementedException(code.GetType.FullName)
        End Select
    End Sub
End Module
