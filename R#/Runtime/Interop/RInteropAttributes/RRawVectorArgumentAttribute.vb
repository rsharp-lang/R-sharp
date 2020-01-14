Namespace Runtime.Interop

    Public Interface IVectorExpressionLiteral

        Function ParseVector(default$, schema As Type) As Array

    End Interface

    ''' <summary>
    ''' 表示这个参数是一个数组，环境系统不应该自动调用getFirst取第一个值
    ''' </summary>
    <AttributeUsage(AttributeTargets.Parameter, AllowMultiple:=False, Inherited:=True)>
    Public Class RRawVectorArgumentAttribute : Inherits RInteropAttribute

        ''' <summary>
        ''' The element type of the target vector type
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' If this property is not null, then it means the optional argument have 
        ''' a default string expression value which could be parsed as current vector
        ''' type.
        ''' </remarks>
        Public ReadOnly Property vector As Type
        Public ReadOnly Property parser As Type

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="vector">The element type of the target vector type</param>
        ''' <param name="parser">
        ''' <see cref="IVectorExpressionLiteral"/>
        ''' 
        ''' use <see cref="DefaultVectorParser"/> by default.
        ''' </param>
        Sub New(Optional vector As Type = Nothing, Optional parser As Type = Nothing)
            Me.vector = vector
            Me.parser = If(parser, GetType(DefaultVectorParser))
        End Sub

        Public Function GetVector([default] As String) As Array
            Dim literal As IVectorExpressionLiteral = DirectCast(Activator.CreateInstance(parser), IVectorExpressionLiteral)
            Dim vector As Array = literal.ParseVector([default], schema:=Me.vector)

            Return vector
        End Function
    End Class

    Public Structure DefaultVectorParser : Implements IVectorExpressionLiteral

        Public Function ParseVector(default$, schema As Type) As Array Implements IVectorExpressionLiteral.ParseVector
            Select Case schema
                Case GetType(String)
                    Return [default]?.Split("|"c)
                Case GetType(Double)
                    Return [default]?.Split(","c).Select(AddressOf Val).ToArray
                Case Else
                    Throw New NotImplementedException(schema.FullName)
            End Select
        End Function
    End Structure
End Namespace