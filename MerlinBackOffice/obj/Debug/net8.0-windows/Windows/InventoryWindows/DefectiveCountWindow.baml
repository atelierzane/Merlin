   M S B A M L   `   `   `  ����K  GMerlinBackOffice, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nulle7clr-namespace:MerlinBackOffice.Windows.InventoryWindows)MerlinBackOffice.Windows.InventoryWindows  5   	   D    >MerlinBackOffice.Windows.InventoryWindows.DefectiveCountWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Ilocal7clr-namespace:MerlinBackOffice.Windows.InventoryWindows     5   	   ���#C9C9C9  � 
ResizeMode$  NoResize=�5	   	    � WindowStartupLocation$ CenterScreen=�5
   	    � Title$ DefectiveCountWindow��5   	   $	��650��6&   $	��875��63   .��5      �  � RowDefinitions 5   
   �� 5      $��40 �6   �� 5      �� 5      $��55 �6    � ColumnDefinitions 5   
   �� 5      $	��200 �6   �� 5      .+�5   
   �� -   5        $��lvDefectiveItems��5      $��10,10,10,10q�5      $��Inter)�5      $��Regular"�5      $��1��5      $��1��5      ${�False��5       ��5"      �� 5#       �� 
TargetType" ��6    �� Triggers 5$      �� 5%       �� Binding 6&   ��  �� Path$ IsDiscrepancy�	 �� Value	 True5&   #   .a�5'      �� 
 �� Property
 � �� �� Value
 � ���5(   "   5)      5+       �� View 5-      �� 5.      .P�5/      �� $	��SKU��6%    �� Width$	 120��51   "    �� DisplayMemberBinding 50   "   �� *SKU+51   "   �� 52      $��Category��6%   $	 120��54   "    53   "   �� *
CategoryID+54   "   �� 55      $��Title��6%   $	 150��57   "    56   "   �� *ProductName+57   "   �� 58      $��Initial DH Qty��6%   $	 100��5:   "    59   "   �� *ExpectedQuantity+5:   "   �� 5;      $��Counted Qty��6%   $	 100��5=   "    5<   "   �� *ScannedQuantity+5=   "   5>      5@      �� 5B   
   -   5         ��Name$ 
InputPanel��5B       �� Orientation$ 
Horizontal=�6)   $��1��6B   $
��Left=�6R   $��Center=�6m   $��	250,0,0,0q�6�   $��40��6�   $	��449��6�   ${�False��6�   .+�5D      �� $
��SKU:��6   $��14(�5E      $��Inter)�5F      $��Medium"�5G      ���#181818$��5,10,5,5q�5I      $��Center=�5J      �� 5K      -   5         ��Name$ txtSKU��5K      $��5,10,5,5q�5L      $��25��5M      $��Center=�5N      $��Inter)�5O      $��3q�5P      $	��150��5Q      �� 5S      $
��QTY:��6   $��14(�5T      $��Inter)�5U      $��Medium"�5V      ���#181818$��5,10,5,5q�5X      $��Center=�5Y      �� 5\      -   5        $ txtQuantity��5\      $��0,10,5,5q�5]      $��25��5^      $��50��5_      $��Center=�5`      $��Inter)�5a      $��3q�5b      �� 5e      -   5         ��Name$ AddCountButton��5e       $   LightMinimalistButtonStyle_Short#��[  5f      $	��100��6G   $��0,5,10,5q�6S   $
��Left=�5g      $��Bottom=�6(   .��5h      �� $ 
Horizontal=�6   .+�5i      �� $��Add to Count��6      LightMinimalistButtonText#��[ 64   5j      5k      5l      �� 5n   
   $��1��6   .+�5o      �� -   5        $ StartNewCountButton��5o       ( $LightMinimalistButtonStyle_Inventory#��[ 5p      $	��185��6P   $��5,10,0,5q�6\   .��5q      �� $ 
Horizontal=�6   .+�5r      �� $��Start Count��6    #��[ 63   5s      5t      �� 5v      -   5        #��[ 5w      $	��185��6O   $��5,10,0,5q�6[   .��5x      �� $ 
Horizontal=�6   .+�5y      �� $��Count History��6    #��[ 65   5z      5{      �� 5}      #��[ 6   $	��185��6S   $��5,10,0,5q�6_   .��5~      �� $ 
Horizontal=�6   .+�5      �� $��Print Report��6    #��[ 64   5�      5�      5�      �� 5�   
   $��1��6   $��Bottom=�6"   .+�5�      �� #��[ 6   $	��185��6S   $��	5,10,0,10q�6_   .��5�      �� $ 
Horizontal=�6   .+�5�      �� $��Cancel [ESC]��6    #��[ 64   5�      5�      5�      �� 5�   
   -   5        $ FinalizeCountButton��5�      #��[ 5�      $��2��6K   $��1��6X   $	��150��6h   $��10q�6t   $��Right=�5�      $��Center=�6)   $��35��6D   ${�False��5�      .��5�      �� $ 
Horizontal=�6   .+�5�      �� $��Finalize Count��6   #��[ 62   5�      5�      5�      5�      