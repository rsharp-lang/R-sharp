#Region "Microsoft.VisualBasic::e93ec41b694af8c55e179028fc5eab0a, snowFall\Context\RPC\Protocols.vb"

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

'     Enum Protocols
' 
'         GetSymbol
' 
'  
' 
' 
' 
' 
' /********************************************************************************/

#End Region


Imports System.IO
Imports Microsoft.VisualBasic.Serialization

Namespace Context.RPC

    Public Enum Protocols
        Initialize
        GetSymbol
        PushResult
    End Enum

    Public Class GetSymbol : Inherits RawStream

        Public Property uuid As Integer
        Public Property name As String

        Sub New()
        End Sub

        Sub New(payload As Byte())

        End Sub

        Public Overrides Sub Serialize(buffer As Stream)
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace
