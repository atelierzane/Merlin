   M S B A M L   `   `   `  ����K  GMerlinBackOffice, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullO,clr-namespace:MerlinBackOffice.DialogWindowsMerlinBackOffice.DialogWindows  5   	   =    7MerlinBackOffice.DialogWindows.ItemMovementSearchWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   >local,clr-namespace:MerlinBackOffice.DialogWindows     5   	     � Title$  Item Movement��5   	   $	��300��6   $	��400��6,   .��5	      �  � ColumnDefinitions 5
   
   �� 5       � RowDefinitions 5   
   �� 5      �� 5      $��30 �6   .+�5   
   �� $��0��6   .+�5      ��  �� Orientation$ 
Horizontal=�6   .+�5      �� $��	SKU/UPC: ��6   $��
10,10,0,10q�5      �� 5      $	��250��6   $��10q�5      5      �� 5      $ 
Horizontal=�6   .+�5      �� $��
Category: ��6   $��
10,10,0,10q�5      �� 5      $	��250��6   $��10q�5      5      �� 5       $ 
Horizontal=�6   .+�5!      �� $��Start Date: ��6   $��
10,10,0,10q�5"      (  "System.Windows.Controls.DatePicker  5#      $	��250��6   $��10q�5$      5%      �� 5'      $ 
Horizontal=�6   .+�5(      �� $��
End Date: ��6   $��	10,10,1,0q�5)        5*      $	��250��6   $��10q�5+      5,      5-      �� 5/   
   $��1��6   $ 
Horizontal=�50      $��Right=�51      .+�52      �� $��[ESC] Close��6   $��5q�53      �� 54      $��[ENTER] Search��6   $��5q�55      57      59      5:      