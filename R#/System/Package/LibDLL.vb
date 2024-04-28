#Region "Microsoft.VisualBasic::8ec3c5c7e00265f67c54d2886d2a752b, E:/GCModeller/src/R-sharp/R#//System/Package/LibDLL.vb"

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

    '   Total Lines: 141
    '    Code Lines: 96
    ' Comment Lines: 26
    '   Blank Lines: 19
    '     File Size: 5.79 KB


    '     Class LibDLL
    ' 
    '         Function: GetDllFile, getDllFromAppDir, getDllFromAttachedPackages
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.FileIO
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime

Namespace Development.Package

    Public Class LibDLL

        ''' <summary>
        ''' try to get .net clr dll assembly file from app directory or attached package assembly directories
        ''' </summary>
        ''' <param name="libDllName"></param>
        ''' <param name="env"></param>
        ''' <param name="searchContext"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' attach_lib_dir
        ''' </remarks>
        Public Shared Function GetDllFile(libDllName As String,
                                          env As Environment,
                                          Optional ByRef searchContext As Generic.List(Of String) = Nothing) As String

            Dim location As Value(Of String) = ""

            If searchContext Is Nothing Then
                searchContext = New List(Of String)
            End If

            If Not (location = getDllFromAppDir(
                libDll:=libDllName,
                globalEnvironment:=env.globalEnvironment,
                searchContext:=searchContext
            )).StringEmpty Then

                Return CType(location, String)
            ElseIf Not (location = getDllFromAttachedPackages(
                libDll:=libDllName,
                globalEnvironment:=env.globalEnvironment,
                searchContext:=searchContext
            )).StringEmpty Then

                Return CType(location, String)
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' load from the assembly of attatch packages
        ''' </summary>
        ''' <param name="libDll"></param>
        ''' <param name="globalEnvironment"></param>
        ''' <returns></returns>
        Private Shared Function getDllFromAttachedPackages(libDll As String,
                                                           globalEnvironment As GlobalEnvironment,
                                                           ByRef searchContext As Generic.List(Of String)) As String
            Dim location As Value(Of String) = ""

            For Each pkg As PackageEnvironment In globalEnvironment.attachedNamespace
                Dim lib_dir As Directory = pkg.libpath

                For Each assemblyDir As String In {$"{lib_dir.folder}/assembly", $"{lib_dir.folder}/lib/assembly", lib_dir.folder}
#If NETCOREAPP Then
                    If (location = $"{assemblyDir}/{libDll}.dll").FileExists Then
                        Return location
                    ElseIf (location = $"{assemblyDir}/{CreatePackage.getRuntimeTags}/{libDll}.dll").FileExists Then
                        Return location
                    ElseIf (location = $"{assemblyDir}/{libDll}").FileExists Then
                        Return location
                    ElseIf (location = $"{assemblyDir}/{CreatePackage.getRuntimeTags}/{libDll}").FileExists Then
                        Return location
                    End If
#Else
                If (location = $"{assemblyDir}/{libDll}.dll").FileExists Then
                    Return location
                ElseIf (location = $"{assemblyDir}/{libDll}").FileExists Then
                    Return location
                End If
#End If
                    Call searchContext.Add(assemblyDir)
                    Call searchContext.Add($"{assemblyDir}/{CreatePackage.getRuntimeTags}")
                Next
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="libDll">
        ''' the dll file name or full path
        ''' </param>
        ''' <param name="globalEnvironment"></param>
        ''' <returns></returns>
        Friend Shared Function getDllFromAppDir(libDll As String, globalEnvironment As GlobalEnvironment, ByRef searchContext As Generic.List(Of String)) As String
            Dim SetDllDirectory As String = globalEnvironment.options.getOption("SetDllDirectory", env:=globalEnvironment)

            If libDll.FileExists Then
                Return libDll.GetFullPath
            End If

            If SetDllDirectory.DirectoryExists Then
                Call searchContext.Add($"{SetDllDirectory}/{libDll}")

                If $"{SetDllDirectory}/{libDll}".FileExists Then
                    Return $"{SetDllDirectory}/{libDll}"
                End If
            End If

            For Each location As String In New String() {
                $"{App.HOME}/{libDll}",
                $"{App.HOME}/Library/{libDll}",
                $"{App.HOME}/../lib/{libDll}",
                $"{App.HOME}/../library/{libDll}",
                $"{App.HOME}/../Library/{libDll}"
            }
                If location.FileExists Then
                    Return location
                Else
                    Call searchContext.Add(location)
                End If
            Next

            If Not globalEnvironment.scriptDir Is Nothing Then
                If $"{globalEnvironment.scriptDir}/{libDll}".FileExists Then
                    Return $"{globalEnvironment.scriptDir}/{libDll}"
                End If
            End If

            ' if file not found then we test if the dll 
            ' file extension Is Missing Or Not?
            If Not libDll.ExtensionSuffix("exe", "dll") Then
                Return getDllFromAppDir($"{libDll}.dll", globalEnvironment, searchContext)
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace
