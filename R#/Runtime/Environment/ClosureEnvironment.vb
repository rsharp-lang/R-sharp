#Region "Microsoft.VisualBasic::8a151328dfb6ca8462fe76af20acccc2, R-sharp\R#\Runtime\Environment\ClosureEnvironment.vb"

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

    '   Total Lines: 41
    '    Code Lines: 22
    ' Comment Lines: 10
    '   Blank Lines: 9
    '     File Size: 1.38 KB


    '     Class ClosureEnvironment
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: FindSymbol
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime

    Public Class ClosureEnvironment : Inherits Environment

        ReadOnly closure_context As Environment

        Sub New(caller As Environment, closure_context As Environment)
            Call MyBase.New(caller, caller.stackFrame, isInherits:=False)

            Me.closure_context = closure_context
        End Sub

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
            Dim symbol As Symbol = closure_context.FindSymbol(name, [inherits])

            If symbol Is Nothing Then
                symbol = MyBase.FindSymbol(name, [inherits]:=False)
            End If

            Return symbol
        End Function
    End Class
End Namespace
