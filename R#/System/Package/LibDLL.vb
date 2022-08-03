#Region "Microsoft.VisualBasic::8b0957d19aaf9aab9c7fad863d1f8a1c, R-sharp\R#\System\Package\LibDLL.vb"

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

    '   Total Lines: 107
    '    Code Lines: 78
    ' Comment Lines: 16
    '   Blank Lines: 13
    '     File Size: 4.25 KB


    '     Class LibDLL
    ' 
    '         Function: GetDllFile, getDllFromAppDir, getDllFromAttachedPackages
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If netcore5 = 0 Then
Imports System.ComponentModel.Composition
#Else
#End If
Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime

Namespace Development.Package

    Public Class LibDLL

        Public Shared Function GetDllFile(libDllName As String, env As Environment) As String
            Dim location As Value(Of String) = ""

            If Not (location = getDllFromAppDir(libDllName, env.globalEnvironment)).StringEmpty Then
                Return CType(location, String)
            ElseIf Not (location = getDllFromAttachedPackages(libDllName, env.globalEnvironment)).StringEmpty Then
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
        Private Shared Function getDllFromAttachedPackages(libDll As String, globalEnvironment As GlobalEnvironment) As String
            Dim location As Value(Of String) = ""

            For Each pkg As NamespaceEnvironment In globalEnvironment.attachedNamespace
                For Each assemblyDir As String In {$"{pkg.libpath}/assembly", $"{pkg.libpath}/lib/assembly"}
#If netcore5 = 1 Then
                    If (location = $"{assemblyDir}/{libDll}.dll").FileExists Then
                        Return location
                    ElseIf (location = $"{assemblyDir}/net5.0/{libDll}.dll").FileExists Then
                        Return location
                    ElseIf (location = $"{assemblyDir}/{libDll}").FileExists Then
                        Return location
                    ElseIf (location = $"{assemblyDir}/net5.0/{libDll}").FileExists Then
                        Return location
                    End If
#Else
                If (location = $"{assemblyDir}/{libDll}.dll").FileExists Then
                    Return location
                ElseIf (location = $"{assemblyDir}/{libDll}").FileExists Then
                    Return location
                End If
#End If
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
        Friend Shared Function getDllFromAppDir(libDll As String, globalEnvironment As GlobalEnvironment) As String
            Dim SetDllDirectory As String = globalEnvironment.options.getOption("SetDllDirectory", env:=globalEnvironment)

            If libDll.FileExists Then
                Return libDll.GetFullPath
            End If

            If SetDllDirectory.DirectoryExists Then
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
                Return getDllFromAppDir($"{libDll}.dll", globalEnvironment)
            Else
                Return Nothing
            End If
        End Function
    End Class
End Namespace
