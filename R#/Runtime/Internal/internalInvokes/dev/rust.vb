Imports Microsoft.VisualBasic.ApplicationServices.DynamicInterop
Imports Microsoft.VisualBasic.CommandLine.Reflection

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' the rust language helper
    ''' </summary>
    Public Module rust

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
            Dim dll As New UnmanagedDll(dllName:=x)
            Dim globalEnv = env.globalEnvironment

            Call globalEnv.nativeLibraries.Add(x.BaseName, dll)

            Return Nothing
        End Function
    End Module
End Namespace