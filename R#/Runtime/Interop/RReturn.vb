#Region "Microsoft.VisualBasic::0235b13fa84acafe93b558832d2ac0a3, R-sharp\R#\Runtime\Interop\RReturn.vb"

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

    '   Total Lines: 60
    '    Code Lines: 42
    ' Comment Lines: 7
    '   Blank Lines: 11
    '     File Size: 1.94 KB


    '     Class RReturn
    ' 
    '         Properties: HasValue, invisible, isError, messages, type
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime.Interop

    ''' <summary>
    ''' VB.NET api to R# function invoke result wrapper, with additional warning, debug, errors messages, etc
    ''' </summary>
    Public Class RReturn : Inherits Value(Of Object)

        Public Property messages As New List(Of Message)

        Public ReadOnly Property type As RType

        ''' <summary>
        ''' Change the Print Mode to Invisible
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property invisible As Boolean

        Public ReadOnly Property isError As Boolean
            Get
                Return messages.Any(Function(msg) msg.level = MSG_TYPES.ERR)
            End Get
        End Property

        Public Overrides ReadOnly Property HasValue As Boolean
            Get
                If Value Is Nothing Then
                    Return Not messages.IsNullOrEmpty
                Else
                    Return True
                End If
            End Get
        End Property

        Private Sub New()
        End Sub

        Sub New(value As Object, Optional invisible As Boolean = False)
            Me.Value = value
            Me.invisible = invisible

            If Not value Is Nothing Then
                Me.type = RType.GetRSharpType(value.GetType)
            Else
                Me.type = RType.GetRSharpType(GetType(Void))
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Widening Operator CType(msg As Message) As RReturn
            Return New RReturn With {
                .messages = New List(Of Message) From {msg}
            }
        End Operator
    End Class
End Namespace
