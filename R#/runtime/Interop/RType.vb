#Region "Microsoft.VisualBasic::9cd673e18a103d9904f3d3cb41e4f4a6, R#\Runtime\Interop\RType.vb"

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

'     Class RType
' 
'         Properties: fullName, isArray, mode, raw
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface

Namespace Runtime.Interop

    Public Class RType : Implements IReflector

        Public ReadOnly Property fullName As String
            Get
                Return raw.FullName
            End Get
        End Property

        Public ReadOnly Property mode As TypeCodes
        Public ReadOnly Property isArray As Boolean
        Public ReadOnly Property raw As Type

        Dim names As String()

        Sub New(raw As Type)
            Me.raw = raw
            Me.isArray = raw.IsInheritsFrom(GetType(Array))
        End Sub

        Public Overrides Function ToString() As String
            Return $"<{mode.Description}> {raw.Name}"
        End Function

        Public Function getNames() As String() Implements IReflector.getNames
            Return names
        End Function
    End Class
End Namespace
