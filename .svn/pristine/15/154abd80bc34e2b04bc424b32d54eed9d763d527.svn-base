Imports Microsoft.VisualBasic.Shoal
Imports Microsoft.VisualBasic.Scripting.ShoalShell.Runtime.Debugging

Module Program

    Dim _ScriptEngine As Microsoft.VisualBasic.Scripting.ShoalShell.Runtime.ScriptEngine
    Dim _Interpreter As Microsoft.VisualBasic.CommandLine.Interpreter

    Public Property Settings As Conf
    Public Property ScriptDebugger As DebuggerListener

    Public Property ShellOpenFile As String

    Public ReadOnly Property ScriptEngine As Microsoft.VisualBasic.Scripting.ShoalShell.Runtime.ScriptEngine
        Get
            Return _ScriptEngine
        End Get
    End Property

    Public Sub Initialize(cmdl As CommandLine.CommandLine)
        _ScriptEngine = New Scripting.ShoalShell.Runtime.ScriptEngine
        Settings = Conf.DefaultFile.LoadXml(Of Conf)()

        If Settings Is Nothing Then
            Settings = New Conf
        End If

        If String.IsNullOrEmpty(Settings.Debugger) OrElse Not FileIO.FileSystem.FileExists(Settings.Debugger) Then

        Else
            _ScriptDebugger = New DebuggerListener(Settings.Debugger, "")
        End If

        If Not cmdl.IsNullOrEmpty Then

            If cmdl.Parameters.IsNullOrEmpty AndAlso FileIO.FileSystem.FileExists(cmdl.Name) Then
                ShellOpenFile = cmdl.Name
            Else
                _Interpreter = Microsoft.VisualBasic.CommandLine.Interpreter.CreateInstance(Of Program.CommandLines)()
                Call Application.Exit()
            End If
        End If
    End Sub

    Public Sub Finalize()
        Call ScriptDebugger.Dispose()
        Call Settings.GetXml.SaveTo(Conf.DefaultFile)
    End Sub

    Private Class CommandLines

    End Class
End Module
