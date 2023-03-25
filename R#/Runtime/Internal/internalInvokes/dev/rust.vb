Imports Microsoft.VisualBasic.ApplicationServices.DynamicInterop
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' the rust language helper
    ''' </summary>
    Public Module rust

        Private Function SolveCLibrary(x As String, env As Environment) As String
            If x.FileExists Then
                Return x
            ElseIf TypeOf env Is PackageEnvironment Then
                Dim pkg As PackageEnvironment = env
                Dim dir As String = $"{pkg.libpath}/lib_c/"
                Dim fileName As String = $"{dir}/{x}"

                If fileName.FileExists Then
                    Return fileName
                Else
                    GoTo FIND_RENV
                End If
            Else
FIND_RENV:
                For Each dir As String In New String() {
                    App.HOME,
                    $"{App.HOME}/lib/",
                    $"{App.HOME}/lib_c/",
                    $"{App.HOME}/../lib/",
                    $"{App.HOME}/../lib_c/"
                }
                    If $"{dir}/{x}".FileExists Then
                        Return $"{dir}/{x}"
                    End If
                Next

                Return x
            End If
        End Function

        ''' <summary>
        ''' ### Foreign Function Interface
        ''' 
        ''' Load or unload DLLs (also known as shared objects), and test whether a 
        ''' C function or Fortran subroutine is available.
        ''' </summary>
        ''' <returns></returns>
        ''' <param name="x">
        ''' a character string giving the pathname to a DLL, also known as a dynamic 
        ''' shared object. (See ‘Details’ for what these terms mean.)
        ''' </param>
        <ExportAPI("dyn.load")>
        Public Function dynLoad(x As String, Optional env As Environment = Nothing) As Object
            Dim dll As New UnmanagedDll(dllName:=SolveCLibrary(x, env))
            Dim globalEnv = env.globalEnvironment
            Dim libc_key As String = x.BaseName

            ' hook current library to runtime environment
            globalEnv.nativeLibraries(libc_key) = dll

            Return dll
        End Function

        ''' <summary>
        ''' create an integer scalar value
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        <ExportAPI("i32")>
        Public Function i32(x As Object) As Integer
            Return CLRVector.asInteger(x).First
        End Function

        <ExportAPI("string")>
        Public Function [string](s As Object) As String
            Return CLRVector.asCharacter(s).First
        End Function
    End Module
End Namespace