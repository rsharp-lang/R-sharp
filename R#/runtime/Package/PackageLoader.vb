Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Package

    Public Module PackageLoader

        ''' <summary>
        ''' 应该是只会加载静态方法
        ''' </summary>
        ''' <param name="dll$"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function ParsePackages(dll$, Optional strict As Boolean = True) As IEnumerable(Of Package)
            Dim types As Type() = Assembly.LoadFrom(dll.GetFullPath).GetTypes
            Dim package As PackageAttribute

            For Each type As Type In types
                package = type.GetCustomAttribute(Of PackageAttribute)

                If package Is Nothing Then
                    If strict Then
                        Continue For
                    Else
                        package = New PackageAttribute(type.Name) With {
                            .Description = type.Description
                        }
                    End If
                End If

                Yield New Package(package, package:=type)
            Next
        End Function

        ''' <summary>
        ''' Scan the given directory and parse package from dll files.
        ''' </summary>
        ''' <param name="directory"></param>
        ''' <param name="strict"></param>
        ''' <returns></returns>
        Public Iterator Function ScanDllFiles(directory As String, Optional strict As Boolean = True) As IEnumerable(Of Package)
            For Each dll As String In ls - l - r - "*.dll" <= directory
                For Each package As Package In dll.ParsePackages(strict)
                    Yield package
                Next
            Next
        End Function
    End Module
End Namespace