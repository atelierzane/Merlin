   M S B A M L   `   `   `  ����M  IMerlinCommunicator, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullI)clr-namespace:MerlinCommunicator.ControlsMerlinCommunicator.Controls  5   	   #    MerlinCommunicator.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   L HWindowExtras.Wpf, Version=1.0.29.0, Culture=neutral, PublicKeyToken=null7winex+https://github.com/yariker/WindowExtras.Wpf  5   	   X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5   	   5local)clr-namespace:MerlinCommunicator.Controls  5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          �Name$  
mainWindow��5   	    � Title$ 
MainWindow��5   	   $	��800��5   	   $
��1400��5	   	    � WindowStyle$
 None=�5
   	    � AllowsTransparency$
 True��5   	   
������  � WindowStartupLocation$ CenterScreen=�5   	     WindowExtras.Wpf.WindowEx   WindowShadow 5      +  %WindowExtras.Wpf.Shadows.WindowShadow  5   
   -   5        
shadow��   OffsetX$ 0Q�5   -      OffsetY$	 1.5Q�69      Opacity$ 0Q�6G   	   Radius$	 35Q�6S   
 � 	Resources
 5       �� MergedDictionaries �� Source   PageTransitionOut �� TargetProperty	 W� To g� Duration W� EasingFunction.  (System.Windows.Media.Animation.CubicEase7  1System.Windows.Media.Animation.EasingFunctionBase   
EasingMode  PageTransitionIn W� From g� 	BeginTime�� 5   
    5      �� 5      $! /Style/WindowResources.xaml?�6%   �� 5      $ /Style/ButtonStyles.xaml?�6%   %�  &	        &	 -    �� 5      .��5!      W� $ Opacity�6"    05"      $ 0:0:0.2D�6%    5#        5$      $ EaseIn=�6$   5&      W� 5)      $, &RenderTransform.(TranslateTransform.X)�6"    1005*      $ 0:0:0.4D�6'    5+        5,      $ EaseIn=�6$   5.      5/      �� 52      .��54      W� $, &RenderTransform.(TranslateTransform.X)�6"    -10055       06*   $ 0:0:0.4D�61    56        57      $ EaseOut=�6$   59      W� 5<      $ Opacity�6"    05=       16'   $ 0:0:0.4D�6.    0:0:0.16A    5>        5?      $ EaseOut=�6$   5A      5B      5C      .��5H      � .+�5I   
   �� -   5         ��Name$ 
mainBorder��5I        mainBorderStyle#��[ 5J      $��Stretch=�5K      $��Stretch=�5L      $��0q�5M      ��5N      �� 5O       �� Color$ Black��6"    �� Opacity$
 0.01Q�60   5Q      � 5S   
   -   5         �Name$ ContentGrid��5S       � RowDefinitions 5T      �� 5U      $��40 �6    �� 5V      -   5         ��Name$ 	SearchRow��5V       $��0 �63   �� 5W       � ColumnDefinitions 5Z      �� 5[      -   5         ��Name$ 
MenuColumn��5[   #   $��60 �67   �� 5\      $��* �6#   z�5_      T� 5`       T� X$ 0Q�6%   .+�5e      �� -   5        $ 
titleBarBg��5e        mainBorderBackgroundLight2#��[ 6)   $��0��6]   $��2��6j    J� Opacity$
 .015Q�6~   $��1.5,1.5,1.5,0q�6�   �� 5f      -   5        $ 	menuBarBg��5f      #��[ 6(   $��1��6\   $��0��6i   $��2��6y   $
 .085Q�6�   $��1.5,0,0,1.5q�6�   �� 5g      -	   5        $ mainBg��5g      #��[ 6%   $��1��6Y   $��2��6f   $��1��6v   $
 .045Q�6�   $��0,0,1.5,1.5q�6�   �� 5j      -
   5         ��Name$ glowSeparator��5j      $��1��6/   $��0��6:   $��2��6G   $
��1500��6[   $��Center=�6h   $
 .775Q�6�   $��0,39,0,0q�6�   �� 5k      -   5        $ glowSeparatorBG��5k      $��40��61   $��0��6=   $��2��6J   $
��1500��6^   $��Center=�6k   $
 .065Q�6�   $��0,0,0,0q�6�    J� IsHitTestVisible$ False��6�   �� 5m      -   5          ��Name$  glowEffectCanvas��5m      
������ $��2��6H   $ False��6\   �� 5n      $��1��63   ��6   �� 63    � 5p      -   5        !  �Name$! 	MainFrame��5p      $��1��6'   $��2��67   $��2��6D   "  � NavigationUIVisibility$" Hidden=�6U   z�5q      T� 5r      $ 0Q�6)   5t      0   *MerlinCommunicator.Controls.WindowTitleBar  5v      -   5        #  Name$# windowTitleBar��5v   #   $   Title$ $ Dynamic Title - Merlin ROS��6;   $��0��6^   $��2��6k   �� 5|      $��0��6   $��1��6)   $��2��66   $	��Top=�6G   $
��Left=�6_   $��
-1.5,5,5,5q�6z   .+�5}      d� -   5        % d�Name$% MenuToggleButton��5}      
������ $
��Left=�6R   $��
10,20,10,5q�6m     controlToggleButton#��[ 6�   z�5~      T� 5      -   5        MenuButtonTransform��5   -   5�      �� 5�      -   5        & ��Name$& 	MenuItems��5�      $	��Top=�60   $��5q�6H   .+�5�      1   +MerlinCommunicator.Controls.MenuRadioButton  $
��Home��6,   
' � Tag$' House��6;     MenuRadioButtonStyleDark#��[ 6G   �� 5�      $��1��6    ���#181818$	��250��5�   "   $ .2Q�5�   "   $��
5,5,-2.5,5q�5�   "   5�      5�      5�      5�      ( � Triggers( 5�      :� 5�   
   ) :� RoutedEvent$) Window.Loaded��6   .[�5�      ��   WindowAnimation#��[ 6   5�      5�      