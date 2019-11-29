Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports SMRUCC.Rsharp.Interpreter
Imports SMRUCC.Rsharp.Runtime.Components.Interface
Imports SMRUCC.Rsharp.Runtime.Internal.ConsolePrinter

Namespace Runtime.Internal.Invokes.LinqPipeline
    Friend Structure Group : Implements RNameIndex

        Public key As Object
        Public group As Array

        Shared Sub New()
            Call printer.AttachConsoleFormatter(Of Group)(Function(group) group.ToString)
        End Sub

        Public Overrides Function ToString() As String
            Return vbCrLf &
                $" '{group.Length}' elements with key: " & printer.ValueToString(key) & vbCrLf &
                group.AsObjectEnumerator _
                    .Select(Function(o)
                                Return "   " & printer.ValueToString(o)
                            End Function) _
                    .JoinBy(vbCrLf)
        End Function

        Public Function getByName(name As String) As Object Implements RNameIndex.getByName
            If name = NameOf(key) Then
                Return key
            ElseIf name = NameOf(group) Then
                Return group
            Else
                Return Nothing
            End If
        End Function

        Public Function getByName(names() As String) As Object Implements RNameIndex.getByName
            Return names.Select(AddressOf getByName).ToArray
        End Function

        Public Function setByName(name As String, value As Object, envir As Environment) As Object Implements RNameIndex.setByName
            If name = NameOf(key) Then
                key = value
            ElseIf name = NameOf(group) Then
                group = Runtime.asVector(Of Object)(value)
            Else
                Return Internal.stop(New InvalidOperationException, envir)
            End If

            Return value
        End Function

        Public Function setByName(names() As String, value As Array, envir As Environment) As Object Implements RNameIndex.setByName
            Dim values As New List(Of Object)
            Dim val As Object
            Dim i As i32 = Scan0

            For Each name As String In names
                val = setByName(name, value.GetValue(++i), envir)

                If Program.isException(val) Then
                    Return val
                Else
                    values.Add(val)
                End If
            Next

            Return values.ToArray
        End Function

        Public Function getNames() As String() Implements IReflector.getNames
            Return {NameOf(key), NameOf(group)}
        End Function
    End Structure
End Namespace