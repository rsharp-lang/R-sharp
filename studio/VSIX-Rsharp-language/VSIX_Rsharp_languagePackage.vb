#Region "Microsoft.VisualBasic::a040d94fc961f7ff062798b45985d890, studio\VSIX-Rsharp-language\VSIX_Rsharp_languagePackage.vb"

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

    ' Class VSIX_Rsharp_languagePackage
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Runtime.InteropServices
Imports System.Threading
Imports Microsoft.VisualBasic
Imports Microsoft.VisualStudio.Shell
Imports Task = System.Threading.Tasks.Task


''' <summary>
''' This is the class that implements the package exposed by this assembly.
''' </summary>
''' <remarks>
''' <para>
''' The minimum requirement for a class to be considered a valid package for Visual Studio
''' Is to implement the IVsPackage interface And register itself with the shell.
''' This package uses the helper classes defined inside the Managed Package Framework (MPF)
''' to do it: it derives from the Package Class that provides the implementation Of the 
''' IVsPackage interface And uses the registration attributes defined in the framework to 
''' register itself And its components with the shell. These attributes tell the pkgdef creation
''' utility what data to put into .pkgdef file.
''' </para>
''' <para>
''' To get loaded into VS, the package must be referred by &lt;Asset Type="Microsoft.VisualStudio.VsPackage" ...&gt; in .vsixmanifest file.
''' </para>
''' </remarks>
<PackageRegistration(UseManagedResourcesOnly:=True, AllowsBackgroundLoading:=True)>
<Guid(VSIX_Rsharp_languagePackage.PackageGuidString)>
Public NotInheritable Class VSIX_Rsharp_languagePackage
    Inherits AsyncPackage

    ''' <summary>
    ''' Package guid
    ''' </summary>
    Public Const PackageGuidString As String = "96b2757c-03c3-492a-91f1-9920c5795bb0"

#Region "Package Members"

    ''' <summary>
    ''' Initialization of the package; this method is called right after the package is sited, so this is the place
    ''' where you can put all the initialization code that rely on services provided by VisualStudio.
    ''' </summary>
    ''' <param name="cancellationToken">A cancellation token to monitor for initialization cancellation, which can occur when VS is shutting down.</param>
    ''' <param name="progress">A provider for progress updates.</param>
    ''' <returns>A task representing the async work of package initialization, or an already completed task if there is none. Do not return null from this method.</returns>
    Protected Overrides Async Function InitializeAsync(cancellationToken As CancellationToken, progress As IProgress(Of ServiceProgressData)) As Task
        ' When initialized asynchronously, the current thread may be a background thread at this point.
        ' Do any initialization that requires the UI thread after switching to the UI thread.
        Await Me.JoinableTaskFactory.SwitchToMainThreadAsync()
    End Function

#End Region

End Class

