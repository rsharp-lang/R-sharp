#Region "Microsoft.VisualBasic::057cab07df42cba3d415dd8506e7f669, D:/GCModeller/src/R-sharp/R#//Runtime/System/RsharpDataObject.vb"

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

'   Total Lines: 26
'    Code Lines: 18
' Comment Lines: 3
'   Blank Lines: 5
'     File Size: 762 B


'     Class RsharpDataObject
' 
'         Properties: elementType
' 
'         Function: ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Components

    Public Interface IAttributeReflector

        ''' <summary>
        ''' Get all attribute name that tagged with current symbol object.
        ''' </summary>
        ''' <returns></returns>
        Function getAttributeNames() As IEnumerable(Of String)

    End Interface

    ''' <summary>
    ''' the R# data object with specific element data type
    ''' </summary>
    Public MustInherit Class RsharpDataObject : Implements IAttributeReflector

        Protected m_type As RType = RType.any

        ''' <summary>
        ''' holds the custom attribute data that tagged with current R# runtime object
        ''' this dictionary object value is created in lazy mode
        ''' </summary>
        Protected m_attributes As Dictionary(Of String, Object)

        Public Overridable Property elementType As RType
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return m_type
            End Get
            Protected Friend Set(value As RType)
                m_type = value
            End Set
        End Property

        Public Function getAttribute(name As String, Optional [default] As Object = Nothing) As Object
            If m_attributes Is Nothing Then
                Return [default]
            End If

            If m_attributes.ContainsKey(name) Then
                Return m_attributes(name)
            Else
                Return [default]
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub setAttribute(name As String, val As Object)
            If m_attributes Is Nothing Then
                m_attributes = New Dictionary(Of String, Object)
            End If

            m_attributes(name) = val
        End Sub

        ''' <summary>
        ''' Get all attribute name that tagged with current symbol object.
        ''' </summary>
        ''' <returns></returns>
        Public Function getAttributeNames() As IEnumerable(Of String) Implements IAttributeReflector.getAttributeNames
            If m_attributes Is Nothing Then
                Return New String() {}
            Else
                Return m_attributes.Keys
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"{MyClass.GetType.Name}<{elementType.ToString}>"
        End Function
    End Class
End Namespace
