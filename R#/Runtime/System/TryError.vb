#Region "Microsoft.VisualBasic::4a6834ed9351ba3a67fe6d3552802d04, D:/GCModeller/src/R-sharp/R#//Runtime/System/TryError.vb"

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

    '   Total Lines: 39
    '    Code Lines: 26
    ' Comment Lines: 7
    '   Blank Lines: 6
    '     File Size: 1.24 KB


    '     Class TryError
    ' 
    '         Properties: [error], stackframe, stacktrace
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString, traceback
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components

    ''' <summary>
    ''' try-error
    ''' </summary>
    Public Class TryError

        Public ReadOnly Property [error] As String()
        ''' <summary>
        ''' the error location
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property stackframe As StackFrame
        Public ReadOnly Property stacktrace As StackFrame()

        Sub New(err As Message, stackframe As StackFrame)
            Me.error = err.message
            Me.stackframe = stackframe
            Me.stacktrace = err.environmentStack
        End Sub

        Public Function traceback() As vector
            Return New vector With {
                .data = stacktrace _
                    .Select(Function(line) line.ToString) _
                    .ToArray,
                .elementType = RType.GetRSharpType(GetType(String))
            }
        End Function

        Public Overrides Function ToString() As String
            Return [error].JoinBy("; ")
        End Function
    End Class
End Namespace
