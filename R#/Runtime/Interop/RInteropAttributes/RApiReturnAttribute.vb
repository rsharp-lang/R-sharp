#Region "Microsoft.VisualBasic::4ed4d64e75adf6a17fd4e95a578d6406, F:/GCModeller/src/R-sharp/R#//Runtime/Interop/RInteropAttributes/RApiReturnAttribute.vb"

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

    '   Total Lines: 98
    '    Code Lines: 62
    ' Comment Lines: 23
    '   Blank Lines: 13
    '     File Size: 3.61 KB


    '     Class RApiReturnAttribute
    ' 
    '         Properties: fields, isClassGraph, returnTypes, unionType
    ' 
    '         Constructor: (+4 Overloads) Sub New
    '         Function: GetActualReturnType, GetClass, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports any = Microsoft.VisualBasic.Scripting

Namespace Runtime.Interop

    ''' <summary>
    ''' For make compatibale with return value and exception message or R# object wrapper
    ''' The .NET api is usually declare as returns object value, then we could use this
    ''' attribute to let user known the actual returns type of the target api function
    ''' </summary>
    <AttributeUsage(AttributeTargets.Method, AllowMultiple:=False, Inherited:=True)>
    Public Class RApiReturnAttribute : Inherits RInteropAttribute

        Public ReadOnly Property returnTypes As Type()
        Public ReadOnly Property fields As String()

        Public ReadOnly Property unionType As String
            Get
                If returnTypes.Length = 1 AndAlso returnTypes(Scan0) Is GetType(list) Then
                    Return $"list({fields.JoinBy(", ")})"
                Else
                    Return returnTypes.Select(Function(type) type.Name).JoinBy("|")
                End If
            End Get
        End Property

        ''' <summary>
        ''' Do we can create a <see cref="RS4ClassGraph"/> from the attribute values?
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property isClassGraph As Boolean
            Get
                Return returnTypes(Scan0) Is GetType(list) AndAlso Not fields.IsNullOrEmpty
            End Get
        End Property

        ''' <summary>
        ''' this function returns a typescript language liked union type
        ''' </summary>
        ''' <param name="type"></param>
        Sub New(ParamArray type As Type())
            returnTypes = type
        End Sub

        ''' <summary>
        ''' Create a type reference via the R# type code enumeration 
        ''' flag for indicate this function return value.
        ''' </summary>
        ''' <param name="type"></param>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(type As TypeCodes)
            Call Me.New(New Type() {RType.GetType(type).GetRawElementType})
        End Sub

        Sub New(type As TypeCode)
            Call Me.New(New Type() {RType.GetType(type).GetRawElementType})
        End Sub

        ''' <summary>
        ''' this function returns a R# <see cref="list"/> with these specific first level slot keys
        ''' </summary>
        ''' <param name="slots"></param>
        Sub New(ParamArray slots As String())
            fields = slots
            returnTypes = {GetType(list)}
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <returns>
        ''' this function returns nothing if not <see cref="isClassGraph"/>.
        ''' </returns>
        Public Function GetClass() As RS4ClassGraph
            If Not isClassGraph Then
                Return Nothing
            Else
                Dim uuid As String = any.ToString(
                    obj:=MyBase.TypeId,
                    null:=$"anonymous_{Me.GetHashCode.ToHexString}"
                )
                Return New RS4ClassGraph(uuid, fields)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"fun() -> {unionType}"
        End Function

        Public Shared Function GetActualReturnType(api As MethodInfo) As Type()
            Dim tag As RApiReturnAttribute = api.GetCustomAttribute(Of RApiReturnAttribute)

            If tag Is Nothing Then
                Return {api.ReturnType}
            Else
                Return tag.returnTypes
            End If
        End Function
    End Class
End Namespace
