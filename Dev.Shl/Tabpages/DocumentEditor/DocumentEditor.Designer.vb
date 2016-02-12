Namespace DocumentEditor

    <Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
    Partial Class DocumentEditor
        Inherits System.Windows.Forms.UserControl

        'UserControl overrides dispose to clean up the component list.
        <System.Diagnostics.DebuggerNonUserCode()> _
        Protected Overrides Sub Dispose( disposing As Boolean)
            Try
                If disposing AndAlso components IsNot Nothing Then
                    components.Dispose()
                End If
            Finally
                MyBase.Dispose(disposing)
            End Try
        End Sub

        'Required by the Windows Form Designer
        Private components As System.ComponentModel.IContainer

        'NOTE: The following procedure is required by the Windows Form Designer
        'It can be modified using the Windows Form Designer.  
        'Do not modify it using the code editor.
        <System.Diagnostics.DebuggerStepThrough()> _
        Private Sub InitializeComponent()
            Me.components = New System.ComponentModel.Container()
            Me.DocumentEditor1 = New Pavel.CodeEditor.DocumentEditor()
            Me.Timer1 = New System.Windows.Forms.Timer(Me.components)
            CType(Me.DocumentEditor1, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'DocumentEditor1
            '
            Me.DocumentEditor1.AutoCompleteBracketsList = New Char() {Global.Microsoft.VisualBasic.ChrW(40), Global.Microsoft.VisualBasic.ChrW(41), Global.Microsoft.VisualBasic.ChrW(123), Global.Microsoft.VisualBasic.ChrW(125), Global.Microsoft.VisualBasic.ChrW(91), Global.Microsoft.VisualBasic.ChrW(93), Global.Microsoft.VisualBasic.ChrW(34), Global.Microsoft.VisualBasic.ChrW(34), Global.Microsoft.VisualBasic.ChrW(39), Global.Microsoft.VisualBasic.ChrW(39)}
            Me.DocumentEditor1.AutoScrollMinSize = New System.Drawing.Size(147, 14)
            Me.DocumentEditor1.BackBrush = Nothing
            Me.DocumentEditor1.CharHeight = 14
            Me.DocumentEditor1.CharWidth = 8
            Me.DocumentEditor1.Cursor = System.Windows.Forms.Cursors.IBeam
            Me.DocumentEditor1.DisabledColor = System.Drawing.Color.FromArgb(CType(CType(100, Byte), Integer), CType(CType(180, Byte), Integer), CType(CType(180, Byte), Integer), CType(CType(180, Byte), Integer))
            Me.DocumentEditor1.Dock = System.Windows.Forms.DockStyle.Fill
            Me.DocumentEditor1.IsReplaceMode = False
            Me.DocumentEditor1.Location = New System.Drawing.Point(0, 0)
            Me.DocumentEditor1.Name = "DocumentEditor1"
            Me.DocumentEditor1.Paddings = New System.Windows.Forms.Padding(0)
            Me.DocumentEditor1.SelectionColor = System.Drawing.Color.FromArgb(CType(CType(60, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(0, Byte), Integer), CType(CType(255, Byte), Integer))
            Me.DocumentEditor1.Size = New System.Drawing.Size(479, 321)
            Me.DocumentEditor1.TabIndex = 0
            Me.DocumentEditor1.Text = "DocumentEditor1"
            Me.DocumentEditor1.Zoom = 100
            '
            'Timer1
            '
            Me.Timer1.Enabled = True
            Me.Timer1.Interval = 1000
            '
            'DocumentEditor
            '
            Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 12.0!)
            Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
            Me.Controls.Add(Me.DocumentEditor1)
            Me.Name = "DocumentEditor"
            Me.Size = New System.Drawing.Size(479, 321)
            CType(Me.DocumentEditor1, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)

        End Sub
        Friend WithEvents DocumentEditor1 As Pavel.CodeEditor.DocumentEditor
        Friend WithEvents Timer1 As System.Windows.Forms.Timer

    End Class
End Namespace