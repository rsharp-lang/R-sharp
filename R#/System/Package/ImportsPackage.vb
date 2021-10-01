#Region "Microsoft.VisualBasic::47e6f0c522e5404549c62a1fb17073f6, R#\System\Package\ImportsPackage.vb"

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
    '         Function: (+2 Overloads) GetAllApi, ImportsStatic, ImportsStaticInternalImpl
    ' 
    '         Sub: ImportsInstance, ImportsSymbolLanguages
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If netcore5 = 0 Then
Imports System.ComponentModel.Composition
#Else
Imports System.Composition
#End If
Imports System.Reflection
Imports System.Runtime.CompilerServices
#If netcore5 = 1 Then
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
#End If
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Interop.Operator

Namespace Development.Package

    ''' <summary>
    ''' Helper methods for add .NET function into <see cref="Environment"/> target
    ''' </summary>
    Public Module ImportsPackage

        Public Function GetAllApi(package As Package) As IEnumerable(Of NamedValue(Of MethodInfo))
            Return GetAllApi(package.package, strict:=True, includesInternal:=False)
        End Function

        ''' <summary>
        ''' 这个函数会获取得到通过<see cref="ExportAttribute"/>或者<see cref="ExportAPIAttribute"/>标记的函数
        ''' </summary>
        ''' <param name="package"></param>
        ''' <param name="strict"></param>
        ''' <param name="includesInternal"></param>
        ''' <returns></returns>
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
            Dim exportFlag As ExportAttribute

            For Each method As MethodInfo In methods
                flag = method.GetCustomAttribute(Of ExportAPIAttribute)

                If flag Is Nothing Then
                    exportFlag = method.GetCustomAttribute(Of ExportAttribute)

                    If strict AndAlso exportFlag Is Nothing Then
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

        Const obsoleteAssemblyImage$ = "Unable to load R# package module '{0}', due to the reason of obsolete assembly file! Please re-compile your package under the latest R#/sciBASIC.NET runtime!"

        ''' <summary>
        ''' This function returns a list of object which is masked by the new imports <paramref name="package"/>
        ''' </summary>
        ''' <param name="env"></param>
        ''' <param name="package"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ImportsStatic(env As Environment, package As Type, Optional strict As Boolean = True) As IEnumerable(Of String)
            Try
#If netcore5 = 1 Then
                Call deps.TryHandleNetCore5AssemblyBugs(package)
#End If
                Call package.Assembly.TryRunZzzOnLoad

                ' imports package and export api
                Return env.ImportsStaticInternalImpl(package, strict:=strict)
            Catch ex As Exception
                If TypeOf ex Is MissingMethodException Then
                    With DirectCast(ex, MissingMethodException)
                        If .Message = ".ctor" AndAlso InStr(ex.StackTrace, "GetCustomAttribute") > 0 Then
                            Throw New TypeLoadException(String.Format(obsoleteAssemblyImage, package.FullName), ex)
                        Else
                            Throw
                        End If
                    End With
                Else
                    Throw
                End If
            End Try
        End Function

        ''' <summary>
        ''' imports all static api method in a given package module. 
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="package"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        <Extension>
        Public Function ImportsStaticInternalImpl(envir As Environment, package As Type, Optional strict As Boolean = True) As IEnumerable(Of String)
            Dim [global] As GlobalEnvironment = envir.globalEnvironment
            Dim symbol As Symbol
            Dim Rmethods As RMethodInfo() = ImportsPackage _
                .GetAllApi(package, strict) _
                .Select(Function(m) New RMethodInfo(m)) _
                .ToArray
            Dim masked As New List(Of String)

            For Each api As RMethodInfo In Rmethods
                symbol = [global].FindFunction(api.name)

                If symbol Is Nothing Then
                    ' add new
                    symbol = New Symbol(api, TypeCodes.closure) With {
                        .name = api.name,
                        .[readonly] = False
                    }

                    [global].funcSymbols.Add(symbol)
                Else
                    ' overrides and masked by current package
                    symbol.SetValue(api, envir)
                    masked += symbol.name
                End If
            Next

            Call [global].attachedNamespace _
                .Add(package.NamespaceEntry.Namespace, package.Assembly.Location) _
                .AddSymbols(Rmethods.Select(Function(api) DirectCast(api, RFunction)))

            ' load types
            Dim types As RTypeExportAttribute() = package _
                .GetCustomAttributes(Of RTypeExportAttribute) _
                .ToArray

            For Each type As RTypeExportAttribute In types
                envir.globalEnvironment.types(type.name) = RType.GetRSharpType(type.model)
            Next

            ' find module initializer
            Dim init As MethodInfo = package _
                .GetMethods(bindingAttr:=BindingFlags.Public Or BindingFlags.Static) _
                .Where(Function(m)
                           Return Not m.GetCustomAttribute(GetType(RInitializeAttribute)) Is Nothing
                       End Function) _
                .FirstOrDefault

            If Not init Is Nothing Then
                If init.GetParameters.IsNullOrEmpty Then
                    Call init.Invoke(Nothing, {})
                Else
                    Call init.Invoke(Nothing, {envir})
                End If
            End If

            Call BinaryOperatorEngine.ImportsOperators(package, envir)
            Call ImportsPackage.ImportsSymbolLanguages(package, envir.globalEnvironment)

            Return masked
        End Function

        Private Sub ImportsSymbolLanguages(package As Type, env As GlobalEnvironment)
            For Each method In From m As MethodInfo
                               In package.GetMethods
                               Where m.IsStatic
                               Let mask As RSymbolLanguageMaskAttribute = m.GetAttribute(Of RSymbolLanguageMaskAttribute)
                               Where Not mask Is Nothing
                               Select mask, ptr = m

                Call env.symbolLanguages.AddSymbolLanguage(method.mask, method.ptr)
            Next
        End Sub

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
                Call [global].Push(api.name, api, [readonly]:=False, mode:=TypeCodes.closure)
            Next
        End Sub
    End Module
End Namespace
