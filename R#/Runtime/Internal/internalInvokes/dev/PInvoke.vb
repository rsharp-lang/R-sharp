#Region "Microsoft.VisualBasic::75b54ca873b7904e85e514a890f20aad, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/Rinterop.vb"

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

'   Total Lines: 62
'    Code Lines: 18
' Comment Lines: 39
'   Blank Lines: 5
'     File Size: 2.74 KB


'     Module Rinterop
' 
'         Function: Rcall, Rload
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Reflection.Emit
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' helper class for create runtime delegate type in dynamics
    ''' </summary>
    Friend Class RPInvoke

        ReadOnly returnVal As Type
        ReadOnly parameters As NamedValue(Of Type)()

        ReadOnly m_vals As Object()

        Public Const external_native_call_name As String = "Invoke"

        Public ReadOnly Property Values As Object()
            Get
                Return m_vals
            End Get
        End Property

        Sub New(arguments As list)
            Dim args As New List(Of NamedValue(Of Type))
            Dim vals As New List(Of Object)

            If arguments.hasName("return") Then
                returnVal = MapReturns(arguments.getByName("return"))
            End If

            For Each key As String In arguments.getNames.Where(Function(si) si <> "return")
                vals.Add(arguments.getByName(key))
                args.Add(key, MapParameterValue(arguments.getByName(key)))
            Next

            m_vals = vals.ToArray
            parameters = args.ToArray
        End Sub

        Private Shared Function MapReturns(val As Object) As Type
            Dim desc As String = CLRVector.asCharacter(val).FirstOrDefault

            If desc.StringEmpty Then
                Return Nothing
            End If

            Select Case desc.Trim.ToLower
                Case "i32", "integer" : Return GetType(Integer)
                Case "i64", "long" : Return GetType(Long)
                Case "char", "byte", "sbyte" : Return GetType(Byte)
                Case "str", "string", "character" : Return GetType(String)
                Case "float", "single", "f32" : Return GetType(Single)
                Case "double", "f64" : Return GetType(Double)
                Case "boolean", "bool", "logical" : Return GetType(Boolean)
                Case Else
                    Return GetType(IntPtr)
            End Select
        End Function

        Private Shared Function MapParameterValue(val As Object) As Type
            Dim type As Type = val.GetType

            If DataFramework.IsPrimitive(type) Then
                Return type
            Else
                Return GetType(IntPtr)
            End If
        End Function

        Public Function GetDelegate() As Type
            Dim tdelegate As TypeBuilder = DynamicType.GetTypeBuilder("native_delegate_func", GetType(MulticastDelegate), isAbstract:=True, sealed:=True)
            Dim new_fp = tdelegate.DefineConstructor(MethodAttributes.RTSpecialName Or
                                                     MethodAttributes.HideBySig Or
                                                     MethodAttributes.Public,
                                                     CallingConventions.Standard, {GetType(Object), GetType(IntPtr)})
            Dim params As Type() = parameters.Select(Function(a) a.Value).ToArray
            Dim native_calls = tdelegate.DefineMethod(external_native_call_name,
                                                      MethodAttributes.RTSpecialName Or
                                                      MethodAttributes.Public Or
                                                      MethodAttributes.HideBySig Or
                                                      MethodAttributes.NewSlot Or
                                                      MethodAttributes.Virtual, CallingConventions.Standard, returnVal, params)

            new_fp.SetImplementationFlags(MethodImplAttributes.CodeTypeMask)
            native_calls.SetImplementationFlags(MethodImplAttributes.CodeTypeMask)

            Return tdelegate.CreateType
        End Function

    End Class
End Namespace
