
Imports System.IO
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Runtime
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.Runtime.Interop
Imports REnv = SMRUCC.Rsharp.Runtime

<Package("io")>
Public Module io

    ''' <summary>
    ''' Apply the function f to the result of open(args...; kwargs...) 
    ''' and close the resulting file descriptor upon completion.
    ''' </summary>
    ''' <param name="f"></param>
    ''' <param name="filename"></param>
    ''' <param name="mode"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("open")>
    Public Function open(f As Object,
                         Optional filename As String = Nothing,
                         Optional mode As String = "r",
                         Optional env As Environment = Nothing) As Object

        If TypeOf f Is String Then
            mode = filename
            filename = f
        End If

        If mode = "w" Then
            Return New StreamWriter(filename.Open(FileMode.OpenOrCreate, doClear:=True, [readOnly]:=False))
        ElseIf mode = "a" Then
            Return New StreamWriter(filename.Open(FileMode.OpenOrCreate, doClear:=False, [readOnly]:=False))
        Else
            Dim r As New StreamReader(filename.Open(FileMode.Open, doClear:=False, [readOnly]:=True))

            If TypeOf f Is DeclareLambdaFunction Then
                Return DirectCast(f, DeclareLambdaFunction).Invoke(env, {New InvokeParameter(r, Scan0)})
            Else
                Return r
            End If
        End If
    End Function

    ''' <summary>
    ''' open for writing
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="x"></param>
    ''' <param name="env"></param>
    ''' <returns></returns>
    <ExportAPI("write")>
    Public Function write(file As StreamWriter, <RRawVectorArgument> x As Object,
                          Optional env As Environment = Nothing)

        Dim vec As Array = REnv.TryCastGenericArray(REnv.asVector(Of Object)(x), env)
        Dim type As Type = vec.GetType.GetElementType

        Select Case type
            Case GetType(String)
                For Each str As String In DirectCast(vec, String())
                    Call file.Write(str)
                Next
            Case GetType(Char)
                Call file.Write(New String(DirectCast(vec, Char())))
            Case GetType(Integer)
                For Each i As Integer In DirectCast(vec, Integer())
                    Call file.Write(i)
                Next
            Case GetType(Long)
                For Each l As Long In DirectCast(vec, Long())
                    Call file.Write(l)
                Next
            Case GetType(Double)
                For Each d As Double In DirectCast(vec, Double())
                    Call file.Write(d)
                Next
            Case GetType(Single)
                For Each s As Single In DirectCast(vec, Single())
                    Call file.Write(s)
                Next
            Case Else
                Throw New NotImplementedException(type.ToString)
        End Select

        Return Nothing
    End Function

    ''' <summary>
    ''' open for reading
    ''' </summary>
    ''' <param name="file"></param>
    ''' <param name="type"></param>
    ''' <returns></returns>
    <ExportAPI("read")>
    Public Function read(file As StreamReader, <RSymbolTextArgument> Optional type As String = "String") As Object
        Select Case LCase(type)
            Case "string"
                Return file.ReadToEnd
            Case Else
                Throw New NotImplementedException(type)
        End Select
    End Function

    '''' <summary>
    '''' Close an I/O stream. Performs a flush first.
    '''' </summary>
    '''' <param name="resource"></param>
    '<ExportAPI("close")>
    'Public Sub close(resource As IDisposable)
    '    Call flush(resource)
    '    Call resource.Dispose()
    'End Sub

    ''' <summary>
    ''' Commit all currently buffered writes to the given stream.
    ''' </summary>
    ''' <param name="resource"></param>
    <ExportAPI("flush")>
    Public Sub flush(resource As Object)
        If TypeOf resource Is Stream Then
            Call DirectCast(resource, Stream).Flush()
        End If
    End Sub
End Module
