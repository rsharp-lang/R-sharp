#Region "Microsoft.VisualBasic::eaf981dd1090d9dbaf72f5a7a4c68775, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/dev/rust.vb"

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

    '   Total Lines: 191
    '    Code Lines: 103
    ' Comment Lines: 67
    '   Blank Lines: 21
    '     File Size: 8.31 KB


    '     Module rust
    ' 
    '         Function: [string], dynLoad, f32, f64, i32
    '                   i64, Rcall, SolveCLibrary
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ApplicationServices.DynamicInterop
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop
Imports SMRUCC.Rsharp.Runtime.Vectorization

Namespace Runtime.Internal.Invokes

    ''' <summary>
    ''' the rust language helper
    ''' </summary>
    ''' 
    <Package("rust")>
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
        ''' Modern Interfaces to VisualBasic.NET code
        ''' 
        ''' Functions to pass R# objects to compiled VisualBasic.NET code that has been loaded into R#.
        ''' </summary>
        ''' <param name="NAME">
        ''' a character string giving the name of a C function, or an object 
        ''' of class "NativeSymbolInfo", "RegisteredNativeSymbol" or 
        ''' "NativeSymbol" referring to such a name.</param>
        ''' <param name="PACKAGE">
        ''' If supplied, confine the search For a character String .NAME To 
        ''' the DLL given by this argument (plus the conventional extension, 
        ''' '.so’, ‘.dll’, ...).
        ''' This argument follows ... And so its name cannot be abbreviated.
        ''' This Is intended to add safety for packages, which can ensure by 
        ''' using this argument that no other package can override their 
        ''' external symbols, And also speeds up the search (see 'Note’).
        ''' </param>
        ''' <param name="args">
        ''' arguments to be passed to the compiled code. Up to 65 for .Call.
        ''' </param>
        ''' <param name="env"></param>
        ''' <returns>An R object constructed in the compiled code.</returns>
        <ExportAPI(".Call")>
        Public Function Rcall(NAME As String, PACKAGE As String,
                              <RListObjectArgument>
                              Optional args As list = Nothing,
                              Optional env As Environment = Nothing) As Object

            ' # This file was generated by Rcpp::compileAttributes
            ' # Generator token: 10BE3573-1514-4C36-9D1C-5A225CD40393
            '
            ' jaccard_coeff <- function(idx) {
            '    .Call('Rphenograph_jaccard_coeff', PACKAGE = 'Rphenograph', idx)
            ' }
            Dim dll As UnmanagedDll = env.globalEnvironment.nativeLibraries.TryGetValue(PACKAGE)
            Dim hashcode As String = $"{dll}!{NAME}"
            ' make a delegate type at here
            ' the parameter type is generated from the arguments
            ' and also then parameter order is generated from the argument order in the list
            Dim pinvoke As New RPInvoke(arguments:=args)

            If dll Is Nothing Then
                Return Internal.debug.stop({
                    $"Error in .Call(""{NAME}"", PACKAGE = ""{PACKAGE}""): ""{NAME}"" not available for .Call() from native library ""{PACKAGE}""",
                    $"symbol_name: {NAME}",
                    $"native_library: {PACKAGE}"
                }, env)
            End If

            Static get_native_function As MethodInfo = GetType(UnmanagedDll) _
                .GetMethods _
                .Where(Function(m)
                           Return m.Name = NameOf(UnmanagedDll.GetFunction) AndAlso
                                  m.GetParameters.Length = 2
                       End Function) _
                .First

            ' C native library
            ' MY_C_API_MARKER void Play(void* simulation, const char* variableIdentifier, double* values, TimeSeriesGeometry* geom);
            '
            ' .net clr
            ' private delegate void Play_csdelegate(IntPtr simulation, string variableIdentifier, IntPtr values, IntPtr geom);
            ' // and somewhere in a class a field is set:
            ' NativeLib = new UnmanagedDll(someNativeLibFilename);

            ' void Play_cs(IModelSimulation simulation, string variableIdentifier, double[] values, ref MarshaledTimeSeriesGeometry geom)
            ' {
            '    IntPtr values_doublep, geom_struct;
            '    // here glue code here to create native arrays/structs
            '    NativeLib.GetFunction<Play_csdelegate>("Play")(CheckedDangerousGetHandle(simulation, "simulation"), variableIdentifier, values_doublep, geom_struct);
            '    // here copy args back to managed; clean up transient native resources.
            ' }
            Static native_func_list As New Dictionary(Of String, [Delegate])

            Return native_func_list.ComputeIfAbsent(
                key:=hashcode,
                lazyValue:=Function()
                               Dim del As Type = pinvoke.GetDelegate
                               ' run function with reflection
                               Dim native_func As [Delegate] = get_native_function.Invoke(
                                   obj:=dll,
                                   parameters:=New Object() {NAME, del}
                               )

                               Return native_func
                           End Function) _
                                         _
                .DynamicInvoke(pinvoke.Values)
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

        <ExportAPI("i64")>
        Public Function i64(x As Object) As Long
            Return CLRVector.asLong(x).First
        End Function

        <ExportAPI("f32")>
        Public Function f32(x As Object) As Single
            Return CLRVector.asNumeric(x).First
        End Function

        <ExportAPI("f64")>
        Public Function f64(x As Object) As Double
            Return CLRVector.asNumeric(x).First
        End Function
    End Module
End Namespace
