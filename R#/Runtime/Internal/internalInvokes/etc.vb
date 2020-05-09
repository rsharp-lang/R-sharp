#Region "Microsoft.VisualBasic::d1fc2044b06206e91c590abd9f7ca56f, R#\Runtime\Internal\internalInvokes\etc.vb"

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

'     Module etc
' 
'         Function: contributors, license
' 
'         Sub: demo
' 
' 
' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices.Development
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports Win32 = System.Environment

Namespace Runtime.Internal.Invokes

    Module etc

        ''' <summary>
        ''' # The R# License Terms
        ''' 
        ''' The license terms under which R# is distributed.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("license")>
        Public Function license() As <RSuppressPrint> Object
            Call Console.WriteLine(Rsharp.LICENSE.GPL3)
            Return Nothing
        End Function

        ''' <summary>
        ''' # ``R#`` Project Contributors
        ''' 
        ''' The R# Who-is-who, describing who made significant contributions to the development of R#.
        ''' </summary>
        ''' <returns></returns>
        <ExportAPI("contributors")>
        Public Function contributors() As <RSuppressPrint> Object
            Call Console.WriteLine(My.Resources.contributions)
            Return Nothing
        End Function

        <ExportAPI("demo")>
        Public Sub demo()

        End Sub

        ''' <summary>
        ''' ### Extract System and User Information
        ''' 
        ''' Reports system and user information.
        ''' </summary>
        ''' <returns>
        ''' A character vector with fields
        '''
        ''' + ``sysname`` The operating system name.
        ''' + ``release`` The OS release.
        ''' + ``version`` The OS version.
        ''' + ``nodename`` A name by which the machine Is known On 
        '''     the network (If any).
        ''' + ``machine`` A concise description Of the hardware, 
        '''     often the CPU type.
        ''' + ``login`` The user 's login name, or "unknown" if it 
        '''     cannot be ascertained.
        ''' + ``user`` The name Of the real user ID, Or "unknown" If 
        '''     it cannot be ascertained.
        ''' + ``effective_user`` The name Of the effective user ID, Or 
        '''     "unknown" If it cannot be ascertained. This may differ 
        '''     from the real user In 'set-user-ID’ processes.
        '''
        ''' The last three fields give the same value On Windows.
        ''' </returns>
        <ExportAPI("Sys.info")>
        Public Function Sys_info() As list
            Return New list With {
                .slots = New Dictionary(Of String, Object)
            }
        End Function
        ''' ### Collect Information About the Current R Session
        ''' 
        ''' Print version information about R, the OS and attached or 
        ''' loaded packages.
        ''' </summary>
        ''' <returns>
        ''' sessionInfo() returns an object of class "sessionInfo" which has 
        ''' print and toLatex methods. This is a list with components
        ''' </returns>
        <ExportAPI("sessionInfo")>
        <RApiReturn(GetType(RSessionInfo))>
        Public Function sessionInfo(env As Environment) As vbObject
            Dim info As New RSessionInfo With {
                .Rversion = RVer(env)
            }

            Return New vbObject(info)
        End Function

        ''' <summary>
        ''' ### Version Information
        ''' 
        ''' R.Version() provides detailed information about the version of R running.
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns>
        ''' R.Version returns a list with character-string components
        '''
        ''' + ``platform`` the platform For which R was built. A triplet Of the form 
        '''    CPU-VENDOR-OS, As determined by the configure script. E.g, 
        '''    "i686-unknown-linux-gnu" Or "i386-pc-mingw32".
        ''' + ``arch`` the architecture(CPU) R was built On/For.
        ''' + ``os`` the underlying operating system.
        ''' + ``system`` CPU And OS, separated by a comma.
        ''' + ``status`` the status Of the version (e.g., "alpha")
        ''' + ``major`` the major version number
        ''' + ``minor`` the minor version number, including the patchlevel
        ''' + ``year`` the year the version was released
        ''' + ``month`` the month the version was released
        ''' + ``day`` the day the version was released
        ''' + ``svn rev`` the Subversion revision number, which should be either "unknown" 
        '''    Or a Single number. (A range Of numbers Or a number With M Or S appended 
        '''    indicates inconsistencies In the sources used To build this version Of R.)
        ''' + ``language`` always "R".
        ''' </returns>
        <ExportAPI("R.Version")>
        Public Function RVer(env As Environment) As list
            Dim core As AssemblyInfo = GetType(Environment).Assembly.FromAssembly
            Dim version As Version = Version.Parse(core.AssemblyVersion)
            Dim built As DateTime = core.BuiltTime

            Return New list With {
                .slots = New Dictionary(Of String, Object) From {
                    {"platform", "Microsoft VisualStudio 2019/.NET Framework v4.8/Microsoft VisualBasic.NET"},
                    {"arch", "x86_64"},
                    {"os", ".NET Framework v4.8"},
                    {"system", Win32.OSVersion.VersionString},
                    {"status", Win32.OSVersion.ServicePack},
                    {"major", version.Major},
                    {"minor", version.Minor},
                    {"year", built.Year},
                    {"month", built.Month},
                    {"day", built.Day},
                    {"svn rev", version.Revision},
                    {"language", "R#"},
                    {"version.string", $"R version {core.AssemblyVersion} ({built.ToString})"},
                    {"nickname", "R-sharp"}
                }
            }
        End Function
    End Module

    Public Class RSessionInfo

        ''' <summary>
        ''' a list, the result of calling R.Version().
        ''' </summary>
        ''' <returns></returns>
        <RNameAlias("R.version")>
        Public Property Rversion As list
        ''' <summary>
        ''' a character string describing the platform R was 
        ''' built under. Where sub-architectures are in use 
        ''' this is of the form platform/sub-arch (nn-bit).
        ''' </summary>
        ''' <returns></returns>
        Public Property platform As String
        ''' <summary>
        ''' a character string, the result of calling Sys.getlocale().
        ''' </summary>
        ''' <returns></returns>
        Public Property locale As String
        ''' <summary>
        ''' a character string (or possibly NULL), the same as osVersion, see below.
        ''' </summary>
        ''' <returns></returns>
        Public Property running As String
        ''' <summary>
        ''' a character vector, the result of calling RNGkind().
        ''' </summary>
        ''' <returns></returns>
        Public Property RNGkind As String
        ''' <summary>
        ''' a character vector of base packages which are attached.
        ''' </summary>
        ''' <returns></returns>
        Public Property basePkgs As String()
        ''' <summary>
        ''' (not always present): a named list of the results of calling 
        ''' packageDescription on packages whose namespaces are loaded 
        ''' but are not attached.
        ''' </summary>
        ''' <returns></returns>
        Public Property loadedOnly As String
        ''' <summary>
        ''' a character string, the result of calling getOption("matprod").
        ''' </summary>
        ''' <returns></returns>
        Public Property matprod As String
        ''' <summary>
        ''' a character string, the result of calling extSoftVersion()["BLAS"].
        ''' </summary>
        ''' <returns></returns>
        Public Property BLAS As String
        ''' <summary>
        ''' a character string, the result of calling La_library().
        ''' </summary>
        ''' <returns></returns>
        Public Property LAPACK As String

        Public Overrides Function ToString() As String
            Dim info As New StringBuilder

            Call info.AppendLine(Rversion.GetString("version.string"))
            Call info.AppendLine($"Platform: {Rversion.GetString("platform")} ({Rversion.GetString("arch")})")
            Call info.AppendLine($"Running under: {Rversion.GetString("system")}")
            Call info.AppendLine()
            Call info.AppendLine($"Matrix products: {matprod}")
            Call info.AppendLine()

            Return info.ToString
        End Function
    End Class
End Namespace
