#Region "Microsoft.VisualBasic::e5920091921b775b35d4634f2acec2d6, R#\Runtime\Internal\objects\base\s4Methods.vb"

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

    '   Total Lines: 102
    '    Code Lines: 82 (80.39%)
    ' Comment Lines: 2 (1.96%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 18 (17.65%)
    '     File Size: 4.21 KB


    '     Module s4Methods
    ' 
    '         Function: conversionValue, (+2 Overloads) createObject, defineObject
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.[Object].Converts
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Object.baseOp

    Public Module s4Methods

        <Extension>
        Public Function defineObject(type As S4Object, env As GlobalEnvironment) As Type
            Dim def As New DynamicType(base:=GetType(s4Reflector))
            Dim valueType As Type

            For Each slot As KeyValuePair(Of String, String) In type.slots
                Select Case Microsoft.VisualBasic.Strings.Trim(slot.Value).ToLower
                    Case "numeric", "double" : valueType = GetType(Double())
                    Case "integer" : valueType = GetType(Integer())
                    Case "character" : valueType = GetType(String())
                    Case "raw" : valueType = GetType(Byte())
                    Case "list" : valueType = GetType(list)
                    Case "float" : valueType = GetType(Single())
                    Case "long" : valueType = GetType(Long())
                    Case Else
                        valueType = env.types(slot.Value).raw
                End Select

                Call def.Add(slot.Key, valueType)
            Next

            Return def.Create(type.class_name, asm_module:="r_sharp_s4object").GeneratedType
        End Function

        <Extension>
        Public Function createObject(type As RType, values As list, env As Environment) As Object
            Dim obj As Object = Activator.CreateInstance(type.raw)
            Dim val As Object
            Dim reflection = DataFramework.Schema(type.raw, PropertyAccess.Writeable, PublicProperty, nonIndex:=True)
            Dim writer As PropertyInfo

            For Each slot As KeyValuePair(Of String, Object) In values.slots
                writer = reflection(slot.Key)
                val = slot.Value
                val = RCType.CTypeDynamic(val, writer.PropertyType, env)

                If TypeOf val Is Message Then
                    Return val
                End If

                Call writer.SetValue(obj, val)
            Next

            Return obj
        End Function

        <Extension>
        Public Function createObject(s4class As S4Object, values As list, env As Environment) As Object
            Dim obj As Object = Activator.CreateInstance(s4class.raw)
            Dim val As Object

            For Each slot As KeyValuePair(Of String, String) In s4class.slots
                If values.hasName(slot.Key) Then
                    val = values.getByName(slot.Key)
                Else
                    ' use the default value
                    If s4class.prototype.ContainsKey(slot.Key) Then
                        val = s4class.prototype(slot.Key)
                    Else
                        Continue For
                    End If
                End If

                val = s4Methods.conversionValue(val, slot.Value, env)
                s4class.reflection(slot.Key).SetValue(obj, val)
            Next

            DirectCast(obj, s4Reflector).s4class = s4class

            Return obj
        End Function

        Public Function conversionValue(val As Object, type As String, env As Environment) As Object
            Select Case Microsoft.VisualBasic.Strings.Trim(type).ToLower
                Case "numeric", "double" : val = CLRVector.asNumeric(val)
                Case "integer" : val = CLRVector.asInteger(val)
                Case "character" : val = CLRVector.asCharacter(val)
                Case "raw" : val = CLRVector.asRawByte(val)
                Case "list" : val = base.Rlist(val, env)
                Case "float" : val = CLRVector.asFloat(val)
                Case "long" : val = CLRVector.asLong(val)
                Case Else
                    ' no conversion?
            End Select

            Return val
        End Function
    End Module
End Namespace
