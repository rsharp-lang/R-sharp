#Region "Microsoft.VisualBasic::e49bf54074a42e2ae63e01d68cd9c60e, R-sharp\R#\System\Package\LibDLL.vb"

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


     Code Statistics:

        Total Lines:   93
        Code Lines:    73
        Comment Lines: 8
        Blank Lines:   12
        File Size:     3.62 KB


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
                Dim assemblyDir As String = $"{pkg.libpath}/assembly"

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

            Return Nothing
        End Function

        Private Shared Function getDllFromAppDir(libDll As String, globalEnvironment As GlobalEnvironment) As String
            For Each location As String In {
                    $"{App.HOME}/{libDll}",
                    $"{App.HOME}/Library/{libDll}",
                    $"{App.HOME}/../lib/{libDll}"
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
                For Each location As String In {
                    $"{App.HOME}/{libDll}.dll",
                    $"{App.HOME}/Library/{libDll}.dll",
                    $"{App.HOME}/../lib/{libDll}.dll"
                }
                    If location.FileExists Then
                        Return location
                    End If
                Next
            End If

            Return Nothing
        End Function
    End Class
End Namespace
