   M S B A M L   `   `   `  ����K  GMerlinBackOffice, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nulle7clr-namespace:MerlinBackOffice.Windows.InventoryWindows)MerlinBackOffice.Windows.InventoryWindows  5   	   H    BMerlinBackOffice.Windows.InventoryWindows.SellableAdjustmentWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Ilocal7clr-namespace:MerlinBackOffice.Windows.InventoryWindows     5   	     � 
ResizeMode$  NoResize=�5   	   ���#C9C9C9 � Title$" Sellable On Hand Adjustments��5
   	   $	��515��6.   $	��850��6;    � WindowStartupLocation$ CenterScreen=�6G   .��5      �  � RowDefinitions 5   
   �� 5      $	��130 �6   �� 5      $	��50* �6   �� 5      $��50 �6   .+�5   
   �� .+�5      �� $��0��6    �� Orientation$ 
Horizontal=�6&   $��5,5,5,2q�6?   .+�5      �� -   5         ��Name$ txtSearchSKU��5      $	��200��60   $��3q�6<   $��5,5,5,5q�6H   $��25��6Y   $��Inter)�5      $��	Enter SKU��6-   $��Center=�5      �� 5      -   5         $   LightMinimalistButtonStyle_Short#��[  5   4   $��75��6n   $��5q�6y   .��5      �� $ 
Horizontal=�6!   .+�5      �� $��Search��6$     LightMinimalistButtonText#��[ 62   5      5      �� 5!      -   5         ��Name$ cmbAdjustmentType��5!      $	��210��66   $��5,5,0,5q�6B   $��Inter)�6S   .@�5"      �� $��	To Shrink��6#   �� 5#      $��From Shrink��6#   5$      �� 5&      -   5        $ txtNewQuantity��5&      $	��200��62   $��5q�6>   $��New Quantity��6I   $��Inter)�5'      $��3q�6g   $��Center=�5(      �� 5*      -   5        #��[  5*   5   $��95��6o   $��5q�6z   .��5+      �� $ 
Horizontal=�6!   .+�5,      �� $��Add to List��6$   #��[ 67   5-      5.      5/      �� 51      $ 
Horizontal=�6   $��5q�62   .+�52      �� .+�53      �� $ 
Horizontal=�6!   .+�54      �� $��Product Name: ��6$   $��Inter)�6;   $��5q�6N   �� 55      -   5         ��Name$ txtProductName��55   $   $��Inter)�6<   $��5q�6O   56      �� 58      $ 
Horizontal=�6!   .+�59      �� $��
Category: ��6$   $��Inter)�67   $��5q�6J   �� 5:      -   5        $ txtCategory��5:   $   $��5q�69   $��Inter)�6D   $	��590��6W   5;      5<      �� 5>      .+�5?      �� $ 
Horizontal=�6!   $��Right=�6;   .+�5@      �� $��Sellable on Hand: ��6$   $��Inter)�6?   $��5q�6R   �� 5A      -   5        $ txtQuantitySellable��5A   $   $��Inter)�6A   $��5q�6T   $��30��6_   5B      �� 5D      $ 
Horizontal=�6!   $��Right=�6:   .+�5E      �� $��Defective on Hand: ��6$   $��Inter)�6@   $��5q�6S   �� 5F      -	   5        $ txtQuantityDefective��5F   $   $��Inter)�6B   $��5q�6U   $��30��6`   $��Right=�6k   5G      5H      5J      5K      �� 5N   
   $ 
Horizontal=�6   $��	5,2,5,361q�6.   $��1��6A   &   System.Windows.Controls.DataGrid  5Q   
   -
   5          Name$ dgPendingAdjustments��5Q      	   AutoGenerateColumns$	 False��61   
   CanUserAddRows$
 False��6M   $��10,100,10,0q�5R      $��2��6(   $��Inter)�69   
������    
IsReadOnly$
 True��6f      Columns 5S      0  *System.Windows.Controls.DataGridTextColumn  5T      ,  &System.Windows.Controls.DataGridColumn   Header$ Category��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ 75 6V   1  +System.Windows.Controls.DataGridBoundColumn   Binding 67   �� *
CategoryID+6V     5U      $	 SKU��6%   $	 100 6J    62   �� *SKU+6J     5V      $ Product Name��6%   $	 211 6[    6;   �� *ProductName+6[     5W      $ Adjustment Type��6%   $	 150 6a    6>   �� *AdjustmentType+6a     5X      $ Original Qty.��6%   $ 85 6a    6<   �� *OriginalQuantity+6a     5Y      $ New Qty.��6%   $ 85 6W    67   �� *NewQuantity+6W     5Z      $ 
Difference��6%   $ 85 6`    69   �� *QuantityDifference+6`   5\      �� 5_   
   $��2��6   $ 
Horizontal=�6"   $��Right=�6;   $��Center=�6W   $��10q�6s   .+�5`      �� #��[  6   $��0,0,5,0q�6O   $��70��6`   .��5a      �� $ 
Horizontal=�6   .+�5b      �� $��Cancel��6    #��[ 6.   5c      5d      �� 5e      -   5         ��Name$ btnFinalizeAdjustments��5e      #��[  65   $	��150��6�   .��5f      �� $ 
Horizontal=�6   .+�5g      �� $��Finalize Adjustments��6    #��[ 6<   5h      5i      5j      5k      5l      