Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace System.Package.File

    Public Class PackageNamespace

        Public Property meta As DESCRIPTION
        Public Property checksum As String
        Public Property libPath As String
        Public Property assembly As Dictionary(Of String, String)
        Public Property dependency As Dependency()
        Public Property symbols As Dictionary(Of String, String)
        Public Property runtime As AssemblyInfo
        Public Property framework As AssemblyInfo

        Sub New(dir As String)
            meta = $"{dir}/index.json".LoadJsonFile(Of DESCRIPTION)
            checksum = $"{dir}/CHECKSUM".ReadAllText
            libPath = dir
            assembly = $"{dir}/manifest/assembly.json".LoadJsonFile(Of Dictionary(Of String, String))
            dependency = $"{dir}/manifest/dependency.json".LoadJsonFile(Of Dependency())
            symbols = $"{dir}/manifest/symbols.json".LoadJsonFile(Of Dictionary(Of String, String))
            runtime = $"{dir}/manifest/runtime.json".LoadJsonFile(Of AssemblyInfo)
            framework = $"{dir}/manifest/framework.json".LoadJsonFile(Of AssemblyInfo)
        End Sub

        Public Function FindAssemblyPath(assemblyName As String) As String
            If assembly.ContainsKey($"{assemblyName}.dll") Then
                Return $"{libPath}/assembly/{assemblyName}.dll"
            Else
                Return Nothing
            End If
        End Function

        Public Iterator Function EnumerateSymbols() As IEnumerable(Of NamedValue(Of String))
            For Each item In symbols
                Yield New NamedValue(Of String)(item)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return meta.ToString
        End Function

    End Class
End Namespace