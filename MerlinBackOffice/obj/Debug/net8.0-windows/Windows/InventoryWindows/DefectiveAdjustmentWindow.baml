   M S B A M L   `   `   `  ����K  GMerlinBackOffice, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nulle7clr-namespace:MerlinBackOffice.Windows.InventoryWindows)MerlinBackOffice.Windows.InventoryWindows  5   	   I    CMerlinBackOffice.Windows.InventoryWindows.DefectiveAdjustmentWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   Ilocal7clr-namespace:MerlinBackOffice.Windows.InventoryWindows     5   	     � 
ResizeMode$  NoResize=�5   	   ���#C9C9C9 � Title$# Defective On Hand Adjustments��5
   	   $	��515��6/   $	��850��6<    � WindowStartupLocation$ CenterScreen=�6H   .��5      �  � RowDefinitions 5   
   �� 5      $	��130 �6   �� 5      $	��50* �6   �� 5      $��50 �6   .+�5   
   �� .+�5      �� $��0��6    �� Orientation$ 
Horizontal=�6&   $��5,5,5,2q�6?   .+�5      �� -   5         ��Name$ txtSearchSKU��5      $	��200��60   $��3q�6<   $��5,5,5,5q�6H   $��25��6Y   $��Inter)�5      $��	Enter SKU��6-   $��Center=�5      �� 5      -   5         $   LightMinimalistButtonStyle_Short#��[  5   4   $��75��6n   $��5q�6y   .��5      �� $ 
Horizontal=�6!   .+�5      �� $��Search��6$     LightMinimalistButtonText#��[ 62   5      5      �� 5      -   5         ��Name$ cmbAdjustmentType��5      $	��210��66   $��5,5,0,5q�6B   $��Inter)�6S   .@�5      �� $��To Defective��6#   �� 5      $��From Defective��6#   5       �� 5!      -   5        $ txtNewQuantity��5!      $	��200��62   $��5q�6>   $��New Quantity��6I   $��Inter)�5"      $��3q�6g   $��Center=�5#      �� 5$      -   5        #��[  5$   5   $��95��6o   $��5q�6z   .��5%      �� $ 
Horizontal=�6!   .+�5&      �� $��Add to List��6$   #��[ 67   5'      5(      5)      �� 5+      $ 
Horizontal=�6   $��5q�62   .+�5,      �� .+�5-      �� $ 
Horizontal=�6!   .+�5.      �� $��Product Name: ��6$   $��Inter)�6;   $��5q�6N   �� 5/      -   5         ��Name$ txtProductName��5/   $   $��Inter)�6<   $��5q�6O   50      �� 51      $ 
Horizontal=�6!   .+�52      �� $��
Category: ��6$   $��Inter)�67   $��5q�6J   �� 53      -   5        $ txtCategory��53   $   $��5q�69   $��Inter)�6D   $	��590��6W   54      55      �� 56      .+�57      �� $ 
Horizontal=�6!   $��Right=�6:   .+�58      �� $��Sellable on Hand: ��6$   $��Inter)�6?   $��5q�6R   �� 59      -   5        $ txtQuantitySellable��59   $   $��Inter)�6A   $��5q�6T   $��30��6_   5:      �� 5;      $ 
Horizontal=�6!   $��Right=�6:   .+�5<      �� $��Defective on Hand: ��6$   $��Inter)�6@   $��5q�6S   �� 5=      -	   5        $ txtQuantityDefective��5=   $   $��Inter)�6B   $��5q�6U   $��30��6`   $��Right=�6k   5>      5?      5@      5A      �� 5D   
   $ 
Horizontal=�6   $��	5,2,5,361q�6.   $��1��6A   &   System.Windows.Controls.DataGrid  5G   
   -
   5          Name$ dgPendingAdjustments��5G      	   AutoGenerateColumns$	 False��61   
   CanUserAddRows$
 False��6M   $��10,100,10,0q�5H      $��2��6(   $��Inter)�69   
������    
IsReadOnly$
 True��6f      Columns 5I      0  *System.Windows.Controls.DataGridTextColumn  5J      ,  &System.Windows.Controls.DataGridColumn   Header$ Category��6%      Width5  /System.Windows.Controls.DataGridLengthConverter$ 75 6V   1  +System.Windows.Controls.DataGridBoundColumn   Binding 67   �� *
CategoryID+6V     5K      $	 SKU��6%   $	 100 6J    62   �� *SKU+6J     5L      $ Product Name��6%   $	 211 6[    6;   �� *ProductName+6[     5M      $ Adjustment Type��6%   $	 150 6a    6>   �� *AdjustmentType+6a     5N      $ Original Qty.��6%   $ 85 6a    6<   �� *OriginalQuantity+6a     5O      $ New Qty.��6%   $ 85 6W    67   �� *NewQuantity+6W     5P      $ 
Difference��6%   $ 85 6`    69   �� *QuantityDifference+6`   5R      �� 5U   
   $��2��6   $ 
Horizontal=�6"   $��Right=�6;   $��Center=�6W   $��10q�6r   .+�5V      �� #��[  6   $��0,0,5,0q�6O   $��70��6`   .��5W      �� $ 
Horizontal=�6   .+�5X      �� $��Cancel��6    #��[ 6.   5Y      5Z      �� 5[      -   5         ��Name$ btnFinalizeAdjustments��5[      #��[  65   $	��150��6�   .��5\      �� $ 
Horizontal=�6   .+�5]      �� $��Finalize Adjustments��6    #��[ 6<   5^      5_      5`      5a      5b      