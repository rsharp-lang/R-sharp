Imports SMRUCC.Rsharp.Runtime.Components.Configuration
Imports SMRUCC.Rsharp.Runtime.Package

Namespace Runtime

    ''' <summary>
    ''' R#之中的全局环境对象
    ''' </summary>
    Public Class GlobalEnvironment : Inherits Environment

        Public ReadOnly Property options As Options
        Public ReadOnly Property packages As LocalPackageDatabase

        Sub New(packages As LocalPackageDatabase, options As Options)
            Me.packages = packages
            Me.options = options
        End Sub

    End Class
End Namespace