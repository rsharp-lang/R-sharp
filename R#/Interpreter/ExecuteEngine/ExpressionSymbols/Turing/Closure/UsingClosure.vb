#Region "Microsoft.VisualBasic::f296da975d323132d645f43f6badccfa, R-sharp\R#\Interpreter\ExecuteEngine\ExpressionSymbols\Turing\Closure\UsingClosure.vb"

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

    '   Total Lines: 80
    '    Code Lines: 51
    ' Comment Lines: 16
    '   Blank Lines: 13
    '     File Size: 2.82 KB


    '     Class UsingClosure
    ' 
    '         Properties: expressionName, stackFrame, type
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Evaluate, ToString
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

Namespace Interpreter.ExecuteEngine.ExpressionSymbols.Closure

    ''' <summary>
    ''' ```R
    ''' using xxx as creator() {
    ''' 
    ''' }
    ''' ```
    ''' </summary>
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

                ' run using closure and get result
                result = closure.Evaluate(env)

                ' then dispose the target
                ' release handle and clean up the resource
                Call DirectCast(resource, IDisposable).Dispose()

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
