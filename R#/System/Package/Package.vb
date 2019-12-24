#Region "Microsoft.VisualBasic::5bd25b10249e07410cc4b6a1f625fd6f, R#\System\Package\Package.vb"

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

'     Class Package
' 
'         Properties: [namespace], info, package
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: GetFunction, GetFunctionInternal, ToString
' 
' 
' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace System.Package

    Public Class Package

        Public Property info As PackageAttribute
        Public Property package As Type

        Public ReadOnly Property [namespace] As String
            Get
                Return info.Namespace
            End Get
        End Property

        Public ReadOnly Property LibPath As String
            Get
                Return package.Assembly.Location
            End Get
        End Property

        Sub New(info As PackageAttribute, package As Type)
            Me.info = info
            Me.package = package
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetPackageModuleInfo() As AssemblyInfo
            Return package.Assembly.FromAssembly
        End Function

        ''' <summary>
        ''' [namespace::name -> entry_point]
        ''' </summary>
        Shared ReadOnly apiCache As New Dictionary(Of String, RMethodInfo)

        Public Function GetFunction(apiName As String) As RMethodInfo
            Dim ref$ = $"{[namespace]}::{apiName}"
            Dim api As RMethodInfo

            If Not apiCache.ContainsKey(ref) Then
                api = GetFunctionInternal(apiName)
                apiCache(ref) = api
            Else
                api = apiCache(ref)
            End If

            Return api
        End Function

        Private Function GetFunctionInternal(apiName As String) As RMethodInfo
            Dim apiHandle As NamedValue(Of MethodInfo) = ImportsPackage _
                .GetAllApi(package) _
                .FirstOrDefault(Function(api)
                                    Return api.Name = apiName
                                End Function)

            If apiHandle.IsEmpty Then
                Return Nothing
            Else
                Return New RMethodInfo(apiHandle)
            End If
        End Function

        Public Overrides Function ToString() As String
            Return $"{info.Namespace}: {info.Description}"
        End Function
    End Class
End Namespace
