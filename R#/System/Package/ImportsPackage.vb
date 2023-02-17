#Region "Microsoft.VisualBasic::bb19f8b0d433f1d0526d5a3c52f8b7a1, D:/GCModeller/src/R-sharp/R#//System/Package/ImportsPackage.vb"

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

    '   Total Lines: 290
    '    Code Lines: 202
    ' Comment Lines: 46
    '   Blank Lines: 42
    '     File Size: 12.08 KB


    '     Module ImportsPackage
    ' 
    '         Function: (+2 Overloads) GetAllApi, GetExportName, getFlags, ImportsStatic, ImportsStaticInternalImpl
    '                   ParseAnyFlagName, TryParse
    ' 
    '         Sub: ImportsInstance, ImportsSymbolLanguages, runMain
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If Not NETCOREAPP Then
Imports System.ComponentModel.Composition
#Else
Imports System.Composition
#End If
Imports System.Reflection
Imports System.Runtime.CompilerServices
#If NETCOREAPP Then
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
Imports any = Microsoft.VisualBasic.Scripting

Namespace Development.Package

    ''' <summary>
    ''' Helper methods for add .NET function into <see cref="Environment"/> target
    ''' </summary>
    Public Module ImportsPackage

        Public Function GetAllApi(package As Package) As IEnumerable(Of NamedValue(Of MethodInfo))
            Return GetAllApi(package.package, strict:=True, includesInternal:=False)
        End Function

        Private Function getFlags(includesInternal As Boolean) As BindingFlags
            If includesInternal Then
                Return BindingFlags.NonPublic Or BindingFlags.Public Or BindingFlags.Static
            Else
                Return BindingFlags.Public Or BindingFlags.Static
            End If
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

            Dim access As BindingFlags = getFlags(includesInternal)
            Dim methods As MethodInfo() = package.GetMethods(access)
            Dim found As NamedValue(Of MethodInfo)

            For Each method As MethodInfo In methods
                found = TryParse(method, strict)

                If Not found.Value Is Nothing Then
                    Yield found
                End If
            Next
        End Function

        Private Function ParseAnyFlagName(method As MethodInfo, strict As Boolean, ByRef notFound As Boolean) As String
            Dim exportFlag = method.GetCustomAttribute(Of ExportAttribute)
            Dim anyAttr As CustomAttributeData

            Static exportTag As Index(Of String) = {"Export", "ExportAPI"}

            If exportFlag Is Nothing Then
                ' continute test with the custom attribute name
                anyAttr = method.CustomAttributes _
                    .Where(Function(a)
                               Dim b1 = a.AttributeType.Name Like exportTag
                               Dim b2 = a.AttributeType.Name.Replace("Attribute", "") Like exportTag

                               Return b1 OrElse b2
                           End Function) _
                    .FirstOrDefault

                If Not anyAttr Is Nothing Then
                    Dim argv = anyAttr.NamedArguments _
                        .Where(Function(a) a.MemberName.TextEquals("name")) _
                        .FirstOrDefault

                    Return any.ToString(argv.TypedValue.Value)
                ElseIf strict Then
                    notFound = True
                End If

                Return Nothing
            Else
                Return method.Name
            End If
        End Function

        <Extension>
        Public Function GetExportName(method As MethodInfo, strict As Boolean) As String
            Dim flag = method.GetCustomAttribute(Of ExportAPIAttribute)
            Dim name As String
            Dim notFound As Boolean = False

            If flag Is Nothing Then
                name = ParseAnyFlagName(method, strict, notFound)

                If notFound Then
                    Return Nothing
                End If
            Else
                name = flag.Name
            End If

            Return name
        End Function

        Private Function TryParse(method As MethodInfo, strict As Boolean) As NamedValue(Of MethodInfo)
            Dim name As String = method.GetExportName(strict)

            ' name will be empty when strict parameter is TRUE
            ' andalso the export attribute is not found
            If name Is Nothing Then
                Return Nothing
            End If

            Return New NamedValue(Of MethodInfo) With {
                .Name = If(name.StringEmpty, method.Name, name),
                .Value = method,
                .Description = method.Name
            }
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
#If NETCOREAPP Then
                Call deps.TryHandleNetCore5AssemblyBugs(package)
#End If
                Call package.Assembly.TryRunZzzOnLoad

                ' imports package and export api
                Return env.ImportsStaticInternalImpl(package, strict:=strict)
            Catch ex As Exception
                If TypeOf ex Is MissingMethodException Then
                    With DirectCast(ex, MissingMethodException)
                        If .Message = ".ctor" AndAlso Microsoft.VisualBasic.InStr(ex.StackTrace, "GetCustomAttribute") > 0 Then
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

            ' 20221122 the const keyword is limited to the user code
            ' which means, user can not set value of a locked symbol
            ' in script
            ' but the function declare in a package is no limited via
            ' this keyword, the imports function can overrides the 
            ' const keyword.

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
                    ' even the const symbol that could be
                    ' overrides and masked by current package
                    ' while in the progress of package
                    ' imports
                    symbol.SetValue(api, envir, [overrides]:=True)
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

            Call ImportsPackage.runMain(package, envir)
            Call BinaryOperatorEngine.ImportsOperators(package, envir)
            Call ImportsPackage.ImportsSymbolLanguages(package, envir.globalEnvironment)

            Return masked
        End Function

        Private Sub runMain(package As Type, envir As Environment)
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
        End Sub

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

        ''' <summary>
        ''' imports class object instance methods.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <param name="target"></param>
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
                Call [global].Push(
                    name:=api.name,
                    value:=api,
                    [readonly]:=False,
                    mode:=TypeCodes.closure
                )
            Next
        End Sub
    End Module
End Namespace
