<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Partial Class FormIDEMain
    Inherits Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()>
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
    <System.Diagnostics.DebuggerStepThrough()>
    Private Sub InitializeComponent()
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FormIDEMain))
        Me.StatusStrip1 = New System.Windows.Forms.StatusStrip()
        Me.Ribbon1 = New System.Windows.Forms.Ribbon()
        Me.RibbonOrbMenuItem1 = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.RibbonOrbMenuItem2 = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.RibbonOrbMenuItem3 = New System.Windows.Forms.RibbonOrbMenuItem()
        Me.RibbonButton1 = New System.Windows.Forms.RibbonButton()
        Me.RibbonTab1 = New System.Windows.Forms.RibbonTab()
        Me.RibbonTab2 = New System.Windows.Forms.RibbonTab()
        Me.RibbonTab3 = New System.Windows.Forms.RibbonTab()
        Me.MultipleTabpagePanel1 = New Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel()
        Me.RibbonSeparator1 = New System.Windows.Forms.RibbonSeparator()
        Me.RibbonOrbRecentItem1 = New System.Windows.Forms.RibbonOrbRecentItem()
        Me.RibbonOrbOptionButton1 = New System.Windows.Forms.RibbonOrbOptionButton()
        Me.SuspendLayout()
        '
        'StatusStrip1
        '
        Me.StatusStrip1.Location = New System.Drawing.Point(0, 709)
        Me.StatusStrip1.Name = "StatusStrip1"
        Me.StatusStrip1.Size = New System.Drawing.Size(1146, 22)
        Me.StatusStrip1.TabIndex = 3
        Me.StatusStrip1.Text = "StatusStrip1"
        '
        'Ribbon1
        '
        Me.Ribbon1.Font = New System.Drawing.Font("Segoe UI", 9.0!)
        Me.Ribbon1.Location = New System.Drawing.Point(0, 0)
        Me.Ribbon1.Minimized = False
        Me.Ribbon1.Name = "Ribbon1"
        '
        '
        '
        Me.Ribbon1.OrbDropDown.BorderRoundness = 8
        Me.Ribbon1.OrbDropDown.Location = New System.Drawing.Point(0, 0)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.RibbonOrbMenuItem1)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.RibbonOrbMenuItem2)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.RibbonOrbMenuItem3)
        Me.Ribbon1.OrbDropDown.MenuItems.Add(Me.RibbonSeparator1)
        Me.Ribbon1.OrbDropDown.Name = ""
        Me.Ribbon1.OrbDropDown.OptionItems.Add(Me.RibbonOrbOptionButton1)
        Me.Ribbon1.OrbDropDown.RecentItems.Add(Me.RibbonOrbRecentItem1)
        Me.Ribbon1.OrbDropDown.Size = New System.Drawing.Size(527, 207)
        Me.Ribbon1.OrbDropDown.TabIndex = 0
        Me.Ribbon1.OrbImage = Nothing
        Me.Ribbon1.OrbStyle = System.Windows.Forms.RibbonOrbStyle.Office_2013
        '
        '
        '
        Me.Ribbon1.QuickAcessToolbar.Items.Add(Me.RibbonButton1)
        Me.Ribbon1.RibbonTabFont = New System.Drawing.Font("Trebuchet MS", 9.0!)
        Me.Ribbon1.Size = New System.Drawing.Size(1146, 176)
        Me.Ribbon1.TabIndex = 4
        Me.Ribbon1.Tabs.Add(Me.RibbonTab1)
        Me.Ribbon1.Tabs.Add(Me.RibbonTab2)
        Me.Ribbon1.Tabs.Add(Me.RibbonTab3)
        Me.Ribbon1.TabsMargin = New System.Windows.Forms.Padding(12, 26, 20, 0)
        Me.Ribbon1.Text = "Ribbon1"
        Me.Ribbon1.ThemeColor = System.Windows.Forms.RibbonTheme.Black
        '
        'RibbonOrbMenuItem1
        '
        Me.RibbonOrbMenuItem1.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.RibbonOrbMenuItem1.Image = CType(resources.GetObject("RibbonOrbMenuItem1.Image"), System.Drawing.Image)
        Me.RibbonOrbMenuItem1.SmallImage = CType(resources.GetObject("RibbonOrbMenuItem1.SmallImage"), System.Drawing.Image)
        Me.RibbonOrbMenuItem1.Text = "Information"
        '
        'RibbonOrbMenuItem2
        '
        Me.RibbonOrbMenuItem2.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.RibbonOrbMenuItem2.Image = CType(resources.GetObject("RibbonOrbMenuItem2.Image"), System.Drawing.Image)
        Me.RibbonOrbMenuItem2.SmallImage = CType(resources.GetObject("RibbonOrbMenuItem2.SmallImage"), System.Drawing.Image)
        Me.RibbonOrbMenuItem2.Text = "Open"
        '
        'RibbonOrbMenuItem3
        '
        Me.RibbonOrbMenuItem3.DropDownArrowDirection = System.Windows.Forms.RibbonArrowDirection.Left
        Me.RibbonOrbMenuItem3.Image = CType(resources.GetObject("RibbonOrbMenuItem3.Image"), System.Drawing.Image)
        Me.RibbonOrbMenuItem3.SmallImage = CType(resources.GetObject("RibbonOrbMenuItem3.SmallImage"), System.Drawing.Image)
        Me.RibbonOrbMenuItem3.Text = "Exit"
        '
        'RibbonButton1
        '
        Me.RibbonButton1.Image = CType(resources.GetObject("RibbonButton1.Image"), System.Drawing.Image)
        Me.RibbonButton1.MaxSizeMode = System.Windows.Forms.RibbonElementSizeMode.Compact
        Me.RibbonButton1.SmallImage = CType(resources.GetObject("RibbonButton1.SmallImage"), System.Drawing.Image)
        Me.RibbonButton1.Text = "Save"
        '
        'RibbonTab1
        '
        Me.RibbonTab1.Text = "Start"
        '
        'RibbonTab2
        '
        Me.RibbonTab2.Text = "Edit"
        '
        'RibbonTab3
        '
        Me.RibbonTab3.Text = "About"
        '
        'MultipleTabpagePanel1
        '
        Me.MultipleTabpagePanel1.DisabledCloseControl = False
        Me.MultipleTabpagePanel1.Dock = System.Windows.Forms.DockStyle.Fill
        Me.MultipleTabpagePanel1.EnableMenu = True
        Me.MultipleTabpagePanel1.Location = New System.Drawing.Point(0, 176)
        Me.MultipleTabpagePanel1.Name = "MultipleTabpagePanel1"
        Me.MultipleTabpagePanel1.PageInterval = 0
        Me.MultipleTabpagePanel1.Renderer = Nothing
        Me.MultipleTabpagePanel1.Size = New System.Drawing.Size(1146, 533)
        Me.MultipleTabpagePanel1.SizeMode = Microsoft.VisualBasic.MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel.TabpageSizeModes.UIResourceAutoSize
        Me.MultipleTabpagePanel1.TabIndex = 5
        Me.MultipleTabpagePanel1.UIResource = Nothing
        '
        'RibbonOrbRecentItem1
        '
        Me.RibbonOrbRecentItem1.Image = CType(resources.GetObject("RibbonOrbRecentItem1.Image"), System.Drawing.Image)
        Me.RibbonOrbRecentItem1.SmallImage = CType(resources.GetObject("RibbonOrbRecentItem1.SmallImage"), System.Drawing.Image)
        Me.RibbonOrbRecentItem1.Text = "RibbonOrbRecentItem1"
        '
        'RibbonOrbOptionButton1
        '
        Me.RibbonOrbOptionButton1.Image = CType(resources.GetObject("RibbonOrbOptionButton1.Image"), System.Drawing.Image)
        Me.RibbonOrbOptionButton1.SmallImage = CType(resources.GetObject("RibbonOrbOptionButton1.SmallImage"), System.Drawing.Image)
        Me.RibbonOrbOptionButton1.Text = "RibbonOrbOptionButton1"
        '
        'FormIDEMain
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(7.0!, 17.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(1146, 731)
        Me.Controls.Add(Me.MultipleTabpagePanel1)
        Me.Controls.Add(Me.Ribbon1)
        Me.Controls.Add(Me.StatusStrip1)
        Me.Font = New System.Drawing.Font("Microsoft YaHei", 9.0!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(134, Byte))
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.Margin = New System.Windows.Forms.Padding(3, 4, 3, 4)
        Me.Name = "FormIDEMain"
        Me.Text = "LANS ShellScript Studio"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents StatusStrip1 As System.Windows.Forms.StatusStrip
    Friend WithEvents Ribbon1 As Ribbon
    Friend WithEvents RibbonOrbMenuItem1 As RibbonOrbMenuItem
    Friend WithEvents RibbonOrbMenuItem2 As RibbonOrbMenuItem
    Friend WithEvents RibbonOrbMenuItem3 As RibbonOrbMenuItem
    Friend WithEvents RibbonTab1 As RibbonTab
    Friend WithEvents RibbonTab2 As RibbonTab
    Friend WithEvents RibbonTab3 As RibbonTab
    Friend WithEvents RibbonButton1 As RibbonButton
    Friend WithEvents MultipleTabpagePanel1 As MolkPlusTheme.Windows.Forms.Controls.TabControl.TabPage.MultipleTabpagePanel
    Friend WithEvents RibbonSeparator1 As RibbonSeparator
    Friend WithEvents RibbonOrbOptionButton1 As RibbonOrbOptionButton
    Friend WithEvents RibbonOrbRecentItem1 As RibbonOrbRecentItem
End Class
