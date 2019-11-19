#Region "Microsoft.VisualBasic::32e0cd57227bf70c7ee1bd0cefe0a3e0, R#\Runtime\Internal\utils.vb"

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

    '     Module utils
    ' 
    '         Function: installPackages
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Package
Imports Microsoft.VisualBasic.Language

Namespace Runtime.Internal

    Module utils

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="packages">The dll file name</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function installPackages(packages As String(), envir As Environment) As Object
            Dim pkgMgr As PackageManager = envir.GlobalEnvironment.packages
            Dim namespaces As New List(Of String)

            For Each pkgName As String In packages.SafeQuery
                namespaces += pkgMgr.InstallLocals(pkgName)
            Next

            Return namespaces.ToArray
        End Function
    End Module
End Namespace
