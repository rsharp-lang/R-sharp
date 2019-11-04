Imports System.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Runtime.Package

    Public Module PackageLoader

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
    End Module
End Namespace