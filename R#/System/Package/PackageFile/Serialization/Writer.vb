Imports System.IO
Imports System.Text
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Closure
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.DataSets
Imports SMRUCC.Rsharp.Interpreter.ExecuteEngine.ExpressionSymbols.Operators
Imports SMRUCC.Rsharp.Runtime.Components
Imports SMRUCC.Rsharp.System.Package.File.Expressions
Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics

Namespace System.Package.File

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <remarks>
    ''' expression: [ExpressionTypes, i32][dataSize, i32][TypeCodes, byte][expressionData, bytes]
    '''              4                     4              1                ...
    ''' </remarks>
    Public Class Writer : Implements IDisposable

        Dim buffer As BinaryWriter
        Dim disposedValue As Boolean

        Public Const Magic As String = "SMRUCC/R#"

        Public ReadOnly Property RSymbol As RSymbol
        Public ReadOnly Property RLiteral As RLiteral
        Public ReadOnly Property RBinary As RBinary
        Public ReadOnly Property RCallFunction As RCallFunction
        Public ReadOnly Property RFunction As RFunction
        Public ReadOnly Property RImports As RRequire
        Public ReadOnly Property RUnary As RUnary
        Public ReadOnly Property RVector As RVector

        Sub New(buffer As Stream)
            Me.buffer = New BinaryWriter(buffer)

            Me.RSymbol = New RSymbol(Me)
            Me.RLiteral = New RLiteral(Me)
            Me.RBinary = New RBinary(Me)
            Me.RCallFunction = New RCallFunction(Me)
            Me.RImports = New RRequire(Me)
            Me.RUnary = New RUnary(Me)
            Me.RVector = New RVector(Me)
            Me.RFunction = New RFunction(Me)
        End Sub

        Public Function GetBuffer(x As Expression) As Byte()
            Select Case x.GetType
                Case GetType(DeclareNewSymbol) : Return RSymbol.GetBuffer(x)
                Case GetType(DeclareNewFunction) : Return RFunction.GetBuffer(x)
                Case GetType(Literal) : Return RLiteral.GetBuffer(x)
                Case GetType(BinaryOrExpression),
                     GetType(BinaryBetweenExpression),
                     GetType(BinaryInExpression),
                     GetType(AppendOperator)

                    Return RBinary.GetBuffer(x)

                Case GetType(FunctionInvoke) : Return RCallFunction.GetBuffer(x)
                Case GetType(Require), GetType([Imports]) : Return RImports.GetBuffer(x)
                Case GetType(UnaryNot) : Return RUnary.GetBuffer(x)
                Case GetType(VectorLiteral) : Return RVector.GetBuffer(x)
                Case Else
                    Throw New NotImplementedException(x.GetType.FullName)
            End Select
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns>
        ''' 函数返回表达式的长度
        ''' </returns>
        Public Function Write(x As Expression) As Integer
            Dim buffer As Byte() = GetBuffer(x)
            Call Me.buffer.Write(buffer)
            Return buffer.Length
        End Function

        Public Shared Function GetBuffer(sourceMap As StackFrame) As Byte()
            Using ms As New MemoryStream, outfile As New BinaryWriter(ms)
                Call outfile.Write(0)
                Call outfile.Write(Encoding.ASCII.GetBytes(sourceMap.File))
                Call outfile.Write(CByte(0))
                Call outfile.Write(Encoding.ASCII.GetBytes(sourceMap.Line))
                Call outfile.Write(CByte(0))
                Call outfile.Write(Encoding.ASCII.GetBytes(sourceMap.Method.Namespace))
                Call outfile.Write(CByte(0))
                Call outfile.Write(Encoding.ASCII.GetBytes(sourceMap.Method.Module))
                Call outfile.Write(CByte(0))
                Call outfile.Write(Encoding.ASCII.GetBytes(sourceMap.Method.Method))

                Call outfile.Flush()
                Call ms.Seek(Scan0, SeekOrigin.Begin)
                Call outfile.Write(ms.Length - 4)

                Return ms.ToArray
            End Using
        End Function

        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not disposedValue Then
                If disposing Then
                    ' TODO: dispose managed state (managed objects)
                    Call buffer.Flush()
                    Call buffer.Close()
                End If

                ' TODO: free unmanaged resources (unmanaged objects) and override finalizer
                ' TODO: set large fields to null
                disposedValue = True
            End If
        End Sub

        ' ' TODO: override finalizer only if 'Dispose(disposing As Boolean)' has code to free unmanaged resources
        ' Protected Overrides Sub Finalize()
        '     ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
        '     Dispose(disposing:=False)
        '     MyBase.Finalize()
        ' End Sub

        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code. Put cleanup code in 'Dispose(disposing As Boolean)' method
            Dispose(disposing:=True)
            GC.SuppressFinalize(Me)
        End Sub
    End Class
End Namespace