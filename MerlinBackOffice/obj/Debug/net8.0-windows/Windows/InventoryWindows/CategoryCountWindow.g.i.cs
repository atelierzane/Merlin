﻿#pragma checksum "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "92C508BA4538EDDEB44F1E2F20807E98345F9D15"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MerlinBackOffice.Windows.InventoryWindows;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Controls.Ribbon;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.Integration;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Shell;


namespace MerlinBackOffice.Windows.InventoryWindows {
    
    
    /// <summary>
    /// CategoryCountWindow
    /// </summary>
    public partial class CategoryCountWindow : System.Windows.Window, System.Windows.Markup.IComponentConnector {
        
        
        #line 31 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox cmbCategory;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button StartNewCountButton;
        
        #line default
        #line hidden
        
        
        #line 82 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock txtCurrentCategory;
        
        #line default
        #line hidden
        
        
        #line 93 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ListView lvInventoryItems;
        
        #line default
        #line hidden
        
        
        #line 134 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel InputPanel;
        
        #line default
        #line hidden
        
        
        #line 142 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtSKU;
        
        #line default
        #line hidden
        
        
        #line 156 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox txtQuantity;
        
        #line default
        #line hidden
        
        
        #line 163 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AddCountButton;
        
        #line default
        #line hidden
        
        
        #line 173 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button FinalizeCountButton;
        
        #line default
        #line hidden
        
        private bool _contentLoaded;
        
        /// <summary>
        /// InitializeComponent
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.1.0")]
        public void InitializeComponent() {
            if (_contentLoaded) {
                return;
            }
            _contentLoaded = true;
            System.Uri resourceLocater = new System.Uri("/MerlinBackOffice;component/windows/inventorywindows/categorycountwindow.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.1.0")]
        [System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Design", "CA1033:InterfaceMethodsShouldBeCallableByChildTypes")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        void System.Windows.Markup.IComponentConnector.Connect(int connectionId, object target) {
            switch (connectionId)
            {
            case 1:
            this.cmbCategory = ((System.Windows.Controls.ComboBox)(target));
            
            #line 39 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
            this.cmbCategory.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.CmbCategory_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.StartNewCountButton = ((System.Windows.Controls.Button)(target));
            
            #line 53 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
            this.StartNewCountButton.Click += new System.Windows.RoutedEventHandler(this.StartNewCount_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 59 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.CountHistory_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            
            #line 65 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.StartNewCount_Click);
            
            #line default
            #line hidden
            return;
            case 5:
            
            #line 74 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.StartNewCount_Click);
            
            #line default
            #line hidden
            return;
            case 6:
            this.txtCurrentCategory = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 7:
            this.lvInventoryItems = ((System.Windows.Controls.ListView)(target));
            
            #line 94 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
            this.lvInventoryItems.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.LvInventoryItems_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 8:
            this.InputPanel = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 9:
            this.txtSKU = ((System.Windows.Controls.TextBox)(target));
            return;
            case 10:
            this.txtQuantity = ((System.Windows.Controls.TextBox)(target));
            return;
            case 11:
            this.AddCountButton = ((System.Windows.Controls.Button)(target));
            
            #line 163 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
            this.AddCountButton.Click += new System.Windows.RoutedEventHandler(this.AddCountButton_Click);
            
            #line default
            #line hidden
            return;
            case 12:
            this.FinalizeCountButton = ((System.Windows.Controls.Button)(target));
            
            #line 173 "..\..\..\..\..\Windows\InventoryWindows\CategoryCountWindow.xaml"
            this.FinalizeCountButton.Click += new System.Windows.RoutedEventHandler(this.FinalizeCountButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

