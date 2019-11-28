#Region "Microsoft.VisualBasic::d46153d3fd882b807ae2a1a4e48b1798, R#\Runtime\Package\Package.vb"

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

'     Class Package
' 
'         Properties: [namespace], info, package
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Package

    Public Class Package

        Public Property info As PackageAttribute
        Public Property package As Type

        Public ReadOnly Property [namespace] As String
            Get
                Return info.Namespace
            End Get
        End Property

        Sub New(info As PackageAttribute, package As Type)
            Me.info = info
            Me.package = package
        End Sub

        Public Function GetFunction(apiName As String) As RMethodInfo

        End Function

        Public Overrides Function ToString() As String
            Return $"{info.Namespace}: {info.Description}"
        End Function
    End Class
End Namespace
