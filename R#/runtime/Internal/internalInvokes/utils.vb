#Region "Microsoft.VisualBasic::80800a887ab06dd6064617f3e8c9e388, R#\Runtime\Internal\internalInvokes\utils.vb"

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
'         Function: GetInstalledPackages, installPackages, wget
' 
'         Sub: cls, sleep
' 
' 
' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Net
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.System.Package
Imports RPkg = SMRUCC.Rsharp.System.Package.Package

Namespace Runtime.Internal.Invokes

    Module utils

        ''' <summary>
        ''' # Install Packages from Repositories or Local Files
        ''' 
        ''' Download and install packages from CRAN-like repositories or from local files.
        ''' 
        ''' This is the main function to install packages. It takes a vector of names and 
        ''' a destination library, downloads the packages from the repositories and installs 
        ''' them. (If the library is omitted it defaults to the first directory in 
        ''' .libPaths(), with a message if there is more than one.) If lib is omitted or 
        ''' is of length one and is not a (group) writable directory, in interactive use 
        ''' the code offers to create a personal library tree (the first element of 
        ''' Sys.getenv("R_LIBS_USER")) and install there. Detection of a writable 
        ''' directory is problematic on Windows: see the ‘Note’ section.
        '''
        ''' For installs from a repository an attempt Is made To install the packages In 
        ''' an order that respects their dependencies. This does assume that all the 
        ''' entries In Lib are On the Default library path For installs (Set by 
        ''' environment variable R_LIBS).
        '''
        ''' You are advised To run update.packages before install.packages To ensure that 
        ''' any already installed dependencies have their latest versions.
        ''' </summary>
        ''' <param name="packages">The dll file name, character vector of the names of 
        ''' packages whose current versions should be downloaded from the repositories.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        ''' 
        <ExportAPI("install.packages")>
        Public Function installPackages(packages$(), Optional envir As Environment = Nothing) As Object
            Dim pkgMgr As PackageManager = envir.globalEnvironment.packages
            Dim namespaces As New List(Of String)

            For Each pkgName As String In packages.SafeQuery
                If pkgName.FileExists Then
                    namespaces += pkgMgr.InstallLocals(pkgName)
                Else
                    Return Internal.stop($"Library module '{pkgName.GetFullPath}' is not exists on your file system!", envir)
                End If
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
        Public Function GetInstalledPackages(Optional envir As Environment = Nothing) As Object
            Dim pkgMgr As PackageManager = envir.globalEnvironment.packages
            Dim packages As RPkg() = pkgMgr _
                .AsEnumerable _
                .OrderBy(Function(pkg) pkg.namespace) _
                .ToArray
            Dim Package As Array = packages.Select(Function(pkg) pkg.namespace).ToArray
            Dim LibPath As Array = packages.Select(Function(pkg) pkg.LibPath.GetFullPath).ToArray
            Dim Version As Array = packages.Select(Function(pkg) pkg.info.Revision).ToArray
            Dim Built As Array = packages.Select(Function(pkg) pkg.GetPackageModuleInfo.BuiltTime.ToString).ToArray
            Dim Description As Array = packages _
                .Select(Function(pkg)
                            Return pkg.GetPackageDescription(envir) _
                                .LineTokens _
                                .DefaultFirst("n/a")
                        End Function) _
                .ToArray
            Dim summary As New dataframe With {
                .rownames = packages.Select(Function(pkg) pkg.namespace).ToArray,
                .columns = New Dictionary(Of String, Array) From {
                    {NameOf(Package), Package},
                    {NameOf(LibPath), LibPath},
                    {NameOf(Version), Version},
                    {NameOf(Built), Built},
                    {NameOf(Description), Description}
                }
            }

            Return summary
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="url"></param>
        ''' <param name="save"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("wget")>
        Public Function wget(url As String, Optional save As String = Nothing, Optional env As Environment = Nothing) As Object
            If url.StringEmpty Then
                Return Internal.stop({"Missing url data source for file get!"}, env)
            ElseIf save.StringEmpty Then
                save = App.CurrentDirectory & "/" & url.Split("?"c).First.BaseName.NormalizePathString(False)
            End If

            Return Http.wget.Download(url, save)
        End Function

        ''' <summary>
        ''' Clears the console buffer and corresponding console window of display information.
        ''' </summary>
        <ExportAPI("clear")>
        Public Sub cls()
            Call Console.Clear()
        End Sub

        ''' <summary>
        ''' Suspends the current thread for the specified number of seconds.
        ''' </summary>
        ''' <param name="sec"></param>
        <ExportAPI("sleep")>
        Public Sub sleep(sec As Integer)
            Call Thread.Sleep(sec * 1000)
        End Sub
    End Module
End Namespace
