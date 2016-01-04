Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.ShoalShell

<[Namespace]("dynamics.ide_plugins")>
Public Module IDEPlugIn

    Dim CommandName As String
    Dim Iconpath As String
    Dim Target As System.Windows.Forms.ToolStripMenuItem
    Dim _currentPath As String()

    <ExportAPI("plugin.set_name")>
    Public Function SetName(value As String) As String
        CommandName = value
        Return IDEPlugIn.CommandName
    End Function

    <ExportAPI("plugin.set_icon")>
    Public Function SetIcon(value As String) As String
        Try
            IDEPlugIn.Iconpath = FileIO.FileSystem.GetFileInfo(value).FullName
        Catch ex As Exception
            Call Console.WriteLine("FILE_NOT_FOUND::menu icon image file ""{0}"" is not found on the filesystem, image will not load.", value)
        End Try
        Return IDEPlugIn.Iconpath
    End Function

    <ExportAPI("plugin.initialize", info:="This method should be the last command that you call in the shellscript")>
    Public Function Initialize(action As System.Action) As Boolean
        Dim CommandEntry = PlugIn.PlugInLoader.AddCommand(Target, _currentPath, CommandName, 0)
        If Not String.IsNullOrEmpty(Iconpath) AndAlso FileIO.FileSystem.FileExists(Iconpath) Then CommandEntry.Image = System.Drawing.Image.FromFile(Iconpath)
        AddHandler CommandEntry.Click, Sub() Call action()       '关联命令

        Return 0
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Entry">插件的载入点</param>
    ''' <param name="pluginDir">ShellScript插件脚本的文件夹路径</param>
    ''' <remarks></remarks>
    Public Sub LoadScripts(Entry As System.Windows.Forms.ToolStripMenuItem, pluginDir As String)
        Target = Entry
        pluginDir = FileIO.FileSystem.GetDirectoryInfo(pluginDir).FullName

        Using ShellScriptHost As Microsoft.VisualBasic.Scripting.ShoalShell.Runtime.ScriptEngine = New Scripting.ShoalShell.Runtime.ScriptEngine
            For Each ShellScript As String In FileIO.FileSystem.GetFiles(pluginDir, FileIO.SearchOption.SearchAllSubDirectories, "*.txt", "*.vbss")
                _currentPath = ShellScript.Replace(pluginDir, "").Split(CChar("\"))
                _currentPath = _currentPath.Take(_currentPath.Count - 1).ToArray
                Call ShellScriptHost.Exec(ShellScript)
            Next
        End Using
    End Sub
End Module
