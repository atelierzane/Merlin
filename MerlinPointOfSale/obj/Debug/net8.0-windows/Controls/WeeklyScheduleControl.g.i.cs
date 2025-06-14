﻿#pragma checksum "..\..\..\..\Controls\WeeklyScheduleControl.xaml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "87F49B4688072B4E3E8E430E2E1CDA903CAA9692"
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using MerlinPointOfSale.Controls;
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


namespace MerlinPointOfSale.Controls {
    
    
    /// <summary>
    /// WeeklyScheduleControl
    /// </summary>
    public partial class WeeklyScheduleControl : System.Windows.Controls.UserControl, System.Windows.Markup.IComponentConnector {
        
        
        #line 22 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.ComboBox WeekComboBox;
        
        #line default
        #line hidden
        
        
        #line 27 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Button AddShiftButton;
        
        #line default
        #line hidden
        
        
        #line 34 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock AllocatedHoursTextBlock;
        
        #line default
        #line hidden
        
        
        #line 41 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.TextBlock ScheduledHoursTextBlock;
        
        #line default
        #line hidden
        
        
        #line 52 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal MerlinPointOfSale.Controls.SubMenuBarControl subMenuBar;
        
        #line default
        #line hidden
        
        
        #line 53 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Canvas GridLinesCanvas;
        
        #line default
        #line hidden
        
        
        #line 55 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1823:AvoidUnusedPrivateFields")]
        internal System.Windows.Controls.Grid DayContentGrid;
        
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
            System.Uri resourceLocater = new System.Uri("/MerlinPointOfSale;component/controls/weeklyschedulecontrol.xaml", System.UriKind.Relative);
            
            #line 1 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
            System.Windows.Application.LoadComponent(this, resourceLocater);
            
            #line default
            #line hidden
        }
        
        [System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [System.CodeDom.Compiler.GeneratedCodeAttribute("PresentationBuildTasks", "9.0.1.0")]
        [System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal System.Delegate _CreateDelegate(System.Type delegateType, string handler) {
            return System.Delegate.CreateDelegate(delegateType, this, handler);
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
            this.WeekComboBox = ((System.Windows.Controls.ComboBox)(target));
            
            #line 25 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
            this.WeekComboBox.SelectionChanged += new System.Windows.Controls.SelectionChangedEventHandler(this.WeekComboBox_SelectionChanged);
            
            #line default
            #line hidden
            return;
            case 2:
            this.AddShiftButton = ((System.Windows.Controls.Button)(target));
            
            #line 31 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
            this.AddShiftButton.Click += new System.Windows.RoutedEventHandler(this.AddShiftButton_Click);
            
            #line default
            #line hidden
            return;
            case 3:
            
            #line 32 "..\..\..\..\Controls\WeeklyScheduleControl.xaml"
            ((System.Windows.Controls.Button)(target)).Click += new System.Windows.RoutedEventHandler(this.PostScheduleButton_Click);
            
            #line default
            #line hidden
            return;
            case 4:
            this.AllocatedHoursTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 5:
            this.ScheduledHoursTextBlock = ((System.Windows.Controls.TextBlock)(target));
            return;
            case 6:
            this.subMenuBar = ((MerlinPointOfSale.Controls.SubMenuBarControl)(target));
            return;
            case 7:
            this.GridLinesCanvas = ((System.Windows.Controls.Canvas)(target));
            return;
            case 8:
            this.DayContentGrid = ((System.Windows.Controls.Grid)(target));
            return;
            }
            this._contentLoaded = true;
        }
    }
}

