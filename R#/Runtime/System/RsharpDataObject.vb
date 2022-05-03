#Region "Microsoft.VisualBasic::d521a84e9e411ce47709a4c4772f3c2a, R-sharp\R#\Runtime\System\RsharpDataObject.vb"

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

'   Total Lines: 31
'    Code Lines: 23
' Comment Lines: 3
'   Blank Lines: 5
'     File Size: 866.00 B


'     Class RsharpDataObject
' 
'         Properties: elementType
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components

    ''' <summary>
    ''' the R# data object with specific element data type
    ''' </summary>
    Public MustInherit Class RsharpDataObject

        Protected m_type As RType = RType.any

        Public Overridable Property elementType As RType
            Get
                Return m_type
            End Get
            Protected Friend Set(value As RType)
                m_type = value
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return $"{MyClass.GetType.Name}<{elementType.ToString}>"
        End Function
    End Class
End Namespace
