#Region "Microsoft.VisualBasic::e470880f7f54fe9e5cb2af8aaefe7274, R#\System\Components\RSessionInfo.vb"

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

    '   Total Lines: 137
    '    Code Lines: 75
    ' Comment Lines: 44
    '   Blank Lines: 18
    '     File Size: 5.25 KB


    '     Class RSessionInfo
    ' 
    '         Properties: activators, basePkgs, BLAS, environment_variables, LAPACK
    '                     loadedOnly, locale, matprod, output_device, platform
    '                     RNGkind, running, Rversion
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Development.Components

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
        Public Property output_device As String
        Public Property environment_variables As list
        ''' <summary>
        ''' a character string, the result of calling Sys.getlocale().
        ''' </summary>
        ''' <returns></returns>
        Public Property locale As list
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
        Public Property activators As list
        ''' <summary>
        ''' (not always present): a named list of the results of calling 
        ''' packageDescription on packages whose namespaces are loaded 
        ''' but are not attached.
        ''' </summary>
        ''' <returns></returns>
        Public Property loadedOnly As String()
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
            Dim maxColumns As Integer = 120
            Dim i As i32 = Scan0

            Call info.AppendLine(Rversion.GetString("version.string"))
            Call info.AppendLine($"Platform: {Rversion.GetString("platform")} ({Rversion.GetString("arch")})")
            Call info.AppendLine($"Running under: {Rversion.GetString("system")} ({If(App.IsMicrosoftPlatform, "Microsoft", "UNIX")})")
            Call info.AppendLine($"Environment device: {output_device}")
            Call info.AppendLine()
            Call info.AppendLine($"Matrix products: {matprod}")
            Call info.AppendLine()
            Call info.AppendLine("locale:")

            For Each attr In locale.slots.SeqIterator(offset:=1)
                Call info.AppendLine($"[{attr.i}] {attr.value.Key}={attr.value.Value}")
            Next

            Call info.AppendLine()
            Call info.AppendLine("attached R# packages:")

            Using output As New StringWriter(info)
                Call output.WriteLine("<R base>")

                If basePkgs.IsNullOrEmpty Then
                    Call output.WriteLine("none attached...")
                Else
                    Call basePkgs.printContentArray(Nothing, Nothing, maxColumns, output)
                End If

                Call output.WriteLine()
                Call output.WriteLine("<loaded>")

                If loadedOnly.IsNullOrEmpty Then
                    Call output.WriteLine("none attached...")
                Else
                    Call loadedOnly.printContentArray(Nothing, Nothing, maxColumns, output)
                End If

                Call output.Flush()
            End Using

            Call info.AppendLine()
            Call info.AppendLine("activators:")

            If activators.length > 0 Then
                For Each attr In activators.slots.SeqIterator(offset:=1)
                    Call info.AppendLine($"[{attr.i}] {attr.value.Key}={attr.value.Value}")
                Next
            Else
                Call info.AppendLine("nothing")
            End If

            Call info.AppendLine()
            Call info.AppendLine("Environment variables:")

            i = 1

            For Each config In environment_variables.slots
                Call info.AppendLine($"[{++i}] {config.Key}={config.Value}")
            Next

            Return info.ToString
        End Function
    End Class
End Namespace
