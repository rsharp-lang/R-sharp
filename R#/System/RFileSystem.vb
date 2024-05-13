#Region "Microsoft.VisualBasic::b18725cbcc47ee33f562a21e8cbf600d, R#\System\RFileSystem.vb"

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

    '   Total Lines: 25
    '    Code Lines: 15
    ' Comment Lines: 5
    '   Blank Lines: 5
    '     File Size: 939 B


    '     Class RFileSystem
    ' 
    '         Function: GetPackageDir, ListPackageDir, PackageInstalled
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports SMRUCC.Rsharp.Development.Package.File
Imports SMRUCC.Rsharp.Runtime

Namespace Development

    Public Class RFileSystem

        Public Shared Function ListPackageDir(env As Environment) As IEnumerable(Of String)
            Return GetPackageDir(env).ListDirectory
        End Function

        ''' <summary>
        ''' a shortcut wrapper for function <see cref="PackageLoader2.GetPackageDirectory"/>
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        Public Shared Function GetPackageDir(env As Environment) As String
            Return PackageLoader2.GetPackageDirectory(env.globalEnvironment.options, "")
        End Function

        Public Shared Function PackageInstalled(packageName As String, env As Environment) As Boolean
            Return $"{GetPackageDir(env)}/{packageName}".DirectoryExists
        End Function
    End Class
End Namespace
