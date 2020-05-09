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

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

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
        Public Function sessionInfo(env As Environment) As RSessionInfo

        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("R.Version")>
        Public Function RVer(env As Environment) As list

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
        Public Property basePkgs As String
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
            Return MyBase.ToString()
        End Function
    End Class
End Namespace
