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

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Package

Namespace Runtime.Internal

    Module utils

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="packages">The dll file name</param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function installPackages(packages As String(), envir As Environment) As Object
            Dim pkgMgr As PackageManager = envir.globalEnvironment.packages
            Dim namespaces As New List(Of String)

            For Each pkgName As String In packages.SafeQuery
                namespaces += pkgMgr.InstallLocals(pkgName)
            Next

            Call pkgMgr.Flush()

            Return namespaces.ToArray
        End Function

        ''' <summary>
        ''' ## Find Installed Packages
        ''' 
        ''' Find (or retrieve) details of all packages installed in the specified libraries.
        ''' 
        ''' ``installed.packages`` scans the ‘DESCRIPTION’ files of each package found along 
        ''' ``lib.loc`` and returns a matrix of package names, library paths and version numbers.
        '''
        ''' The information found Is cached (by library) For the R session And specified fields argument, 
        ''' And updated only If the top-level library directory has been altered, 
        ''' For example by installing Or removing a package. If the cached information becomes confused, 
        ''' it can be refreshed by running ``installed.packages(noCache = True)``.
        ''' </summary>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        <ExportAPI("installed.packages")>
        Public Function GetInstalledPackages(envir As Environment) As Object


        End Function

        ''' <summary>
        ''' A utils function for print user defined structure and class object instance. 
        ''' </summary>
        ''' <param name="obj">
        ''' The object class is ensure that not nothing!
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>
        ''' All of the  method is non generic, and the property without arguments!
        ''' </remarks>
        Public Function printClass(obj As Object) As String
            Dim type As Type = obj.GetType
            Dim properties As PropertyInfo() = type.GetProperties(PublicProperty).Where(Function(p) p.GetIndexParameters.IsNullOrEmpty).ToArray
            Dim methods As MethodInfo() = type.GetMethods(PublicProperty).Where(Function(m) Not m.ContainsGenericParameters).ToArray
        End Function
    End Module
End Namespace
