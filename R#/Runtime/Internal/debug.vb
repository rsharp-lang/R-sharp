Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.My
Imports Microsoft.VisualBasic.Text
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Internal.Invokes

Namespace Runtime.Internal

    Public NotInheritable Class debug

        ''' <summary>
        ''' 啰嗦模式下会输出一些调试信息
        ''' </summary>
        ''' <returns></returns>
        Public Shared Property verbose As Boolean = False

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 只在<see cref="verbose"/>啰嗦模式下才会工作
        ''' </summary>
        ''' <param name="message$"></param>
        ''' <param name="color"></param>
        Public Shared Sub write(message$, Optional color As ConsoleColor = ConsoleColor.White)
            If verbose Then
                Call VBDebugger.WaitOutput()
                Call Log4VB.Print(message & ASCII.LF, color)
            End If
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function [stop](message As Object, envir As Environment) As Message
            Return base.stop(message, envir)
        End Function
    End Class
End Namespace