#Region "Microsoft.VisualBasic::8f54c1563ac3c96c825b3e7bd09d3072, R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\UsingClosure.vb"

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

    '   Total Lines: 103
    '    Code Lines: 70 (67.96%)
    ' Comment Lines: 19 (18.45%)
    '    - Xml Docs: 78.95%
    ' 
    '   Blank Lines: 14 (13.59%)
    '     File Size: 3.80 KB


    '     Class UsingClosure
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, GetSymbolName, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.Emit.Delegates
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Development.Package.File
Imports System.Runtime.CompilerServices

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' ```R
    ''' using xxx as creator() {
    ''' 
    ''' }
    ''' ```
    ''' </summary>
    ''' <remarks>
    ''' a subclass of <see cref="SymbolExpression"/>
    ''' </remarks>
    Public Class UsingClosure : Inherits SymbolExpression
        Implements IRuntimeTrace

        Public Overrides ReadOnly Property type As TypeCodes
            Get
                Return closure.type
            End Get
        End Property

        Public Overrides ReadOnly Property expressionName As ExpressionTypes
            Get
                Return ExpressionTypes.Using
            End Get
        End Property

        Public ReadOnly Property stackFrame As StackFrame Implements IRuntimeTrace.stackFrame

        Friend ReadOnly params As DeclareNewSymbol
        Friend ReadOnly closure As ClosureExpression

        ''' <summary>
        ''' using params do closure
        ''' </summary>
        ''' <param name="params">using x as value</param>
        ''' <param name="closure"></param>
        ''' <param name="stackframe"></param>
        Sub New(params As DeclareNewSymbol, closure As ClosureExpression, stackframe As StackFrame)
            Me.params = params
            Me.closure = closure
            Me.stackFrame = stackframe
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetSymbolName() As String
            Return params.GetSymbolName
        End Function

        Public Overrides Function Evaluate(envir As Environment) As Object
            Using env As New Environment(envir, stackFrame, isInherits:=False)
                Dim resource As Object = params.Evaluate(env)
                Dim result As Object

                If resource Is Nothing Then
                    Return Internal.debug.stop("Target is nothing!", env)
                ElseIf Program.isException(resource) Then
                    Return resource
                ElseIf Not resource.GetType.ImplementInterface(GetType(IDisposable)) Then
                    Return Message.InCompatibleType(GetType(IDisposable), resource.GetType, env)
                End If

                Try
                    ' run using closure and get result
                    result = closure.Evaluate(env)
                Catch ex As Exception
                    Return Internal.debug.stop({
                        $"error while evaluate the closure expression: {ex.Message}",
                        $"runtime exception: {ex.GetType.Name}"
                    }, env)
                End Try

                Try
                    ' then dispose the target
                    ' release handle and clean up the resource
                    Call DirectCast(resource, IDisposable).Dispose()
                Catch ex As Exception
                    Return Internal.debug.stop({
                        $"error while dispose target resource automatically: {ex.Message}",
                        $"runtime exception: {ex.GetType.Name}"
                    }, env)
                End Try

                Return result
            End Using
        End Function

        Public Overrides Function ToString() As String
            Return $"using {params} {{
    # using closure internal
    {closure}
}}"
        End Function
    End Class
End Namespace
