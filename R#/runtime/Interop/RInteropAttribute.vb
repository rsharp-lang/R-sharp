#Region "Microsoft.VisualBasic::c18e98d69331762107ed67c79995ff79, R#\Runtime\Interop\RInteropAttribute.vb"

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

    '     Class RInteropAttribute
    ' 
    ' 
    ' 
    '     Class RListObjectArgumentAttribute
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Runtime.Interop

    <AttributeUsage(AttributeTargets.All, AllowMultiple:=False, Inherited:=True)>
    Public Class RInteropAttribute : Inherits Attribute
    End Class

    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RListObjectArgumentAttribute : Inherits RInteropAttribute

    End Class
End Namespace
