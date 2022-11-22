#Region "Microsoft.VisualBasic::7bfb46b840bd9dc4bcf22573f8a0a925, R-sharp\R#\Runtime\Interop\RInteropAttributes\RApiReturnAttribute.vb"

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

'   Total Lines: 33
'    Code Lines: 21
' Comment Lines: 5
'   Blank Lines: 7
'     File Size: 1.23 KB


'     Class RApiReturnAttribute
' 
'         Properties: returnTypes
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: GetActualReturnType, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports SMRUCC.Rsharp.Runtime.Internal.Object

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
        ''' this function returns a typescript language liked union type
        ''' </summary>
        ''' <param name="type"></param>
        Sub New(ParamArray type As Type())
            returnTypes = type
        End Sub

        ''' <summary>
        ''' this function returns a R# <see cref="list"/> with these specific first level slot keys
        ''' </summary>
        ''' <param name="slots"></param>
        Sub New(ParamArray slots As String())
            fields = slots
            returnTypes = {GetType(list)}
        End Sub

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
