#Region "Microsoft.VisualBasic::547a3c2b52018c756a7eba0e28cc5a90, R#\Runtime\Interop\REnum.vb"

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

    '   Total Lines: 111
    '    Code Lines: 83
    ' Comment Lines: 7
    '   Blank Lines: 21
    '     File Size: 3.77 KB


    '     Class REnum
    ' 
    '         Properties: baseType, name, raw
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: getByIntVal, GetByName, GetEnumList, hasName, IntValue
    '                   ToString
    ' 
    '         Sub: doEnumParser
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Runtime.Interop

    ''' <summary>
    ''' VB.NET enum type wrapper in R#
    ''' </summary>
    Public Class REnum

        Public Property raw As Type

        ReadOnly namedValues As New Dictionary(Of String, Object)
        ReadOnly intValues As New Dictionary(Of String, Object)
        ReadOnly namedIntegers As New Dictionary(Of String, Long)

        Public ReadOnly Iterator Property values As IEnumerable(Of Object)
            Get
                For Each item As Object In namedValues.Values
                    Yield item
                Next
            End Get
        End Property

        Public ReadOnly Property name As String
            Get
                Return raw.Name
            End Get
        End Property

        Public ReadOnly Property baseType As Type

        Private Sub New(type As Type)
            raw = type

            ' parsing enum type values for 
            ' named values and 
            ' int values
            Call doEnumParser()

            baseType = DirectCast(namedValues.Values.First, [Enum]) _
                .GetTypeCode() _
                .CreatePrimitiveType()
        End Sub

        Public Function IntValue(val As Object) As Long
            Dim key As String = val.ToString.ToLower

            If namedIntegers.ContainsKey(key) Then
                Return namedIntegers(key)
            Else
                Return Conversion.CTypeDynamic(val, GetType(Long))
            End If
        End Function

        Private Sub doEnumParser()
            Dim values As [Enum]() = raw _
                .GetEnumValues _
                .AsObjectEnumerator _
                .Select(Function(flag) DirectCast(flag, [Enum])) _
                .ToArray
            Dim members As Dictionary(Of String, FieldInfo) = raw.GetFields _
                .Where(Function(field) field.FieldType Is raw) _
                .ToDictionary(Function(flag)
                                  Return flag.GetValue(Nothing).ToString
                              End Function)
            Dim int As Long

            For Each flag As [Enum] In values
                int = CLng(members(flag.ToString).GetValue(Nothing))
                intValues.Add("T" & int, flag)
                namedValues.Add(flag.ToString.ToLower, flag)
                namedIntegers.Add(flag.ToString.ToLower, int)
            Next
        End Sub

        Public Function hasName(name As String) As Boolean
            Return namedValues.ContainsKey(name.ToLower)
        End Function

        Public Function GetByName(name As String) As Object
            Return namedValues.TryGetValue(name.ToLower)
        End Function

        Public Function getByIntVal(int As Long) As Object
            Dim key = "T" & int

            If intValues.ContainsKey(key) Then
                Return intValues(key)
            Else
                ' is a flags combination
                Return Conversion.CTypeDynamic(int, baseType)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"({baseType.Name}) {raw.FullName}"
        End Function

        Public Shared Function GetEnumList(type As Type) As REnum
            If Not type.IsEnum Then
                Throw New InvalidCastException(type.FullName)
            End If

            Static enumCache As New Dictionary(Of Type, REnum)
            Return enumCache.ComputeIfAbsent(type, Function() New REnum(type))
        End Function
    End Class
End Namespace
