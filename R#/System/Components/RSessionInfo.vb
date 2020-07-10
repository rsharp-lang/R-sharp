Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace System.Components

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
            Call info.AppendLine($"Running under: {Rversion.GetString("system")}")
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