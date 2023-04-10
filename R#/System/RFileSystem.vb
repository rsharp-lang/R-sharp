#Region "Microsoft.VisualBasic::7762080ad9092aac799a3eda53d79d98, D:/GCModeller/src/R-sharp/R#//System/RFileSystem.vb"

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

    '   Total Lines: 27
    '    Code Lines: 20
    ' Comment Lines: 0
    '   Blank Lines: 7
    '     File Size: 893 B


    '     Class RFileSystem
    ' 
    '         Function: GetPackageDir, ListPackageDir, PackageInstalled
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Runtime

Namespace Development

    Public Class RFileSystem

        Public Shared Function ListPackageDir(env As Environment) As IEnumerable(Of String)
            Return GetPackageDir(env).ListDirectory
        End Function

        Public Shared Function GetPackageDir(env As Environment) As String
            Dim packageDir As String

            If App.IsMicrosoftPlatform Then
                packageDir = $"{env.globalEnvironment.options.lib_loc}/Library/"
            Else
                packageDir = env.globalEnvironment.options.lib_loc
            End If

            Return packageDir
        End Function

        Public Shared Function PackageInstalled(packageName As String, env As Environment) As Boolean
            Return $"{GetPackageDir(env)}/{packageName}".DirectoryExists
        End Function
    End Class
End Namespace
