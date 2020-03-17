Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine

Module devStudio

    ''' <summary>
    ''' require(xxxx)
    ''' </summary>
    ''' <param name="script"></param>
    ''' <returns></returns>
    Public Function getRequiredPackages(script As String) As String()
        Dim program As Program = Program.BuildProgram(script)
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
            Case GetType(ValueAssign)
                Call getRequiredPackages(DirectCast(code, ValueAssign).value, packageNames, solveSymbols)
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

                For Each a As DeclareNewSymbol In newFunc.params
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
