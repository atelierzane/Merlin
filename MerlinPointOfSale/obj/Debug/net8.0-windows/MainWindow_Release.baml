   M S B A M L   `   `   `  ����L  HMerlinPointOfSale, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:MerlinPointOfSale.ControlsMerlinPointOfSale.Controls  5   	   *    $MerlinPointOfSale.MainWindow_Release   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   L HWindowExtras.Wpf, Version=1.0.29.0, Culture=neutral, PublicKeyToken=null7winex+https://github.com/yariker/WindowExtras.Wpf  5   	   X TFontAwesome.Sharp, Version=6.6.0.0, Culture=neutral, PublicKeyToken=d16d1e4e568ec10fFfa=http://schemas.awesome.incremented/wpf/xaml/fontawesome.sharp  5   	   4local(clr-namespace:MerlinPointOfSale.Controls  5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   -   5          �Name$  
mainWindow��5   	    � Title$ 
MainWindow��5   	   $	��900��5   	   $
��1450��5	   	    � WindowStyle$
 None=�5
   	    � AllowsTransparency$
 True��5   	   
������  � WindowStartupLocation$ CenterScreen=�5   	     WindowExtras.Wpf.WindowEx   WindowShadow 5      +  %WindowExtras.Wpf.Shadows.WindowShadow  5   
   -   5        
shadow��   OffsetX$ 0Q�5   -      OffsetY$	 1.5Q�69      Opacity$ 0Q�6G   	   Radius$	 35Q�6S   
 J� InputBindings
 5      �� 5   
    �� Gesture$ Ctrl+S��6=    �� Command 6   �� *ToggleSearchCommand+6=    � 	Resources 5       �� MergedDictionaries �� Source   PageTransitionOut �� TargetProperty	 W� To g� Duration W� EasingFunction.  (System.Windows.Media.Animation.CubicEase7  1System.Windows.Media.Animation.EasingFunctionBase   
EasingMode  PageTransitionIn W� From g� 	BeginTime  ShowAlternateLogo �� 
TargetName  ShowFullLogo�� 5   
    5      �� 5      $! /Style/WindowResources.xaml?�6%   �� 5       $ /Style/ButtonStyles.xaml?�6%   %�  &	        &	 -    &	 �    &	 �    �� 5$      .��5&      W� $ Opacity�6"    05'      $ 0:0:0.2D�6%    5(        5)      $ EaseIn=�6$   5+      W� 5.      $, &RenderTransform.(TranslateTransform.X)�6"    1005/      $ 0:0:0.4D�6'    50        51      $ EaseIn=�6$   53      54      �� 57      .��59      W� $, &RenderTransform.(TranslateTransform.X)�6"    -1005:       06*   $ 0:0:0.4D�61    5;        5<      $ EaseOut=�6$   5>      W� 5A      $ Opacity�6"    05B       16'   $ 0:0:0.4D�6.    0:0:0.16A    5C        5D      $ EaseOut=�6$   5F      5G      �� 5I      .��5K      W� $ FullLogo��6"   $ RenderTransform.ScaleX�5L       05M      $ 0:0:0.15D�5N      W� 5O      $ FullLogo��6"   $ Opacity�5P       05Q      $ 0:0:0.1D�5R      W� 5U      $ AlternateLogo��6"   $ RenderTransform.ScaleX�5V       05W       15X      $ 0:0:0.15D�5Y       0:0:0.25Z      W� 5[      $ AlternateLogo��6"   $ Opacity�5\       05]       .55^      $ 0:0:0.15D�5_       0:0:0.255`      5a      �� 5c      .��5e      W� $ AlternateLogo��6"   $ RenderTransform.ScaleX�5f       05g      $ 0:0:0.15D�5h      W� 5i      $ AlternateLogo��6"   $ Opacity�5j       05k      $ 0:0:0.15D�5l      W� 5o      $ FullLogo��6"   $ RenderTransform.ScaleX�5p       05q       15r      $ 0:0:0.3D�5s       0:0:0.055t      W� 5u      $ FullLogo��6"   $ Opacity�5v       .355w      $ 0:0:0.3D�5x       0:0:0.25y      5z      5|      .��5�      � .+�5�   
   �� -   5         ��Name$ 
mainBorder��5�        mainBorderStyle#��[ 5�      $��Stretch=�5�      $��Stretch=�5�      $��0q�5�      ��5�      �� 5�       �� Color$ Black��6"    �� Opacity$
 0.01Q�60   5�      � 5�   
   -   5         �Name$ ContentGrid��5�       � RowDefinitions 5�      �� 5�      $��40 �6    �� 5�      -   5         ��Name$ 	SearchRow��5�       $��0 �63   �� 5�      �� 5�      $
��Auto �6     � ColumnDefinitions 5�      �� 5�      -   5         ��Name$ 
MenuColumn��5�   #   $��60 �67   �� 5�      $��* �6#   z�5�      T� 5�        T� X$  0Q�6%   .+�5�      �� -   5        $ 
titleBarBg��5�        mainBorderBackground#��[ 6)   $��0��6W   $��2��6d   ! J� Opacity$
! .325Q�6x   $��1.5,1.5,1.5,0q�6�   �� 5�      -   5        $ 	menuBarBg��5�      #��[ 6(   $��1��6V   $��0��6c   $��3��6s   $	! .75Q�6�   $��1.5,0,0,1.5q�6�   �� 5�      -	   5        $ searchBarBg��5�      #��[ 6*   $��1��6X   $��1��6e   $
! .525Q�6v   $��0,0,1.5,1.5q�6�   �� 5�      -
   5        $ mainBg��5�      #��[ 6%   $��1��6S   $��2��6`   $��3��6p   $
! .525Q�6�   $��0,0,1.5,1.5q�6�   �� 5�      -   5        " ��Name$" glowSeparator��5�      $��1��6/   $��0��6:   $��2��6G   $
��1500��6[   $��Center=�6h   $
! .375Q�6�   $��0,39,0,0q�6�   �� 5�      -   5        $" glowSeparatorBG��5�      $��40��61   $��0��6=   $��2��6J   $
��1500��6^   $��Center=�6k   $
! .065Q�6�   $��0,0,0,0q�6�   # J� IsHitTestVisible$# False��6�   �� 5�      -   5        $ ��Name$$ glowEffectCanvas��5�      
������ $��2��6H   $# False��6\   �� 5�      $��1��63   ��6   �� 63    � 5�      -   5        %  �Name$% 	MainFrame��5�      $��1��6'   $��2��67   $��2��6D   &  � NavigationUIVisibility$& Hidden=�6U   z�5�      T� 5�      $  0Q�6)   5�      5   /MerlinPointOfSale.Controls.DialogWindowTitleBar  5�      -   5        '  Name$' windowTitleBar��5�   )   (   Title$ ( Dynamic Title - Merlin ROS��6A   $��0��6d   $��2��6q   �� 5�      $��1��6   $��1��6*   $��Center=�67   .+�5�      .   (MerlinPointOfSale.Controls.MainSearchBar  -   5        )  Name$) mainSearchBar��5�   &   $��10q�6=   5�      �� 5�      $��0��6   $��1��6)   $��2��66   $	��Top=�6G   $
��Left=�6_   $��
-1.5,5,5,5q�6z   .+�5�      d� -   5        * d�Name$* MenuToggleButton��5�      
������ 
�������$
��Left=�6e   $��
10,20,10,5q�6�     controlToggleButtonLight#��[ 6�   z�5�      T� 5�      -   5        MenuButtonTransform��5�   -   5�      d� 5�      -   5        $* SearchToggleButton��5�      $��0��5�      $��0��5�        searchToggleButtonLight#��[ 5�      
������ $
��Left=�5�      $��Center=�5�      $��40��5�      $��40��5�      $��	15,5,10,5q�5�      z�5�      T� 5�      -   5        SearchButtonTransform��5�   -   5�      �� 5�      $��1��6   
�������$	��250��5�   "   $! .2Q�5�   "   $��10,5,0,5q�5�   "   $��Center=�5�      �� 5�      -   5        + ��Name$+ 	MenuItems��5�      $	��Top=�60   $��5q�6H   .+�5�      0   *MerlinPointOfSale.Controls.MenuRadioButton  0   *MerlinPointOfSale.Controls.MenuRadioButton,   Checked$!, OnPointOfSaleButton_Checked��6�   $��Point of Sale��6,   
- � Tag$- CashRegister��6D     MenuRadioButtonStyle#��[ 6W    	 merlinCyan_brush#��[	 6�     5�      $!, OnPerformanceButton_Checked��6   $��Performance��6,   $- ChartPie��6B   #��[ 6Q   6     5�      $, OnCalendarButton_Checked��6�   $��Appointments��6,   $- Calendar��6C   #��[ 6R   6�     5�      $ , OnSchedulingButton_Checked��6{   $��
Scheduling��6,   $- Clock��6A   #��[ 6M   6{   �� 5�      $��1��6    
�������$	��250��5�   "   $! .2Q�5�   "   $��
5,5,-2.5,5q�5�   "     5�      $ , OnBackOfficeButton_Checked��6|   $��Back Office��6,   $- Boxes��6B   #��[ 6N   6|     5�      $#, OnConfigurationButton_Checked��6}   $��Configuration��6,   $
- Gear��6D   #��[ 6O   6}     5�      $", OnPollingAgentButton_Checked��6�   $��Close for Polling��6,   $- PowerOff��6H   #��[ 6W   6�   5�      5�      �� 5�      -   5        . ��Name$. FullLogo��5�      $#��/Resources/Merlin_Logo_BW.png��5�      $��3��5�      $��
10,30,10,5q�5�      $! 0Q�5      $��Bottom=�5     / J� RenderTransformOrigin$/ 0.5,0.5&�5     z�5     �� 5     0 �� ScaleX$0 1Q�6)   1 �� ScaleY$1 1Q�64   5     �� 5	     -   5        $. AlternateLogo��5	     $%��/Resources/Merlin_Logo_Icon.png��5
     $��3��5     $��10q�5     $! .5Q�5     $/ 0.5,0.5&�5     z�5     �� 5     $0 0Q�6)   $1 1Q�64   5     5     5     2 � Triggers2 5     :� 5  
   3 :� RoutedEvent$3 Window.Loaded��6   .[�5     ��  
 WindowAnimation#��[
 6   5     5!     