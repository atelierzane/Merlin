   M S B A M L   `   `   `  ����K  GMerlinBackOffice, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nulle7clr-namespace:MerlinBackOffice.Windows.InventoryWindows)MerlinBackOffice.Windows.InventoryWindows  5   	   C    =MerlinBackOffice.Windows.InventoryWindows.CategoryCountWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Ilocal7clr-namespace:MerlinBackOffice.Windows.InventoryWindows     5   	     � 
ResizeMode$  NoResize=�5   	    � WindowStartupLocation$ CenterScreen=�5   	   ���#C9C9C9 � Title$ CategoryCountWindow��5   	   $	��650��6%   $	��875��62   .��5      �  � RowDefinitions 5      �� 5      $��40 �6   �� 5      $��40 �6   �� 5      $��* �6   �� 5      $��55 �6    � ColumnDefinitions 5      �� 5      $	��200 �6   �� 5      .+�5      �� $��2��6   $��0��6   .+�5      ��  �� Orientation$ 
Horizontal=�6   $
��Left=�6/   $��Center=�6J   $��10q�6e   .+�5      �� $��Select Category: ��6   $��Center=�5      $��10,5,5,5q�5      �� 5      -   5         ��Name$ cmbCategory��5      $��25��5       $	��200��5!      $��10,10,10,10q�5"      $
��Left=�5#      $��Inter)�5$      
���   �$��Regular"�5&      ��5(      �� 5)      .R�5*      �� $ 
Horizontal=�6)   .+�5+   "   �� $
��Bold"�6H   ��6,   �� *
CategoryID+6H   �� 5,   "   $	�� - ��6,   �� 5-   "   ��6,   �� *CategoryName+5.      5/      51      53      �� 55      -   5         ��Name$ StartNewCountButton��55       (  $LightMinimalistButtonStyle_Inventory#��[  56      $	��185��6P   $��5,10,0,5q�6\   .��57      �� $ 
Horizontal=�6   .+�58      �� $��Start Count��6     LightMinimalistButtonText#��[ 60   59      5:      �� 5;      -   5        #��[  5<      $	��185��6P   $��5,10,0,5q�6\   .��5=      �� $ 
Horizontal=�6   .+�5>      �� $��Count History��6   #��[ 62   5?      5@      �� 5A      -   5        #��[  5B      $	��185��6P   $��5,10,0,5q�6\   .��5C      �� $ 
Horizontal=�6   .+�5D      �� $��Print Report��6   #��[ 61   5E      5F      5I      �� 5J      -   5        $��3��5J   *   #��[  5K      $	��185��6P   .��5L      �� $ 
Horizontal=�6   .+�5M      �� $��
Back [ESC]��6   #��[ 6+   5N      5O      �� 5R   
   -   5         ��Name$ txtCurrentCategory��5R      $��0��5S      $��1��5T      $
��Left=�5U      $��Center=�5V      $��16(�5W      $
��Bold"�5X      
���   �$��10q�5Z      �� 5]   
   -   5        $��lvInventoryItems��5]      $��10,10,10,10q�5_      $��Inter)�5`      $��Regular"�5a      $��2��5b      $��1��5c      ${�False��5d      ��5f      �� 5g      	 �� 
TargetType"	 ��6   
 �� Triggers
 5h      �� 5i       �� Binding 6&   ��  �� Path$ IsDiscrepancy� �� Value True5j      .a�5k      ��  �� Property � �� �� Value
 � ���5l      5m      5o       �� View 5q      �� 5r      .P�5s      �� $	��SKU��6%    �� Width$	 120��5u       �� DisplayMemberBinding 5t      �� *SKU+5u      �� 5v      $��Product Name��6%   $	 150��5x       5w      �� *ProductName+5x      �� 5z      $��Initial SH Qty��6%   $	 100��5|       5{      �� *ExpectedQuantity+5|      �� 5}      $��Counted Qty��6%   $	 100��5       5~      �� *ScannedQuantity+5      5�      5�      �� 5�      -   5         ��Name$ 
InputPanel��5�      $ 
Horizontal=�6&   $��1��6?   $��1��6L   $
��Left=�6\   $��Center=�6w   $��	250,0,0,0q�6�   $��40��6�   $	��449��6�   ${�False��6�   .+�5�      �� $
��SKU:��6   $��14(�5�      $��Inter)�5�      $��Medium"�5�      ���#181818$��5,10,5,5q�5�      $��Center=�5�      �� 5�      -	   5         ��Name$ txtSKU��5�      $��5,10,5,5q�5�      $��25��5�      $��Center=�5�      $��Inter)�5�      $��3q�5�      $	��150��5�      �� 5�      $
��QTY:��6   $��14(�5�      $��Inter)�5�      $��Medium"�5�      ���#181818$��5,10,5,5q�5�      $��Center=�5�      �� 5�      -
   5        $ txtQuantity��5�      $��0,10,5,5q�5�      $��25��5�      $��50��5�      $��Center=�5�      $��Inter)�5�      $��3q�5�      �� 5�      -   5        $ AddCountButton��5�       $  LightMinimalistButtonStyle_Short#��[ 5�      $	��100��6G   $��0,5,10,5q�6S   $
��Left=�5�      $��Bottom=�6(   .��5�      �� $ 
Horizontal=�6   .+�5�      �� $��Add to Count��6   #��[ 61   5�      5�      5�      �� 5�      -   5        $ FinalizeCountButton��5�      #��[  5�      $��3��6K   $��1��6X   $	��150��6h   $��10q�6t   $��Right=�5�      $��Center=�6)   $��35��6D   ${�False��5�      .��5�      �� $ 
Horizontal=�6   .+�5�      �� $��Finalize Count��6   #��[ 6/   5�      5�      5�      5�      