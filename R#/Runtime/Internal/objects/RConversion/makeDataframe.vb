Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace Runtime.Internal.Object.Converts

    Public Delegate Function IMakeDataFrame(x As Object, env As Environment) As dataframe

    Module makeDataframe

        ReadOnly makesDataframe As New Dictionary(Of Type, IMakeDataFrame)

        Sub New()
            makesDataframe(GetType(ExceptionData)) = AddressOf TracebackDataFrmae
        End Sub

        Public Function is_ableConverts(type As Type) As Boolean
            Return makesDataframe.ContainsKey(type)
        End Function

        Public Function createDataframe(type As Type, x As Object, env As Environment) As dataframe
            Return makesDataframe(type)(x, env)
        End Function

        Public Function TracebackDataFrmae(data As Object, env As Environment) As dataframe
            Dim stacktrace As StackFrame()

            If TypeOf data Is ExceptionData Then
                stacktrace = DirectCast(data, ExceptionData).StackTrace
            ElseIf TypeOf data Is StackFrame() Then
                stacktrace = DirectCast(data, StackFrame())
            Else
                Throw New NotImplementedException
            End If

            Dim package As Array = stacktrace.Select(Function(a) a.Method.Namespace).ToArray
            Dim [module] As Array = stacktrace.Select(Function(a) a.Method.Module).ToArray
            Dim name As Array = stacktrace.Select(Function(a) a.Method.Method).ToArray
            Dim file As Array = stacktrace.Select(Function(a) a.File).ToArray
            Dim line As Array = stacktrace.Select(Function(a) a.Line).ToArray
            Dim dataframe As New dataframe With {
                .columns = New Dictionary(Of String, Array) From {
                    {NameOf(package), package},
                    {NameOf([module]), [module]},
                    {NameOf(name), name},
                    {NameOf(file), file},
                    {NameOf(line), line}
                }
            }

            Return dataframe
        End Function

    End Module
End Namespace