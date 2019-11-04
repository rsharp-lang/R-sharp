Imports System.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Language.UnixBash
Imports System.Runtime.CompilerServices

Namespace Runtime.Package

    Public Module PackageLoader

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

        Public Iterator Function ScanAssembly(directory As String, Optional strict As Boolean = True) As IEnumerable(Of Package)
            For Each dll As String In ls - l - r - "*.dll" <= directory
                For Each package As Package In dll.ParsePackages(strict)
                    Yield package
                Next
            Next
        End Function
    End Module
End Namespace