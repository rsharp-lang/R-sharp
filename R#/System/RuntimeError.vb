#Region "Microsoft.VisualBasic::034e13438324333d58e2f4ca67846d94, R-sharp\R#\System\RuntimeError.vb"

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

    '   Total Lines: 46
    '    Code Lines: 34
    ' Comment Lines: 3
    '   Blank Lines: 9
    '     File Size: 1.43 KB


    '     Class RuntimeError
    ' 
    '         Properties: StackTrace
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetMessages, SafeGetSource
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Debugging
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Development

    Public Class RuntimeError : Inherits VisualBasicAppException

        ''' <summary>
        ''' the stack trace of R# script
        ''' </summary>
        Dim _stackTrace As String

        Public Overrides ReadOnly Property StackTrace As String
            Get
                Return _stackTrace
            End Get
        End Property

        Public Sub New(message As Message)
            MyBase.New(GetMessages(message).JoinBy("; "), caller:=SafeGetSource(message))

            Me._stackTrace = message.environmentStack _
                .SafeQuery _
                .Select(Function(t) t.ToString) _
                .JoinBy(vbCrLf)
        End Sub

        Private Shared Function SafeGetSource(msg As Message) As String
            If msg.source Is Nothing Then
                Return "<Unknown>"
            Else
                Return msg.source.ToString
            End If
        End Function

        Private Shared Iterator Function GetMessages(msg As Message) As IEnumerable(Of String)
            Dim i As i32 = 1

            For Each line As String In msg
                Yield $"{++i}. {line}"
            Next
        End Function
    End Class
End Namespace
