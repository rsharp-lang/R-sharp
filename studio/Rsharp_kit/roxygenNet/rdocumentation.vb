#Region "Microsoft.VisualBasic::72b39157b2f0b2b4c892af3b380a73af, D:/GCModeller/src/R-sharp/studio/Rsharp_kit/roxygenNet//rdocumentation.vb"

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

    '   Total Lines: 67
    '    Code Lines: 51
    ' Comment Lines: 8
    '   Blank Lines: 8
    '     File Size: 2.56 KB


    ' Module rdocumentation
    ' 
    '     Function: getFunctions, getPkgApisList, rdocumentation
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports any = Microsoft.VisualBasic.Scripting

<Package("rdocumentation")>
Public Module rdocumentation

    <ExportAPI("documentation")>
    Public Function rdocumentation(func As RFunction, template As String, Optional env As Environment = Nothing) As String
        Return New [function]().createHtml(func, template, env)
    End Function

    <ExportAPI("pull_clr_types")>
    Public Function pull_clr_types() As Type()
        Return [function].clr_types.PopAll
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="package">
    ''' the ``R#`` package module name or the module object itself.
    ''' </param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("getFunctions")>
    Public Function getFunctions(package As Object, Optional env As Environment = Nothing) As Object
        Dim apis = getPkgApisList(package, env)

        If apis Like GetType(Message) Then
            Return apis.TryCast(Of Message)
        End If

        Dim funcs As New list With {.slots = New Dictionary(Of String, Object)}
        Dim func As RMethodInfo

        For Each f As NamedValue(Of MethodInfo) In apis.TryCast(Of NamedValue(Of MethodInfo)())
            func = New RMethodInfo(f)
            funcs.add(func.name, func)
        Next

        Return funcs
    End Function

    Friend Function getPkgApisList(package As Object, env As Environment) As [Variant](Of Message, NamedValue(Of MethodInfo)())
        If TypeOf package Is String Then
            Return env.globalEnvironment.packages _
                .FindPackage(any.ToString(package), Nothing) _
                .DoCall(AddressOf ImportsPackage.GetAllApi) _
                .ToArray
        ElseIf TypeOf package Is Development.Package.Package Then
            Return ImportsPackage _
                .GetAllApi(DirectCast(package, Development.Package.Package)) _
                .ToArray
        Else
            Return Components _
                .Message _
                .InCompatibleType(GetType(String), package.GetType, env)
        End If
    End Function
End Module
