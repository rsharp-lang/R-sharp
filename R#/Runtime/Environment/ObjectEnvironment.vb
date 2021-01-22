Imports Microsoft.VisualBasic.ApplicationServices.Debugging.Diagnostics
Imports SMRUCC.Rsharp.Runtime.Components

Namespace Runtime

    ''' <summary>
    ''' helper for object with syntax implements
    ''' </summary>
    Public Class ObjectEnvironment : Inherits Environment

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 优先应用对象的成员
        ''' </remarks>
        Default Public Overrides Property value(name As String) As Symbol
            Get
                Return MyBase.value(name)
            End Get
            Set(value As Symbol)
                MyBase.value(name) = value
            End Set
        End Property

        ''' <summary>
        ''' target object
        ''' </summary>
        ReadOnly target As Object

        Public Sub New(target As Object, parent As Environment, stackFrame As StackFrame)
            MyBase.New(parent, stackFrame, isInherits:=False)

            Me.target = target
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <param name="[inherits]"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 优先应用对象的成员
        ''' </remarks>
        Public Overrides Function FindSymbol(name As String, Optional [inherits] As Boolean = True) As Symbol
            Return MyBase.FindSymbol(name, [inherits])
        End Function
    End Class
End Namespace