#Region "Microsoft.VisualBasic::5b8e07da98cd465ed6afb2fecd79cad1, R#\Runtime\GlobalEnvironment.vb"

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

'     Class GlobalEnvironment
' 
'         Properties: options, packages
' 
'         Constructor: (+1 Overloads) Sub New
' 
' 
' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports SMRUCC.Rsharp.Runtime.Package

Namespace Runtime

    ''' <summary>
    ''' R#之中的全局环境对象
    ''' </summary>
    Public Class GlobalEnvironment : Inherits Environment

        Public ReadOnly Property options As Options
        Public ReadOnly Property packages As PackageManager

        Sub New(options As Options)
            Me.options = options
            Me.packages = New PackageManager(options)
            Me.global = Me
        End Sub

    End Class
End Namespace
