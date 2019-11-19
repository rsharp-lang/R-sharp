#Region "Microsoft.VisualBasic::f2871917fdb327e513228145b18aa5cd, R#\Runtime\Internal\base.vb"

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

    '     Module base
    ' 
    '         Function: [get], [stop], all, any, cat
    '                   createDotNetExceptionMessage, createMessageInternal, getEnvironmentStack, lapply, names
    '                   options, print, sapply, warning
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Logging
Imports Microsoft.VisualBasic.Emit.Delegates
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports devtools = Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Runtime.Internal

    ''' <summary>
    ''' 在这个模块之中仅包含有最基本的数据操作函数
    ''' </summary>
    Public Module base

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function any(test As Object) As Object
            Return Runtime.asLogical(test).Any(Function(b) b = True)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function all(test As Object) As Object
            Return Runtime.asLogical(test).All(Function(b) b = True)
        End Function

        ''' <summary>
        ''' ###### Options Settings
        ''' 
        ''' Allow the user to set and examine a variety of global options which 
        ''' affect the way in which R computes and displays its results.
        ''' </summary>
        ''' <param name="opts">
        ''' any options can be defined, using name = value. However, only the ones below are used in base R.
        ''' Options can also be passed by giving a Single unnamed argument which Is a named list.
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function options(opts As Object, envir As Environment) As Object
            Dim configs As Options = envir.GlobalEnvironment.options

            For Each value In DirectCast(opts, list).slots
                Try
                    configs.setOption(value.Key, value.Value)
                Catch ex As Exception
                    Return Internal.stop(ex, envir)
                End Try
            Next

            Return opts
        End Function

        Public Function [get](x As Object, envir As Environment) As Object
            Dim name As String = Runtime.asVector(Of Object)(x) _
                .DoCall(Function(o)
                            Return Scripting.ToString(Runtime.getFirst(o), null:=Nothing)
                        End Function)

            If name.StringEmpty Then
                Return Internal.stop("NULL value provided for object name!", envir)
            End If

            Dim symbol As Variable = envir.FindSymbol(name)

            If symbol Is Nothing Then
                Return Message.SymbolNotFound(envir, name, TypeCodes.generic)
            Else
                Return symbol.value
            End If
        End Function

        Public Function names([object] As Object, namelist As Object, envir As Environment) As Object
            If namelist Is Nothing OrElse Runtime.asVector(Of Object)(namelist).Length = 0 Then
                ' get names
                Select Case [object].GetType
                    Case GetType(list), GetType(dataframe)
                        Return DirectCast([object], RNames).getNames
                    Case Else
                        Return Internal.stop("unsupported!", envir)
                End Select
            Else
                ' set names
                Select Case [object].GetType
                    Case GetType(list), GetType(dataframe)
                        Return DirectCast([object], RNames).setNames(namelist, envir)
                    Case Else
                        Return Internal.stop("unsupported!", envir)
                End Select
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="message">
        ''' <see cref="String"/> array or <see cref="Exception"/>
        ''' </param>
        ''' <param name="envir"></param>
        ''' <returns></returns>
        Public Function [stop](message As Object, envir As Environment) As Message
            If Not message Is Nothing AndAlso message.GetType.IsInheritsFrom(GetType(Exception), strict:=False) Then
                Return DirectCast(message, Exception).createDotNetExceptionMessage(envir)
            Else
                Return base.createMessageInternal(message, envir, level:=MSG_TYPES.ERR)
            End If
        End Function

        <Extension>
        Private Function createDotNetExceptionMessage(ex As Exception, envir As Environment) As Message
            Dim messages As New List(Of String)
            Dim exception As Exception = ex

            Do While Not ex Is Nothing
                messages += ex.GetType.Name & ": " & ex.Message
                ex = ex.InnerException
            Loop

            ' add stack info for display
            If exception.StackTrace.StringEmpty Then
                messages += "stackFrames: " & vbCrLf & "none"
            Else
                messages += "stackFrames: " & vbCrLf & exception.StackTrace
            End If

            Return New Message With {
                .Message = messages,
                .EnvironmentStack = envir.getEnvironmentStack,
                .MessageLevel = MSG_TYPES.ERR,
                .Trace = devtools.ExceptionData.GetCurrentStackTrace
            }
        End Function

        <Extension>
        Friend Function getEnvironmentStack(parent As Environment) As StackFrame()
            Dim frames As New List(Of StackFrame)

            Do While Not parent Is Nothing
                frames += New StackFrame With {
                    .Method = New Method With {
                        .Method = parent.stackTag
                    }
                }
                parent = parent.parent
            Loop

            Return frames
        End Function

        Private Function createMessageInternal(messages As Object, envir As Environment, level As MSG_TYPES) As Message
            Return New Message With {
                .Message = Runtime.asVector(Of Object)(messages) _
                    .AsObjectEnumerator _
                    .SafeQuery _
                    .Select(Function(o) Scripting.ToString(o, "NULL")) _
                    .ToArray,
                .MessageLevel = level,
                .EnvironmentStack = envir.getEnvironmentStack,
                .Trace = devtools.ExceptionData.GetCurrentStackTrace
            }
        End Function

        Public Function warning(message As Object, envir As Environment) As Message
            Return createMessageInternal(message, envir, level:=MSG_TYPES.WRN)
        End Function

        Public Function cat(values As Object, file As String, sep As String) As Object
            Dim strs = Runtime.asVector(Of Object)(values) _
                .AsObjectEnumerator _
                .Select(Function(o) Scripting.ToString(o, "")) _
                .JoinBy(sep) _
                .DoCall(AddressOf sprintf)

            If Not file.StringEmpty Then
                Call strs.SaveTo(file)
            Else
                Call Console.Write(strs)
            End If

            Return strs
        End Function

        Public Function print(x As Object, envir As Environment) As Object
            If x Is Nothing Then
                Call Console.WriteLine("[1] NULL")
            ElseIf x.GetType.ImplementInterface(GetType(RPrint)) Then
                Try
                    Call Console.WriteLine(DirectCast(x, RPrint).GetPrintContent)
                Catch ex As Exception
                    Return Internal.stop(ex, envir)
                End Try
            ElseIf x.GetType Is GetType(Message) Then
                Return x
            ElseIf x.GetType Is GetType(list) Then
                Call printer.printInternal(DirectCast(x, list).slots, "")
            Else
                Call printer.printInternal(x, "")
            End If

            Return x
        End Function

        Public Function lapply(sequence As Object, apply As RFunction, envir As Environment) As Object
            If sequence.GetType Is GetType(Dictionary(Of String, Object)) Then
                Return DirectCast(sequence, Dictionary(Of String, Object)) _
                    .ToDictionary(Function(d) d.Key,
                                  Function(d)
                                      Return apply.Invoke(envir, {d.Value})
                                  End Function)
            Else
                Return Runtime.asVector(Of Object)(sequence) _
                    .AsObjectEnumerator _
                    .SeqIterator _
                    .ToDictionary(Function(i) $"[[{i.i}]]",
                                  Function(d)
                                      Return apply.Invoke(envir, {d.value})
                                  End Function)
            End If
        End Function

        Public Function sapply(sequence As Object, apply As RFunction, envir As Environment) As Object
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
