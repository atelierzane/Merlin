   M S B A M L   `   `   `  ����L  HMerlinPointOfSale, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:MerlinPointOfSale.ControlsMerlinPointOfSale.Controls  5      .    (MerlinPointOfSale.Controls.MainSearchBar   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5      4local(clr-namespace:MerlinPointOfSale.Controls  5      .��5      � $��0,0,0,0q�6   .+�5   
   �� $��0,0,0,0q�6     �� CornerRadius$  0��5	       J� Opacity$ 0Q�5
       !  MainSearchBarBackgroundLayer2#��[  5      �� 5   
   $��0,0,0,0q�6   $  0��5       ! MainSearchBarBackgroundLayer1#��[ 5      ._�5      !  FontAwesome.Sharp.IconImage  �  �FontAwesome.Sharp.IconImageBase`1[[FontAwesome.Sharp.IconChar, FontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10f]]   Icon$ Search=�6     MainSearchBarIcon#��[ 5      $��18��5      $��18��6&   $
��Left=�5      $��10,0,0,0q�5      5      �� 5   
   $��30��6   $	��465��6!   $��35,0,0,0q�6-   .+�5      �� -   5         ��Name$ 	SearchBox��5      $��0,-5,0,0q�6)     MainSearchBar#��[ 5      ��5      �� *
SearchText+ �� UpdateSourceTrigger$ PropertyChanged=�5      � 5      -   5         �Name$ popup��5      $��Bottom=�5        � 	StaysOpen$ False��5"       � AllowsTransparency$
 True��5#      $��Slide=�5$      ��5      �� *IsPopupOpen+ � PlacementTarget 5!      �� 	 �� RelativeSource	 �� 
 �� AncestorType
 UserControl � HorizontalOffset 5%      �� *PopupHorizontalOffset+ J� Effect 5&      3  -System.Windows.Media.Effects.DropShadowEffect  5'         
BlurRadius$ 50Q�6'      Opacity$ 1Q�67      ShadowDepth$ 10Q�6C   .��5)      ��  ! MainSearchBarBackgroundLayer3#��[ 6    �� Padding$ 10q�6P   $
��1200��6]   $	��400��6m   ._�5*      �� $
��Auto=�6#   .��5+      �� .+�5-      � $��5q�6#    � ColumnDefinitions 5.   "   �� 5/   &   $	��100 �67   �� 51   &   $	��150 �67   �� 53   &   $	��100 �67   �� 55   &   $	��100 �67   �� 57   &   $	��100 �67   �� 59   &   $	��100 �67   �� 5;   &   $	��150 �67   �� 5=   &   $	��450 �67   .+�5@   "   �� $	��SKU��6,   $��0��67   $��Center=�6G   $��Inter)�5A       $
��Bold"�63   $��14(�6E   
�������
������ 5B   3   �� 5C   "   $��Product Name��6,   $��1��6@   $��Center=�6P   $��Inter)�5D       $
��Bold"�63   $��14(�6E   
�������
������ 5E   3   �� 5F   "   $��Price��6,   $��2��69   $��Center=�6I   $��Inter)�5G       $
��Bold"�63   $��14(�6E   
�������
������ 5H   3   �� 5I   "   $��Sellable Qty��6,   $��3��6@   $��Center=�6P   $��Inter)�5J       $
��Bold"�63   $��14(�6E   
�������
������ 5K   3   �� 5L   "   $��Defective Qty��6,   $��4��6A   $��Center=�6Q   $��Inter)�5M       $
��Bold"�63   $��14(�6E   
�������
������ 5N   3   �� 5O   "   $��Category ID��6,   $��5��6?   $��Center=�6O   $��Inter)�5P       $
��Bold"�63   $��14(�6E   
�������
������ 5Q   3   �� 5R   "   $��Category Name��6,   $��6��6A   $��Center=�6Q   $��Inter)�5S       $
��Bold"�63   $��14(�6E   
�������
������ 5T   3   �� 5U   "   $��Actions��6,   $��7��6;   $��Center=�6K   $��Inter)�5V       $
��Bold"�63   $��14(�6E   
�������
������ 5W   3   5X      �� 5[      ��6+   �� *Suggestions+��5\   "   �� 5]   &   .R�5^   *   � $��5q�6/    5_   .   �� 5`   2   $	��100 �6C   �� 5b   2   $	��150 �6C   �� 5d   2   $	��100 �6C   �� 5f   2   $	��100 �6C   �� 5h   2   $	��100 �6C   �� 5j   2   $	��100 �6C   �� 5l   2   $	��150 �6C   �� 5n   2   $	��450 �6C   .+�5q   .   ��   SuggestionText#��[ 68   $��0��6u   ��6`   �� *SKU+6u   �� 5r   .   #��[ 68   $��1��6}   ��6`   �� *ProductName+6}   �� 5s   .   #��[ 68   $��2��6�    �� TextAlignment$ Right=�6�   ��6`   �� *Price+ �� StringFormat$ C��6�   �� 5t   .   #��[ 68   $��3��6�   $ Center=�6�   ��6`   �� *QuantityOnHandSellable+6�   �� 5u   .   #��[ 68   $��4��6�   $ Center=�6�   ��6`   �� *QuantityOnHandDefective+6�   �� 5v   .   #��[ 68   $��5��6|   ��6`   �� *
CategoryID+6|   �� 5w   .   #��[ 68   $��6��6~   ��6`   �� *CategoryName+6~   �� 5x   .   $��7��69    �� Orientation$ 
Horizontal=�6I   $��Center=�6b   .+�5y   2   �� -   5        $��Add to Sale��5y   9     ActionButtonStyle#��[ 6i   �� 5z   2   -   5        $��Add to Trade��5z   9   #��[ 6k   �� 5{   2   -   5        $��Add to Return��5{   9   #��[ 6m   �� 5|   2   -   5        $��Pin to Quick Select��5|   9   #��[ 6x   5}   /   5   +   5�   '   5�      5�      5�      5�      5�      5�      5�      5�      