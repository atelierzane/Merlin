   M S B A M L   `   `   `  ����N  JMerlinAdministrator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null9!clr-namespace:MerlinAdministratorMerlinAdministrator  5   	   *    $MerlinAdministrator.RibbonTestWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   3local!clr-namespace:MerlinAdministrator     5   	     � Title$  RibbonTestWindow��5   	   $	��450��6"   $	��800��6/   .��5	      �  � RowDefinitions 5
   
   �� 5      $��* �6   �� 5      .+�5   
   +  %System.Windows.Controls.Ribbon.Ribbon  -   5          Name$ 	RibbonWin��5         HelpPaneContent 5      1  +System.Windows.Controls.Ribbon.RibbonButton  5         QuickAccessToolBar 5      =  7System.Windows.Controls.Ribbon.RibbonQuickAccessToolBar  5      .@�5        -   5          Name$
 Save��5   #   6  0System.Windows.Controls.Ribbon.RibbonSplitButton  5      -   5          Name$
 Undo��5   (   .@�5      8  2System.Windows.Controls.Ribbon.RibbonSplitMenuItem  $��Undo 1��6.     5      $��Undo 2��6.     5      $��Undo 3��6.   5        5      -   5        $
 Redo��5   (   .@�5        $��Redo 1��6.     5       $��Redo 2��6.   5!      5"         ApplicationMenu 5%      :  4System.Windows.Controls.Ribbon.RibbonApplicationMenu  5&      5  /System.Windows.Controls.Ribbon.RibbonMenuButton   KeyTip$ F��6(   .@�5'      >  8System.Windows.Controls.Ribbon.RibbonApplicationMenuItem  $��Options��60     5(      $
��Exit��60   5)      .@�5,      .	  (System.Windows.Controls.Ribbon.RibbonTab	  $��Catalog��6   	 	  KeyTip$	 H��6)   .@�5/      0
  *System.Windows.Controls.Ribbon.RibbonGroup
  -   5        
 
 Name$
 SearchGroup��5/      $��Search��63   51      
  54      -   5        $
 ClipboardGroup��54      $
��Home��66   .@�55           Label$ Paste��6'   $ V��65   .@�56      3  -System.Windows.Controls.Ribbon.RibbonMenuItem  $��Keep Text Only��6)      KeyTip$ T��6A     57      $��Paste Special...��6)   $ S��6C   58        59         Label$	 Cut��6#      KeyTip$ X��6/     5:      $
 Copy��6$   $ C��61     5;      $ Format Painter��6#   $ FP��6:   5<      
  5>      -   5        $
 Employee��5>      $��Employee And Payroll��60   .@�5?        $ Employee��6'   $ V��68   .@�5@        $��Keep Text Only��6*   $ T��6B     5A      $��Paste Special...��6*   $ S��6D   5B        5C      $
 Save��6$   $ X��61     5D      $	 Add��6$   $ C��60   5E      5F      	  5H      $��Insert��6   $	 I��6(   5I      	  5K      $��
PageLayout��6   $	 L��6,   5L      5M      5P      5Q      