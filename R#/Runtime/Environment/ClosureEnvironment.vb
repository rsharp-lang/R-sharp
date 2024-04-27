#Region "Microsoft.VisualBasic::9b484824cf93708decd6f244c5c6fd6a, G:/GCModeller/src/R-sharp/R#//Runtime/Environment/ClosureEnvironment.vb"

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

    '   Total Lines: 85
    '    Code Lines: 49
    ' Comment Lines: 22
    '   Blank Lines: 14
    '     File Size: 3.25 KB


    '     Class ClosureEnvironment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FindFunction, FindSymbol, renameFrame
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes.LinqPipeline

Namespace Runtime

    ''' <summary>
    ''' A local environment of the function closure
    ''' </summary>
    Public Class ClosureEnvironment : Inherits Environment

        ReadOnly closure_context As Environment

        Sub New(caller As Environment, frame As StackFrame, closure_context As Environment)
            Call MyBase.New(
                parent:=caller,
                stackFrame:=frame,
                isInherits:=False
            )

            Me.closure_context = closure_context
        End Sub

        Public Shared Function renameFrame(frame As StackFrame, invokeName As String) As StackFrame
            Return New StackFrame With {
                .File = frame.File,
                .Line = frame.Line,
                .Method = New Method With {
                    .Method = invokeName,
                    .[Module] = frame.Method.Module,
                    .[Namespace] = frame.Method.Namespace
                }
            }
        End Function

        Public Overrides Function FindFunction(name As String, Optional [inherits] As Boolean = True) As Symbol
            ' found on local environment at first
            Dim symbol As Symbol = funcSymbols.TryGetValue(name)

            ' then found function in the closure environment
            If symbol Is Nothing Then
                symbol = closure_context.FindFunction(name, [inherits]:=[inherits])
            End If
            ' at last, found symbol in global
            If symbol Is Nothing Then
                symbol = MyBase.FindFunction(name, [inherits])
            End If

            Return symbol
        End Function

        ''' <summary>
        ''' find in closure internal context at first 
        ''' and then find on the parent context
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="[inherits]"></param>
        ''' <returns>
        ''' this function returns nothing if the target symbol 
        ''' is not found in the environment context
        ''' </returns>
        Public Overrides Function FindSymbol(name As String, Optional [inherits] As Boolean = True) As Symbol
            ' found on local environment at first
            Dim symbol As Symbol = Me.symbols.TryGetValue(name)

            ' then found symbol in the closure environment
            If symbol Is Nothing Then
                symbol = closure_context.FindSymbol(name, [inherits])
            End If
            ' at last, found symbol in global
            If symbol Is Nothing Then
                Dim findInParent As Boolean = False

                ' 20221217 make an exception for the parallel task progress report
                findInParent = findInParent OrElse name = parallelApplys.ParallelTaskWorkerSymbol
                ' make another exceptions?
                ' ...

                symbol = MyBase.FindSymbol(name, [inherits]:=findInParent)
            End If

            Return symbol
        End Function
    End Class
End Namespace
