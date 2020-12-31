Imports Microsoft.VisualBasic.Language
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.System.Package.File

Namespace System.Package

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

            For Each pkg As PackageNamespace In globalEnvironment.attachedNamespace.Values
                Dim assemblyDir As String = $"{pkg.libPath}/assembly"

                If (location = $"{assemblyDir}/{libDll}.dll").FileExists Then
                    Return location
                ElseIf (location = $"{assemblyDir}/{libDll}").FileExists Then
                    Return location
                End If
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