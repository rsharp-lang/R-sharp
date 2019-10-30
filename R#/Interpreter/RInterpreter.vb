Imports System.IO
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Language
Imports SMRUCC.Rsharp.Runtime

Namespace Interpreter

    Public Class RInterpreter

        ''' <summary>
        ''' Global runtime environment.(全局环境)
        ''' </summary>
        Public ReadOnly Property globalEnvir As New Environment
        Public ReadOnly Property warnings As New List(Of Message)

        Default Public ReadOnly Property GetValue(name As String) As Object
            Get
                Return globalEnvir(name).value
            End Get
        End Property

        Public Const lastVariableName$ = "$"

        Sub New()
            Call globalEnvir.Push(lastVariableName, Nothing, TypeCodes.generic)
        End Sub

        Public Sub PrintMemory(Optional dev As TextWriter = Nothing)
            Dim table$()() = globalEnvir _
                .Select(Function(v)
                            Dim value$ = Variable.GetValueViewString(v)

                            Return {
                                v.name,
                                v.typeCode.ToString,
                                v.typeof.FullName,
                                $"[{v.length}] {value}"
                            }
                        End Function) _
                .ToArray

            With dev Or App.StdOut
                Call .DoCall(Sub(device)
                                 Call table.PrintTable(
                                    dev:=device,
                                    leftMargin:=3,
                                    title:={"name", "mode", "typeof", "value"}
                                 )
                             End Sub)
            End With
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(name$, value As Object, Optional type As TypeCodes = TypeCodes.generic)
            Call globalEnvir.Push(name, value, type)
        End Sub

        Public Sub Add(name$, closure As [Delegate])
            globalEnvir.Push(name, New RMethodInfo(name, closure), TypeCodes.closure)
        End Sub

        Public Sub Add(name$, closure As MethodInfo, Optional target As Object = Nothing)
            globalEnvir.Push(name, New RMethodInfo(name, closure, target), TypeCodes.closure)
        End Sub

        Public Function Invoke(funcName$, ParamArray args As Object()) As Object
            Dim symbol = globalEnvir.FindSymbol(funcName)

            If symbol Is Nothing Then
                Throw New EntryPointNotFoundException($"No object named '{funcName}' could be found in global environment!")
            ElseIf symbol.typeCode <> TypeCodes.closure OrElse Not symbol.typeof.ImplementInterface(GetType(RFunction)) Then
                Throw New InvalidProgramException($"Object '{funcName}' is not a function!")
            End If

            Return DirectCast(symbol.value, RFunction).Invoke(globalEnvir, args)
        End Function

        ''' <summary>
        ''' Run R# script program from text data.
        ''' </summary>
        ''' <param name="script">The script text</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluate(script As String) As Object
            Return RunInternal(script, Nothing, {})
        End Function

        Private Function InitializeEnvironment(source$, arguments As NamedValue(Of Object)()) As Environment
            Dim envir As Environment

            If source Is Nothing Then
                envir = globalEnvir
            Else
                envir = New Environment(globalEnvir, source)
            End If

            For Each var As NamedValue(Of Object) In arguments
                Call envir.Push(var.Name, var.Value)
            Next

            Return envir
        End Function

        Private Function RunInternal(script$, source$, arguments As NamedValue(Of Object)()) As Object
            Dim globalEnvir As Environment = InitializeEnvironment(source, arguments)
            Dim result As Object = Code.ParseScript(script).RunProgram(globalEnvir)
            Dim last As Variable = Me.globalEnvir(lastVariableName)

            If Program.isException(result) Then
                result = printErrorInternal(message:=result)
            Else
                ' set last variable in current environment
                last.value = result
            End If

            Return result
        End Function

        ''' <summary>
        ''' Run R# script program from a given script file 
        ''' </summary>
        ''' <param name="filepath">The script file path.</param>
        ''' <param name="arguments"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Source(filepath$, ParamArray arguments As NamedValue(Of Object)()) As Object
            Return RunInternal(filepath.ReadAllText, filepath.ToFileURL, arguments)
        End Function

        Private Function printErrorInternal(message As Message) As Object
            Dim execRoutine$ = message.EnvironmentStack _
                .Reverse _
                .Select(Function(frame) frame.Method.Method) _
                .JoinBy(" -> ")
            Dim i As i32 = 1
            Dim backup = Console.ForegroundColor

            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine($" Error in {execRoutine}")

            For Each msg As String In message
                Console.WriteLine($"  {++i}. {msg}")
            Next

            Console.ForegroundColor = backup

            Return Nothing
        End Function

        Public Shared ReadOnly Property Rsharp As New RInterpreter

        Public Shared Function Evaluate(script$, ParamArray args As NamedValue(Of Object)()) As Object
            SyncLock Rsharp
                With Rsharp
                    If Not args.IsNullOrEmpty Then
                        Dim name$
                        Dim value As Object

                        For Each var As NamedValue(Of Object) In args
                            name = var.Name
                            value = var.Value

                            Call .globalEnvir.Push(name, value, NameOf(TypeCodes.generic))
                        Next
                    End If

                    Return .Evaluate(script)
                End With
            End SyncLock
        End Function
    End Class
End Namespace