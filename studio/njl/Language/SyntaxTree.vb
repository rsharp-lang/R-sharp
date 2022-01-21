Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.SyntaxParser
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Language

    Public Class SyntaxTree

        ReadOnly script As Rscript
        ReadOnly debug As Boolean = False
        ReadOnly scanner As JlScanner
        ReadOnly opts As SyntaxBuilderOptions
        ReadOnly stack As New Stack(Of JuliaCodeDOM)
        ReadOnly julia As New JuliaCodeDOM With {
            .keyword = "python",
            .level = -1,
            .script = New List(Of Expression)
        }

        ''' <summary>
        ''' current python code dom node
        ''' </summary>
        Dim current As JuliaCodeDOM

        <DebuggerStepThrough>
        Sub New(script As Rscript, Optional debug As Boolean = False)
            Me.debug = debug
            Me.script = script
            Me.scanner = New JlScanner(script.script)
            Me.opts = New SyntaxBuilderOptions(AddressOf ParseJuliaLine) With {
                .source = script,
                .debug = debug
            }
        End Sub

        Public Function ParseJlScript() As Program

        End Function
    End Class
End Namespace