   M S B A M L   `   `   `  ����  MerlinAdministratorI    CMerlinAdministrator.Pages.PromotionManagerPages.RemoveComboBulkPage   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5        N� Title$  RemoveComboBulkPage��5      .��5
      �  � ColumnDefinitions 5   
   �� 5      $	��150 �6   �� 5      .+�5   
   �� 
�������$��2��6$   �� 5   
   $��0��6   $��5q�6%   .+�5      ��  �� Orientation$ 
Horizontal=�6   $��5q�62   .+�5      �� $��SKU: ��6   �� 5      -   5         ��Name$ 
SkuTextBox��5      $��75��6.   5      �� 5      $ 
Horizontal=�6   $��5q�62   .+�5      �� $��Combo Name: ��6   �� 5      -   5        $ ComboNameTextBox��5      $	��100��64   5      �� 5      $ 
Horizontal=�6   $��5q�62   .+�5      �� $��
Price. >=:��6   �� 5      -   5        $ MinPriceTextBox��5      $��75��63   5      �� 5       $ 
Horizontal=�6   $��5q�62   .+�5!      �� $��	Price. <:��6   �� 5"      -   5        $ MaxPriceTextBox��5"      $��75��63   5#      �� 5&      -   5        $��Search��5&      $	��125��6&   $��25��62   6>   �� 5'      -   5        $��Reset��5'      $	��125��6%   $��25��61   6=   �� 5(      -   5        $��Delete Selected��5(      $��1��6/   $
��Left=�6?   $��5q�6Z   $	��150��6e   6q   5)      &   System.Windows.Controls.DataGrid  5,   
   -   5          Name$ ComboDataGrid��5,      $��1��6*   $��5q�6:      AutoGenerateColumns$ False��6E      SelectionMode$ Extended=�6a      Columns 5-      4  .System.Windows.Controls.DataGridTemplateColumn  5/      ,  &System.Windows.Controls.DataGridColumn   Header$ Select��6)   	   HeaderTemplate	 50      �� 51      .R�52      �� 
 ��Name$
 SelectAllCheckBox��6'   -	   5        $	��All��52   B   6P   53         CellTemplate 55      �� 56      .R�57      ��  d� 	IsChecked 6'   �� *
IsSelected+ �� Mode$ TwoWay=�58      5:      0  *System.Windows.Controls.DataGridTextColumn  5<      $	 SKU��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ * 6O   1  +System.Windows.Controls.DataGridBoundColumn   Binding 62   �� *
ComboSKU+6O     5=      $ 
Combo Name��6%   $ * 6W    69   �� *	ComboName+6W     5>      $ Price��6%   $ * 6S    64   �� *
ComboPrice+6S   5@      5B      5C      