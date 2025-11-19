
Imports System.Threading
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Interop

<Package("Rsession")>
Module Rsession

    Public cts As CancellationTokenSource

    ''' <summary>
    ''' # Terminate an R Session
    ''' 
    ''' The function ``quit`` or its alias ``q`` terminate the current R session.
    ''' </summary>
    ''' <param name="save">
    ''' a character string indicating whether the environment (workspace) should be saved, 
    ''' one of ``"no"``, ``"yes"``, ``"ask"`` or ``"default"``.
    ''' </param>
    ''' <param name="status">
    ''' the (numerical) error status to be returned to the operating system, where relevant. 
    ''' Conventionally 0 indicates successful completion.
    ''' </param>
    ''' <param name="runLast">
    ''' should ``.Last()`` be executed?
    ''' </param>
    ''' <keywords>terminal,interactive</keywords>
    <ExportAPI("quit")>
    Public Sub quit(Optional save$ = "no",
                    Optional status% = 0,
                    Optional runLast As Boolean = True,
                    Optional envir As Environment = Nothing)

        Call q(save, status, runLast, envir)
    End Sub

    ''' <summary>
    ''' # Terminate an R Session
    ''' 
    ''' The function ``quit`` or its alias ``q`` terminate the current R session.
    ''' </summary>
    ''' <param name="save">
    ''' a character string indicating whether the environment (workspace) should be saved, 
    ''' one of ``"no"``, ``"yes"``, ``"ask"`` or ``"default"``.
    ''' </param>
    ''' <param name="status">
    ''' the (numerical) error status to be returned to the operating system, where relevant. 
    ''' Conventionally 0 indicates successful completion.
    ''' </param>
    ''' <param name="runLast">
    ''' should ``.Last()`` be executed?
    ''' </param>
    ''' 
    <ExportAPI("q")>
    Public Sub q(Optional save$ = "default",
                 Optional status% = 0,
                 Optional runLast As Boolean = True,
                 Optional envir As Environment = Nothing)

        ' null string will be return if ctrl+C was pressed
        save = Microsoft.VisualBasic.Strings.LCase(save)

        If save = "default" OrElse save = "ask" Then
RE0:
            Call Console.Write("Save workspace image? [y/n/c]: ")

            save = Microsoft.VisualBasic.Strings _
                .Trim(Console.ReadLine) _
                .Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB)
        End If

        If save = "c" Then
            ' cancel
            Return
        ElseIf cts IsNot Nothing AndAlso cts.IsCancellationRequested Then
            Call Console.WriteLine()
            Return
        End If

        If save = "y" OrElse save = "yes" Then
            ' save image for yes
            Dim saveImage As Symbol = envir.FindSymbol("save.image")

            If Not saveImage Is Nothing AndAlso TypeOf saveImage.value Is RMethodInfo Then
                Call DirectCast(saveImage.value, RMethodInfo).Invoke(envir, {})
            End If
        ElseIf save = "n" OrElse save = "no" Then
            ' do nothing for no
        Else
            GoTo RE0
        End If

        If runLast Then
            Dim last = envir.FindSymbol(".Last")

            If Not last Is Nothing Then
                Call DirectCast(last, RFunction).Invoke(envir, {})
            End If
        End If

        Call App.Exit(status)
    End Sub
End Module
