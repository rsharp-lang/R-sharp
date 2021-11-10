Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Development.Package.NuGet

    Public Class Repository

        ''' <summary>
        ''' install R# module package from nuget repository
        ''' </summary>
        ''' <param name="packageName"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Shared Function Install(packageName As String, env As Environment) As Message
            Dim mirror As String = base.getOption("nuget_mirror", [default]:="azure_cn", env)
            Dim prerelease As Boolean = base.getOption("nuget_prerelease", [default]:="false", env) _
                .ToString _
                .ParseBoolean

            Return Install(packageName, env, mirror, include_prerelease:=prerelease)
        End Function

        ''' <summary>
        ''' install R# module package from nuget repository
        ''' </summary>
        ''' <param name="packageName"></param>
        ''' <param name="env"></param>
        ''' <param name="mirror"></param>
        ''' <param name="include_prerelease"></param>
        ''' <returns></returns>
        Public Shared Function Install(packageName As String, env As Environment,
                                       Optional mirror As String = "azure_cn",
                                       Optional include_prerelease As Boolean = False) As Message

        End Function

        ''' <summary>
        ''' search R# package from nuget repository
        ''' </summary>
        ''' <param name="packageName"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Shared Function Search(packageName As String, env As Environment) As dataframe

        End Function

    End Class
End Namespace