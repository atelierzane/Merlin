   M S B A M L   `   `   `  ����L  HMerlinPointOfSale, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:MerlinPointOfSale.ControlsMerlinPointOfSale.Controls  5      5    /MerlinPointOfSale.Controls.PointOfSaleSearchBar   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5      4local(clr-namespace:MerlinPointOfSale.Controls  5      .��5      � .+�5   
   �� $��0,0,0,0q�6     �� CornerRadius$  0��5	       J� Opacity$ 0Q�5
       !  MainSearchBarBackgroundLayer2#��[  5      �� 5   
   $��0,0,0,0q�6   $  0��5       ! MainSearchBarBackgroundLayer1#��[ 5      ._�5      !  FontAwesome.Sharp.IconImage  �  �FontAwesome.Sharp.IconImageBase`1[[FontAwesome.Sharp.IconChar, FontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10f]]   Icon$ Search=�6     MainSearchBarIcon#��[ 5      $��18��5      $��18��6&   $
��Left=�5      $��10,0,0,0q�5      5      �� 5   
   $��30��6   $	��465��6!   $��35,0,0,0q�6-   .+�5      �� -   5         ��Name$ 	SearchBox��5      $��0,-5,0,0q�5        MainSearchBar#��[ 5      $y�Visible=�5      ��5      �� *
SearchText+ �� UpdateSourceTrigger$ PropertyChanged=�5      � 5       -   5         �Name$ popup��5       $��Bottom=�5"       � 	StaysOpen$ False��5$       � AllowsTransparency$
 True��5%      $��Slide=�5&      ��5!      �� *IsPopupOpen+ � PlacementTarget 5#      �� 	 �� ElementName$	 	SearchBox��
 J� Effect
 5'      3  -System.Windows.Media.Effects.DropShadowEffect  5(         
BlurRadius$ 50Q�6'      Opacity$ 1Q�67      ShadowDepth$ 10Q�6C   .��5*      ��  ! MainSearchBarBackgroundLayer3#��[ 6    �� Padding$ 10q�6P   $
��1200��6]   $	��400��6m   ._�5+      �� $
��Auto=�6#   .��5,      �� .+�5.      � $��5q�6#    � ColumnDefinitions 5/   "   �� 50   &   $	��100 �67   �� 52   &   $��* �67   �� 54   &   $	��100 �67   �� 56   &   $	��100 �67   �� 58   &   $	��100 �67   �� 5:   &   $	��100 �67   �� 5<   &   $	��150 �67   �� 5>   &   $	��450 �67   .+�5A   "   �� $	��SKU��6,   $��0��67   $��Center=�6G   $��Inter)�5B       $
��Bold"�63   $��14(�6E   
�������
������ 5C   3   �� 5D   "   $��Product Name��6,   $��1��6@   $��Center=�6P   $��Inter)�5E       $
��Bold"�63   $��14(�6E   
�������
������ 5F   3   �� 5G   "   $��Price��6,   $��2��69   $��Center=�6I   $��Inter)�5H       $
��Bold"�63   $��14(�6E   
�������
������ 5I   3   �� 5J   "   $��Sellable Qty��6,   $��3��6@   $��Center=�6P   $��Inter)�5K       $
��Bold"�63   $��14(�6E   
�������
������ 5L   3   �� 5M   "   $��Defective Qty��6,   $��4��6A   $��Center=�6Q   $��Inter)�5N       $
��Bold"�63   $��14(�6E   
�������
������ 5O   3   �� 5P   "   $��Category ID��6,   $��5��6?   $��Center=�6O   $��Inter)�5Q       $
��Bold"�63   $��14(�6E   
�������
������ 5R   3   �� 5S   "   $��Category Name��6,   $��6��6A   $��Center=�6Q   $��Inter)�5T       $
��Bold"�63   $��14(�6E   
�������
������ 5U   3   �� 5V   "   $��Actions��6,   $��7��6;   $��Center=�6K   $��Inter)�5W       $
��Bold"�63   $��14(�6E   
�������
������ 5X   3   5Y      �� 5\      ��6+   �� *Suggestions+��5]   "   �� 5^   &   .R�5_   *   � $��5q�6/    5`   .   �� 5a   2   $	��100 �6C   �� 5c   2   $��* �6C   �� 5e   2   $	��100 �6C   �� 5g   2   $	��100 �6C   �� 5i   2   $	��100 �6C   �� 5k   2   $	��100 �6C   �� 5m   2   $	��150 �6C   �� 5o   2   $	��450 �6C   .+�5r   .   ��   SuggestionText#��[ 68   $��0��6u   ��6`   �� *SKU+6u   �� 5s   .   #��[ 68   $��1��6}   ��6`   �� *ProductName+6}   �� 5t   .   #��[ 68   $��2��6�    �� TextAlignment$ Right=�6�   ��6`   �� *Price+ �� StringFormat$ C��6�   �� 5u   .   #��[ 68   $��3��6�   $ Center=�6�   ��6`   �� *QuantityOnHandSellable+6�   �� 5v   .   #��[ 68   $��4��6�   $ Center=�6�   ��6`   �� *QuantityOnHandDefective+6�   �� 5w   .   #��[ 68   $��5��6|   ��6`   �� *
CategoryID+6|   �� 5x   .   #��[ 68   $��6��6~   ��6`   �� *CategoryName+6~   �� 5y   .   $��7��69    �� Orientation$ 
Horizontal=�6I   $��Center=�6b   .+�5z   2   �� -   5        $��Add to Sale��5z   9     ActionButtonStyle#��[ 6�    � DataContext 6p   �� 6�   �� 5{   2   -   5        $��Add to Trade��5{   9   #��[ 6k   �� 5|   2   -   5        $��Add to Return��5|   9   #��[ 6m   5}   /   5~   +   5   '   5�      5�      5�      5�      5�      5�      5�      5�      