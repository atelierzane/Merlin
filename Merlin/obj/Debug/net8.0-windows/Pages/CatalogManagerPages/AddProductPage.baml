   M S B A M L   `   `   `  ����  MerlinAdministratorB    <MerlinAdministrator.Pages.CatalogManagerPages.AddProductPage   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5        N� Title$  AddProductPage��5      $	��730��5      .��5
      ��  �� CornerRadius$ 7,7,5,5��6   $��5,10,5,5q�6$   ��5   
   �� 5       �� 
StartPoint$	 0,0&�6"    �� EndPoint$	 0,1&�63   .��5      �  � Color$ #C9C9C9��6    � Offset$ 0Q�6/   � 5      $ #B1B1B1��6   $ 1Q�6/   5      ._�5   
   �  � RowDefinitions 5      �� 5      $��75 �6    �� 5      $��* �6    �� 5      $��60 �6    .+�5      �� $��0��6   $ 5,5,0,0��6"   ��5      �� 5      $	 0,0&�6*   $	 0,1&�6;   .��5      �    HeaderGradientStop1# [  6'   $ 0Q�6T   � 5        HeaderGradientStop2# [ 6'   $ 1Q�6T   5       J� Effect 5!      3  -System.Windows.Media.Effects.DropShadowEffect  5"         
BlurRadius$ 4Q�6'   	   Opacity$	 .5Q�66   
   	Direction$	
 275Q�6C      ShadowDepth$ 2Q�6S   ._�5$      �� .+�5%      �� $��Add Product��6      PageHeaderTitle#��[ 63   $��
10,10,10,5q�6\   �� 5&      $b��\Enter product details below. Variants and additional attributes can be configured as needed.��6      PageHeaderText#��[ 5'       $��10,0,0,0q�6H   5(      5)      �� 5,      $��1��6   $
��Auto=�6(   .��5-      �� $��10q�6   .+�5/      ��  �� Orientation$ Vertical=�6!   $��5,0,0,0q�68   .+�50      �� $��Product Name:��6$     ControlHeaderText#��[ 69   $��5,10,0,5q�6d   �� 51      -   5         ��Name$ ProductNameTextBox��51   "    ! LightMinimalistSearchBoxStyle#��[ 6>   $
��1155��6u   $
��Left=�6�   $��5,0,0,0q�6�   52      �� 54      $ Vertical=�6!   $��5,0,0,0q�68   .+�55      �� $"��Product Category/Department:��6$   #��[ 6H   $��5,10,0,5q�6s   �� 56      -   5        $��CategoryIDComboBox��56   #      LightMinimalistComboBoxStyle#��[ 57   #   $
��1155��58   #   $
��Left=�59   #   $��5,0,0,0q�5:   #    �� SelectedValuePath$ 
CategoryID��5;   #   ��5=      �� 5>   "   .R�5?   &   �� $ 
Horizontal=�61   .+�5@   *   �� $��50��6P   ��64   �� *
CategoryID+6P   �� 5A   *   $	�� - ��64   �� 5B   *   ��64   �� *CategoryName+5C   '   5D   #   5F      5G      �� 5I      $ Vertical=�6!   $��5,0,0,0q�68   .+�5J      �� $��Price:��6$   #��[ 62   $��5,10,0,5q�6]   �� 5K      -   5        $ ProductPriceTextBox��5K   "   #��[ 6?   $
��1155��6v   $
��Left=�6�   $��5,0,0,0q�6�   5L      �� 5N      $ Vertical=�6!   $��5,0,0,0q�68   .+�5O      �� $��Cost:��6$   #��[ 61   $��5,10,0,5q�6\   �� 5P      -   5        $ CostTextBox��5P   "   #��[ 67   $
��1155��6n   $
��Left=�6{   $��5,0,0,0q�6�   5Q      �� 5S      $ Vertical=�6!   $��5,0,0,0q�68   .+�5T      �� $��Vendor:��6$   #��[ 63   $��5,10,0,5q�6^   �� 5U      -   5        $��VendorComboBox��5U   #   #��[ 5V   #   $
��1155��5W   #   $
��Left=�5X   #   $��5,0,0,0q�5Y   #   $ VendorID��5Z   #   ��5[      �� 5\   "   .R�5]   &   �� $ 
Horizontal=�61   .+�5^   *   �� $��75��6N   ��64   �� *
VendorID+6N   �� 5_   *   $	�� - ��64   �� 5`   *   ��64   �� *
VendorName+5a   '   5b   #   5d      5e      �� 5g      -   5        $��TradeValuePanel��5g   I   $ Vertical=�6!   $��5,0,0,0q�68   $y�	Collapsed=�6`   .+�5h      �� $��Trade Value:��6$   #��[ 68   $��5,10,0,5q�6c   �� 5i      -   5        $ TradeValueTextBox��5i   "   #��[ 6=   $	��350��6t   $
��Left=�6�   $��5,0,0,0q�6�   5j      �� 5l      $ Vertical=�6!   $��5,0,0,0q�68   .+�5m      �� $I��CDoes the product have variants, including multiple sizes or colors?��6$   #��[ 6o   $��5,10,0,5q�6�   �� 5n      $ 
Horizontal=�6%   .+�5o      	� -   5        $��rbYes��5o   *   $	��Yes��67    	� 	GroupName$ Variants��6E   $	��5,0q�6x   	� 5p      -	   5        $
��rbNo��5p   *   $��No��66   $ Variants��6C   $	��5,0q�6v   5q      5r      �� 5t      -
   5        $��VariantPanel��5t   !   $y�	Collapsed=�65   $��5,0,0,10q�6L   .+�5u      �� $F��@Add product variant types (e.g., 'Color', 'Size') (Max 3 types):��6$   #��[ 6l   $��5,10,0,5q�6�   �� 5v      -   5        $��VariantTypeList��5v   '   ��5w      �� 5x   "   .R�5y   &   �� $ Vertical=�61   $��0,0,0,10q�6H   .+�5z   *   �� $ 
Horizontal=�65   .+�5{   .   �� $	��200��66   #��[ 6B   $��0,0,10,0q�6y   $��Variant Name��6�   �� 5|   .   -   5        $��-��5|   5    $  LightMinimalistButtonStyle_Short#��[ 6A   $��25��6{   6�   5}   +   �� 5~   *   $��VariantValuesStack��65   $ Vertical=�6O   .+�5   .   �� $ 
Horizontal=�69   .+�5�   2   �� $	��100��6:   #��[ 6F   $��0,0,10,0q�6}   $��Variant Value��6�   �� 5�   2   $	��150��6:   #��[ 6F   $��0,0,10,0q�6}   $	��UPC��6�   �� 5�   2   -   5        $��+��5�   9   #��[ 6E   $��25��6   6�   �� 5�   2   -   5        $��-��5�   9   #��[ 6E   $��25��6   6�   5�   /   5�   +   5�   '   5�   #   5�      5�      �� 5�      -   5        $��btnAddVariantType��5�      #��[ 66   $	��200��6p   $��Add New Variant Type��6|   $y�	Collapsed=�6�   $
��Left=�6�   $��	10,10,0,0q�6�   �� 5�      -   5        $��SingleUPCPanel��5�   !   $��5,0,0,0q�67   .+�5�      �� $
��UPC:��6$   #��[ 60   $��5,10,0,5q�6[   �� 5�      -   5        $ SingleUPCTextBox��5�   "   #��[ 6<   $
��1155��6s   $
��Left=�6�   $��5,0,0,0q�6�   5�      �� 5�      $ Vertical=�6!   $��5,0,0,0q�68   .+�5�      �� $��Add to Quick Select:��6$   #��[ 6@   $��5,10,0,5q�6k   �� 5�      -   5         ��Name$ AddToQuickSelectCheckBox��5�   #   $	��Yes��6E   $��5q�6S   5�      �� 5�      $ Vertical=�6!   $��5,0,0,0q�68   .+�5�      �� $'��!Track Inventory for this product?��6$   #��[ 6M   $��5,10,0,5q�6x   �� 5�      -   5        $ TrackInventoryCheckBox��5�   #   $	��Yes��6C   $��5q�6Q    d� 	IsChecked$
 TrueY�6\   5�      5�      5�      �� 5�      $��2��6   $ 0,0,5,5��6"   ��5�      �� 5�      $	 0,0&�6*   $	 0,1&�6;   .��5�      �   FooterGradientStop1# [ 6'   $ 0Q�6T   � 5�       	 FooterGradientStop2# [	 6'   $ 1Q�6T   5�      ._�5�      �� $��Center=�6   $��5q�68   .+�5�      �� $ 
Horizontal=�6!   $��Right=�6:   .+�5�      �� -   5        $��	btnCancel��5�   !    (
 $LightMinimalistButtonStyle_Inventory#��[
 62   $	��150��6p   $��0,0,5,0q�6|   .��5�      �� $��Cancel��6(     LightMinimalistButtonText#��[ 66   5�      �� 5�      -   5        $��btnSaveProduct��5�   !   #��[
 67   $	��150��6�   $��5,0,0,0q�6�   .��5�      �� $��Save Product��6(   #��[ 6<   5�      5�      5�      5�      5�      5�      5�      