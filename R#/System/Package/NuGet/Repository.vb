#Region "Microsoft.VisualBasic::6131d1eadd3ed15b0e729e403cd685eb, R-sharp\R#\System\Package\NuGet\Repository.vb"

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

    '   Total Lines: 50
    '    Code Lines: 21
    ' Comment Lines: 20
    '   Blank Lines: 9
    '     File Size: 1.92 KB


    '     Class Repository
    ' 
    '         Function: (+2 Overloads) Install, Search
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
