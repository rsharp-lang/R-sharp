Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports Microsoft.VisualBasic.ApplicationServices.Development.NetCore5
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Object

Namespace Development.Hybrids

    Public Class HybridsEngine

        ReadOnly engine As New Dictionary(Of String, ScriptLoader)
        ReadOnly suffix As New List(Of String)

        Sub New()
            Call Register(New HybridsREngine)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function CanHandle(scriptfile As String) As Boolean
            Return scriptfile.ExtensionSuffix(suffix.ToArray)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function LoadScript(scriptfile As String, env As Environment) As Object
            Dim key As String = scriptfile.ExtensionSuffix.ToLower
            Dim loader As ScriptLoader = engine(key)

            Return loader.LoadScript(scriptfile, env)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dllpath">
        ''' dll full path
        ''' </param>
        ''' <returns></returns>
        Public Function Register(dllpath As String) As HybridsEngine
            Dim types As Type() = deps.LoadAssemblyOrCache(dllFile:=dllpath).GetTypes

            For Each type As Type In types
                If type.IsInheritsFrom(GetType(ScriptLoader)) Then
                    Call Me.Register(DirectCast(Activator.CreateInstance(type), ScriptLoader))
                End If
            Next

            Return Me
        End Function

        Public Function Register(loader As ScriptLoader) As HybridsEngine
            engine(loader.SuffixName.ToLower) = loader
            suffix.Add(loader.SuffixName.ToLower)

            Return Me
        End Function
    End Class

    Public MustInherit Class ScriptLoader

        Public MustOverride ReadOnly Property SuffixName As String

        Public MustOverride Function ParseScript(scriptfile As String, env As Environment) As [Variant](Of Message, Program)
        Public MustOverride Function LoadScript(scriptfile As String, env As Environment) As Object

    End Class

    Public Class HybridsREngine : Inherits ScriptLoader

        Public Overrides ReadOnly Property SuffixName As String
            Get
                Return "R"
            End Get
        End Property

        Public Overrides Function LoadScript(scriptfile As String, env As Environment) As Object
            Dim R As RInterpreter = env.globalEnvironment.Rscript
            ' 20200213 因为source函数是创建了一个新的环境容器
            ' 所以函数无法被导入到全局环境之中
            ' 在这里imports关键词操作则是使用全局环境
            Dim script As MagicScriptSymbol = CreateMagicScriptSymbol(scriptfile, R)
            Dim Rscript As Rscript = Rscript.FromFile(scriptfile)
            Dim stackframe As New StackFrame With {
                .File = Rscript.fileName,
                .Line = 0,
                .Method = New Method With {
                    .Method = MethodBase.GetCurrentMethod.Name,
                    .[Module] = "n/a",
                    .[Namespace] = "SMRUCC/R#"
                }
            }

            Call env.setStackInfo(stackframe)

            If env.FindSymbol("!script") Is Nothing Then
                env.Push("!script", New vbObject(script), [readonly]:=False)
            Else
                env.FindSymbol("!script").SetValue(New vbObject(script), env)
            End If

            Dim parsed = ParseScript(scriptfile, env)

            If parsed Like GetType(Message) Then
                Return parsed.TryCast(Of Message)
            Else
                Return parsed.TryCast(Of Program).Execute(env)
            End If
        End Function

        Public Overrides Function ParseScript(scriptfile As String, env As Environment) As [Variant](Of Message, Program)
            Dim Rscript As Rscript = Rscript.FromFile(scriptfile)
            Dim R As RInterpreter = env.globalEnvironment.Rscript
            Dim error$ = Nothing
            Dim program As Program = Program.CreateProgram(Rscript, R.debug, [error]:=[error])

            If program Is Nothing Then
                ' there are syntax error in the external script
                ' for current imports action
                Return Internal.debug.stop([error].Trim(ASCII.CR, ASCII.LF, " "c, ASCII.TAB), env)
            Else
                Return program
            End If
        End Function
    End Class
End Namespace