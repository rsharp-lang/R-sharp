#Region "Microsoft.VisualBasic::68c2268d1ac086d7154c1224976a317c, D:/GCModeller/src/R-sharp/R#//Runtime/Internal/internalInvokes/dev/devtools.vb"

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

    '   Total Lines: 188
    '    Code Lines: 105
    ' Comment Lines: 62
    '   Blank Lines: 21
    '     File Size: 8.24 KB


    '     Module devtools
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: fetchProfileData, flash_load, FNV1aHash, getDllPath, profilerFrames
    '                   stringHashCode
    ' 
    '         Sub: gc, raiseException, raiseThreadException
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.Repository
Imports Microsoft.VisualBasic.ValueTypes
Imports SMRUCC.Rsharp.Development.Components
Imports SMRUCC.Rsharp.Development.Package
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object
Imports SMRUCC.Rsharp.Runtime.Interop

Namespace Runtime.Internal.Invokes

    Public Class NoInspector

        Public Property obj As Object

        Public Overrides Function ToString() As String
            Return $"<&H0_{obj.GetHashCode.ToHexString}> {StringFormats.Lanudry(objectSize(obj))}"
        End Function

        Public Shared Function Wrap(any As Object) As NoInspector
            Return New NoInspector With {.obj = any}
        End Function

    End Class

    Module devtools

        Sub Main()
            Call Internal.Object.Converts.addHandler(GetType(ProfilerFrames), AddressOf profilerFrames)
            Call Internal.Object.Converts.addHandler(GetType(Program), AddressOf scriptTable)
            Call Internal.Object.Converts.addHandler(GetType(ClosureExpression), AddressOf scriptTable1)
        End Sub

        Private Function scriptTable1(closure As ClosureExpression, args As list, env As Environment) As dataframe
            Return scriptTable(closure.program, args, env)
        End Function

        Private Function scriptTable(prog As Program, args As list, env As Environment) As dataframe
            Dim df As New dataframe With {.columns = New Dictionary(Of String, Array)}
            Dim str_width As Integer = 81

            Call df.add("name", prog.Select(Function(exp) exp.expressionName))
            Call df.add("clr_type", prog.Select(Function(exp) exp.GetType.Name))
            Call df.add("is_callable", prog.Select(Function(exp) exp.isCallable))
            Call df.add("symbol", prog.Select(Function(exp) InvokeParameter.GetSymbolName(exp)))
            Call df.add("mode", prog.Select(Function(exp) exp.type))
            Call df.add("expr_raw", prog.Select(Function(exp) NoInspector.Wrap(exp)))
            Call df.add(
                key:="expr_str",
                value:=prog _
                    .Select(Function(exp)
                                Dim str = exp.ToString.TrimNewLine

                                If str.Length > str_width Then
                                    Return str.Substring(0, str_width) & "..."
                                Else
                                    Return str
                                End If
                            End Function))
            Return df
        End Function

        Private Function profilerFrames(data As ProfilerFrames, args As list, env As Environment) As dataframe
            Dim frames As New dataframe With {
                .columns = New Dictionary(Of String, Array),
                .rownames = Enumerable _
                    .Range(1, data.size) _
                    .Select(Function(i) i.ToString) _
                    .ToArray
            }

            frames.columns("time") = data.profiles.Select(Function(f) f.tag.FromUnixTimeStamp).ToArray
            frames.columns("ticks") = data.profiles.Select(Function(f) f.elapse_time).ToArray
            frames.columns("elapse_time") = data.profiles.Select(Function(f) TimeSpan.FromTicks(f.elapse_time)).ToArray
            frames.columns("memory_delta") = data.profiles.Select(Function(f) f.memory_delta).ToArray
            frames.columns("memory_size") = data.profiles.Select(Function(f) f.memory_size).ToArray
            frames.columns("namespace") = data.profiles.Select(Function(f) f.stackframe.Method.Namespace).ToArray
            frames.columns("module") = data.profiles.Select(Function(f) f.stackframe.Method.Module).ToArray
            frames.columns("function") = data.profiles.Select(Function(f) f.stackframe.Method.Method).ToArray
            frames.columns("expression") = data.profiles.Select(Function(f) f.expression).ToArray
            frames.columns("file") = data.profiles.Select(Function(f) f.stackframe.File).ToArray
            frames.columns("line") = data.profiles.Select(Function(f) f.stackframe.Line).ToArray

            Return frames
        End Function

        <ExportAPI("symbol_value")>
        Public Function getSymbolvalue(exp As Expression) As Object
            If TypeOf exp Is DeclareNewSymbol Then
                Return DirectCast(exp, DeclareNewSymbol).value
            Else
                Return Nothing
            End If
        End Function

        <ExportAPI("invoke_parameters")>
        Public Function getInvokeParameters(exp As Expression) As Object
            If TypeOf exp Is FunctionInvoke Then
                Return DirectCast(exp, FunctionInvoke).parameters
            Else
                Return Nothing
            End If
        End Function

        <ExportAPI("strHashcode")>
        Public Function stringHashCode(str As String) As Integer
            Return str.GetDeterministicHashCode
        End Function

        <ExportAPI("FNV1aHashcode")>
        Public Function FNV1aHash(<RRawVectorArgument> [set] As Object, Optional env As Environment = Nothing) As Integer
            Return FNV1a.GetHashCode(getObjectSet([set], env))
        End Function

        ''' <summary>
        ''' Load R script in directory
        ''' 
        ''' Load all of the R script in a given working directory,
        ''' by default is load all script in current directory.
        ''' </summary>
        ''' <param name="dir">The script source directory, by default is current workspace.</param>
        <ExportAPI("flash_load")>
        Public Function flash_load(<RDefaultExpression> Optional dir As String = "~getwd()", Optional env As GlobalEnvironment = Nothing) As Object
            Dim Rlist As String() = dir _
                .EnumerateFiles("*.r", "*.R") _
                .Select(Function(path) path.GetFullPath) _
                .Distinct _
                .ToArray

            For Each script As String In Rlist
                Try
                    Dim err As Object = env.Rscript.Source(script)

                    If Program.isException(err) Then
                        Call debug.PrintMessageInternal(DirectCast(err, Message), env)
                    End If
                Catch ex As Exception
                    Call base.print($"Error while loading script: {script}", , env)
                    Call App.LogException(ex)
                    Call base.print(ex, , env)
                End Try
            Next

            Dim zzz As String = $"{dir}/zzz.R"

            If zzz.FileExists Then
                Call env.doCall(".onLoad")
            End If

            Return New invisible()
        End Function

        ''' <summary>
        ''' get performance profile data
        ''' </summary>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("profiler.fetch")>
        Public Function fetchProfileData(Optional env As Environment = Nothing) As ProfilerFrames
            Dim globalEnv As GlobalEnvironment = env.globalEnvironment

            If globalEnv.profiler2.Count > 0 Then
                Return globalEnv.profiler2.Pop
            Else
                Return Nothing
            End If
        End Function

        ''' <summary>
        ''' ## Garbage Collection
        ''' 
        ''' A call of gc causes a garbage collection to take place. 
        ''' gcinfo sets a flag so that automatic collection is 
        ''' either silent (verbose = FALSE) or prints memory usage 
        ''' statistics (verbose = TRUE).
        ''' </summary>
        ''' <remarks>
        ''' A call of gc causes a garbage collection to take place. 
        ''' This will also take place automatically without user 
        ''' intervention, and the primary purpose of calling gc is 
        ''' for the report on memory usage. For an accurate report 
        ''' full = TRUE should be used.
        ''' It can be useful To Call gc after a large Object has 
        ''' been removed, As this may prompt R To Return memory To 
        ''' the operating system.
        ''' R allocates space For vectors In multiples Of 8 bytes: 
        ''' hence the report Of "Vcells", a relic Of an earlier 
        ''' allocator (that used a vector heap).
        ''' When gcinfo(TRUE) Is in force, messages are sent to the 
        ''' message connection at each garbage collection of the 
        ''' form:
        ''' 
        ''' ```
        '''     Garbage collection 12 = 10+0+2 (level 0) ...
        '''     6.4 Mbytes of cons cells used (58%)
        '''     2.0 Mbytes of vectors used (32%)
        ''' ```
        ''' 
        ''' Here the last two lines give the current memory usage 
        ''' rounded up To the Next 0.1Mb And As a percentage Of the 
        ''' current trigger value. The first line gives a breakdown 
        ''' Of the number Of garbage collections at various levels 
        ''' (For an explanation see the 'R Internals’ manual).
        ''' </remarks>
        <ExportAPI("gc")>
        Public Sub gc()
            Call App.FlushMemory()
        End Sub

        ''' <summary>
        ''' get library dll file path
        ''' </summary>
        ''' <param name="files"></param>
        ''' <param name="env"></param>
        ''' <returns></returns>
        <ExportAPI("GetDllFile")>
        Public Function getDllPath(<RRawVectorArgument> files As Object, Optional env As Environment = Nothing) As Object
            Return EvaluateFramework(Of String, String)(
                env:=env,
                x:=files,
                eval:=Function(name)
                          Return LibDLL.getDllFromAppDir(name, env.globalEnvironment, Nothing).GetFullPath
                      End Function
            )
        End Function

        ''' <summary>
        ''' this function will throw a .NET exception for run exception handler demo test
        ''' </summary>
        ''' <param name="message"></param>
        <ExportAPI("raiseException")>
        Public Sub raiseException(message As String)
            Throw New Exception(message)
        End Sub

        ''' <summary>
        ''' this function will throw a .NET exception on another thread for run exception handler demo test
        ''' </summary>
        ''' <param name="message"></param>
        <ExportAPI("raiseThreadException")>
        Public Sub raiseThreadException(message As String)
            Dim popErr As New Threading.ThreadStart(Sub() Throw New Exception(message))
            Dim thread As New Thread(popErr)

            Call thread.Start()
        End Sub
    End Module
End Namespace
