﻿#pragma checksum "..\..\..\..\..\Pages\PromotionManagerPages\EditComboPage.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "A3C55630485F690EEF667A6FE09F7D41882974C2"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

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


namespace MerlinAdministrator.Pages.PromotionManagerPages {
    
    
    /// <summary>
    /// EditComboPage
    /// </summary>
    public partial class EditComboPage : System.Windows.Controls.Page, System.Windows.Markup.IComponentConnector {
        
        
        #line 54 "..\..\..\..\..\Pages\PromotionManagerPages\EditComboPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox SkuTextBox;
        
        #line default
        #line hidden
        
        
        #line 59 "..\..\..\..\..\Pages\PromotionManagerPages\EditComboPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.StackPanel ComboEditSection;
        
        #line default
        #line hidden
        
        
        #line 63 "..\..\..\..\..\Pages\PromotionManagerPages\EditComboPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox ComboNameTextBox;
        
        #line default
        #line hidden
        
        
        #line 68 "..\..\..\..\..\Pages\PromotionManagerPages\EditComboPage.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBox PriceTextBox;
        
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
            System.Uri resourceLocater = new System.Uri("/MerlinAdministrator;component/pages/promotionmanagerpages/editcombopage.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\..\Pages\PromotionManagerPages\EditComboPage.xaml"
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
            this.SkuTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 2:
            
            #line 55 "..\..\..\..\..\Pages\PromotionManagerPages\EditComboPage.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.SearchButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            this.ComboEditSection = ((System.Windows.Controls.StackPanel)(target));
            return;
            case 4:
            this.ComboNameTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 5:
            this.PriceTextBox = ((System.Windows.Controls.TextBox)(target));
            return;
            case 6:
            
            #line 73 "..\..\..\..\..\Pages\PromotionManagerPages\EditComboPage.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.UpdateButton_Click);
            
            #line default
            #line hidden
            return;
            }
            this._contentLoaded = true;
        }
    }
}

