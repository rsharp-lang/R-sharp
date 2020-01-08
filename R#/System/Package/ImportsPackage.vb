#Region "Microsoft.VisualBasic::ee3fb879e85cb38d9a2f7096a4978709, R#\System\Package\ImportsPackage.vb"

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

    '     Module ImportsPackage
    ' 
    '         Function: GetAllApi, ImportsStatic
    ' 
    '         Sub: ImportsInstance
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Development.XmlDoc.Assembly
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace System.Package

    ''' <summary>
    ''' Helper methods for add .NET function into <see cref="Environment"/> target
    ''' </summary>
    Public Module ImportsPackage

        Public Function GetAllApi(package As Package) As IEnumerable(Of NamedValue(Of MethodInfo))
            Return GetAllApi(package.package, strict:=True, includesInternal:=False)
        End Function

        Public Iterator Function GetAllApi(package As Type,
                                           Optional strict As Boolean = True,
                                           Optional includesInternal As Boolean = False) As IEnumerable(Of NamedValue(Of MethodInfo))

            Dim access As BindingFlags

            If includesInternal Then
                access = BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Static
            Else
                access = BindingFlags.Public Or BindingFlags.Static
            End If

            Dim methods As MethodInfo() = package.GetMethods(access)
            Dim name As String
            Dim flag As ExportAPIAttribute

            For Each method As MethodInfo In methods
                flag = method.GetCustomAttribute(Of ExportAPIAttribute)

                If flag Is Nothing Then
                    If strict Then
                        Continue For
                    Else
                        name = method.Name
                    End If
                Else
                    name = flag.Name
                End If

                Yield New NamedValue(Of MethodInfo) With {
                    .Name = name,
                    .Value = method,
                    .Description = method.Name
                }
            Next
        End Function

        ''' <summary>
        ''' This function returns a list of object which is masked by the new imports <paramref name="package"/>
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="package"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ImportsStatic(envir As Environment, package As Type, Optional strict As Boolean = True) As IEnumerable(Of String)
            Dim [global] As GlobalEnvironment = envir.globalEnvironment
            Dim docs As ProjectType = [global].packages.packageDocs.GetAnnotations(package)
            Dim symbol As Variable
            Dim Rmethods As RMethodInfo() = ImportsPackage _
                .GetAllApi(package, strict) _
                .Select(Function(m) New RMethodInfo(m)) _
                .ToArray
            Dim masked As New List(Of String)

            For Each api As RMethodInfo In Rmethods
                symbol = [global].FindSymbol(api.name)

                If symbol Is Nothing Then
                    ' add new
                    [global].Push(api.name, api, TypeCodes.closure)
                Else
                    ' overrides and masked by current package
                    symbol.value = api
                    masked += symbol.name
                End If
            Next

            Return masked
        End Function

        <Extension>
        Public Sub ImportsInstance(envir As Environment, target As Object)
            Dim methods = target.GetType.GetMethods(BindingFlags.Public Or BindingFlags.Instance)
            Dim Rmethods = methods _
                .Select(Function(m)
                            Dim flag = m.GetCustomAttribute(Of ExportAPIAttribute)
                            Dim name = If(flag Is Nothing, m.Name, flag.Name)

                            Return New RMethodInfo(name, m, target)
                        End Function) _
                .ToArray
            Dim [global] As GlobalEnvironment = envir.globalEnvironment

            For Each api As RMethodInfo In Rmethods
                Call [global].Push(api.name, api, TypeCodes.closure)
            Next
        End Sub
    End Module
End Namespace
