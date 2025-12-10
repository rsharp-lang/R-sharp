#Region "Microsoft.VisualBasic::17dc8e16f6b25a2e3ceebbda7148d495, R#\Runtime\Internal\objects\dataset\vbObject.vb"

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

'   Total Lines: 269
'    Code Lines: 196 (72.86%)
' Comment Lines: 40 (14.87%)
'    - Xml Docs: 90.00%
' 
'   Blank Lines: 33 (12.27%)
'     File Size: 11.27 KB


'     Class vbObject
' 
'         Properties: target, type
' 
'         Constructor: (+2 Overloads) Sub New
'         Function: [TryCast], CreateInstance, existsName, (+2 Overloads) getByName, getNames
'                   getObjMethods, getObjProperties, propertyParserInternal, (+2 Overloads) setByName, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].baseOp
Imports SMRUCC.Rsharp.Runtime.Internal.Object.Converts

Namespace Runtime.Internal.Object

    ''' <summary>
    ''' Proxy for VB.NET class <see cref="Object"/>
    ''' </summary>
    Public Class vbObject : Inherits s4Reflector
        Implements RNameIndex

        ''' <summary>
        ''' that target clr object to wrap in R# runtime.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property target As Object

        Sub New(obj As Object)
            target = obj
            elementType = MakeReflection(obj, properties, methods)
        End Sub

        Public Function [TryCast](Of T)() As T
            Return DirectCast(target, T)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function getNames() As String() Implements IReflector.getNames
            If m_type.haveDynamicsProperty Then
                Return m_type.getNames + DirectCast(target, IDynamicsObject).GetNames.AsList
            Else
                Return m_type.getNames
            End If
        End Function

        Public Function existsName(name As String) As Boolean
            If properties.ContainsKey(name) Then
                Return True
            ElseIf methods.ContainsKey(name) Then
                Return True
            ElseIf m_type.haveDynamicsProperty Then
                Return DirectCast(target, IDynamicsObject).HasName(name)
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' Get property value/method reference by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' Function will returns nothing when target not found, but 
        ''' property value read getter result may be nothing, please 
        ''' check member exists or not by method <see cref="existsName"/> 
        ''' if the result value is nothing of this function.
        ''' </returns>
        Public Overrides Function getByName(name As String) As Object Implements RNameIndex.getByName
            If properties.ContainsKey(name) Then
                Return properties(name).GetValue(target)
            ElseIf methods.ContainsKey(name) Then
                Return methods(name)
            ElseIf m_type.haveDynamicsProperty Then
                Return DirectCast(target, IDynamicsObject).GetItemValue(name)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' set property value by name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="value"></param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Overrides Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            If properties.ContainsKey(name) Then
                If properties(name).CanWrite Then
                    Dim targetType As Type = properties(name).PropertyType

                    If targetType.IsArray Then
                        value = asVector(value, targetType.GetElementType, envir)
                    Else
                        value = RCType.CTypeDynamic(value, targetType, envir)
                    End If

                    Call properties(name).SetValue(target, value)
                Else
                    Return Internal.debug.stop($"Target property '{name}' is not writeable!", envir)
                End If
            ElseIf m_type.haveDynamicsProperty Then
                Call DirectCast(target, IDynamicsObject).SetValue(name, value)
            Else
                Return Internal.debug.stop($"Missing property '{name}'", envir)
            End If

            Return value
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            If target Is Nothing Then
                Return "NULL"
            ElseIf printer.RtoString.ContainsKey(target.GetType) Then
                Return printer.RtoString(target.GetType)(target)
            Else
                Return target.ToString
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function CreateInstance(type As Type) As vbObject
            Return New vbObject(Activator.CreateInstance(type))
        End Function
    End Class
End Namespace
