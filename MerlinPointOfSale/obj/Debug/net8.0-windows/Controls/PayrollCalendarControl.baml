   M S B A M L   `   `   `  ����L  HMerlinPointOfSale, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullG(clr-namespace:MerlinPointOfSale.ControlsMerlinPointOfSale.Controls  5      K*clr-namespace:MerlinPointOfSale.ConvertersMerlinPointOfSale.Converters  5      7    1MerlinPointOfSale.Controls.PayrollCalendarControl   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      Blocalcontrols(clr-namespace:MerlinPointOfSale.Controls     5      <local*clr-namespace:MerlinPointOfSale.Converters     5        � 	Resources  5
      :   4MerlinPointOfSale.Converters.DateVisibilityConverter   DateVisibilityConverter  TransitionOutForward �� TargetProperty	 W� To g� Duration W� EasingFunction2  ,System.Windows.Media.Animation.QuadraticEase7  1System.Windows.Media.Animation.EasingFunctionBase   
EasingMode  TransitionInForward W� From g� 	BeginTime  TransitionOutBackward  TransitionInBackward  MonthTransitionOutForward  MonthTransitionInForward  MonthTransitionOutBackward  MonthTransitionInBackward��%]
  &	        &	      &	 A    &	 �    &	 �    &	     &	 A    &	 �    &	 �      5   
   6(   �� 5   
   .��5      W� $ Opacity�6    05      $ 0:0:0.2D�6!    5        5      $ EaseIn=�6$   5      W� 5      $, &RenderTransform.(TranslateTransform.X)�6    -1005      $ 0:0:0.4D�6$    5        5      $ EaseIn=�6$   5      5      �� 5   
   .��5      W� $, &RenderTransform.(TranslateTransform.X)�6    1005       06%   $ 0:0:0.2D�6,    5         5!      $ EaseOut=�6$   5#      W� 5$      $ Opacity�6    05%       16#   $ 0:0:0.4D�6*    0:0:0.16=    5&        5'      $ EaseOut=�6$   5)      5*      �� 5-   
   .��5.      W� $ Opacity�6    05/      $ 0:0:0.2D�6!    50        51      $ EaseIn=�6$   53      W� 54      $, &RenderTransform.(TranslateTransform.X)�6    10055      $ 0:0:0.4D�6#    56        57      $ EaseIn=�6$   59      5:      �� 5=   
   .��5>      W� $, &RenderTransform.(TranslateTransform.X)�6    -1005?       06&   $ 0:0:0.4D�6-    5@        5A      $ EaseOut=�6$   5C      W� 5D      $ Opacity�6    05E       16#   $ 0:0:0.4D�6*    0:0:0.16=    5F        5G      $ EaseOut=�6$   5I      5J      �� 5L   
   .��5M      W� $ Opacity�6    06B   $ 0:0:0.2D�6I    5N        5O      $ EaseIn=�6$   5Q      W� 5R      $, &RenderTransform.(TranslateTransform.X)�6    -1006a   $ 0:0:0.4D�6k    5S        5T      $ EaseIn=�6$   5V      5W      �� 5Z   
   .��5[      W� $, &RenderTransform.(TranslateTransform.X)�6    1006a    06l   $ 0:0:0.4D�6s    5\        5]      $ EaseOut=�6$   5_      W� 5`      $ Opacity�6    06B    16K   $ 0:0:0.4D�6R    0:0:0.16e    5a        5b      $ EaseOut=�6$   5d      5e      �� 5h   
   .��5i      W� $ Opacity�6    06B   $ 0:0:0.2D�6I    5j        5k      $ EaseIn=�6$   5m      W� 5n      $, &RenderTransform.(TranslateTransform.X)�6    1006a   $ 0:0:0.4D�6j    5o        5p      $ EaseIn=�6$   5r      5s      �� 5v   
   .��5w      W� $, &RenderTransform.(TranslateTransform.X)�6    -1006a    06m   $ 0:0:0.4D�6t    5x        5y      $ EaseOut=�6$   5{      W� 5|      $ Opacity�6    06B    16K   $ 0:0:0.4D�6R    0:0:0.16e    5}        5~      $ EaseOut=�6$   5�      5�      5�      .��5�      � .+�5�   
   � -   5         �Name$ 	MonthView��5�      $y�Visible=�6"   	 J� Opacity$	 1Q�67   z�5�      T� 5�      
 T� X$
 0Q�6%   .+�5�      �� 
������ ._�5�      � .+�5�      ��  �� Orientation$ 
Horizontal=�6!   $��Right=�6:   $	��Top=�6V   $��	5,-75,5,0q�6n   .+�5�      �� $	��◀��6!    	 IconButtonStyle#��[	 5�      $��Center=�5�      $��5q�5�      �� 5�      -   5         ��Name$ MonthNameTextBlock��5�   $   $��Inter)�5�      $��Regular"�5�      $��45(�5�      ���#FFFFFF$
��20,0q�5�      $��Center=�5�      ��5�      �� *DisplayMonth+z�5�      T� 5�   "   $
 0Q�65   5�      �� 5�      $	��▶��6!   #��[	 5�      $��Center=�5�      $��5q�5�      5�      A� 5�       A� Rows$ 1��6"    A� Columns$ 7��6+   $��5,10,5,0q�67   .+�5�      �� $	��Sun��6$   $��Center=�6/   $
��Bold"�6L    
 merlinCyan_brush#��[
 6^   $��Inter)�6�   �� 5�      $	��Tue��6$   $��Center=�6/   $
��Bold"�6L   #��[
 6^   $��Inter)�6�   �� 5�      $	��Wed��6$   $��Center=�6/   $
��Bold"�6L   #��[
 6^   $��Inter)�6�   �� 5�      $	��Thu��6$   $��Center=�6/   $
��Bold"�6L   #��[
 6^   $��Inter)�6�   �� 5�      $	��Fri��6$   $��Center=�6/   $
��Bold"�6L   #��[
 6^   $��Inter)�6�   �� 5�      $	��Sat��6$   $��Center=�6/   $
��Bold"�6L   #��[
 6^   $��Inter)�6�   5�      �� 5�      -   5         ��Name$ DayButtonsContainer��5�   #   ��6@   �� *Dates+z�5�      T� 5�      $
 0Q�61   ��5�      �� 5�      .R�5�   "   A� $ 6��6.   $ 7��67   $��5,50,5,0q�6C   5�      ��5�      �� 5�      .R�5�   "   � .+�5�   &   ��   DateButtonStyle#��[ 5�      ��5�   -   �� *
Date.Day+
 � Tag 5�      �� y�5�      �� *Date+ �� 	Converter# [  5�   #   5�      5�      5�      5�      5�      � 5�   
   -   5        $ DayView��5�      $y�	Collapsed=�6    $	 0Q�67   z�5�      T� 5�      $
 0Q�6%   .+�5�      �  � RowDefinitions 5�      �� 5�      $
��Auto �6$   �� 5�      $��* �6$   .+�5�      �� $ 
Horizontal=�6   $	��Top=�66   $��0��6N   $��10q�6[   .+�5�      �� $��◀ Back��6   #��[	 6.   $
��Left=�5�      $��Center=�60   $��0,0,10,0q�6K   �� 5�      -   5        $ DayViewHeader��5�       $��20(�67   $
��Bold"�6E   ���#FFFFFF6W   5�      �� 5�      $��1��6   $��10q�6,   .��5�      �� .+�5�      �� $��Appointments��6$   $��16(�68   $
��Bold"�6F   ���#00C6FF$��5q�6m   �� 5�      -   5         ��Name$ DayAppointmentsListView��5�   #   $��5q�6D    �� View 5�      �� 5�   "   .P�5�   &   �� $
��Time��65    �� Width$ 80��6C    �� DisplayMemberBinding 6N   �� *Time+�� 5�   &   $��Client��65   $	 150��6E    6Q   �� *Client+�� 5�   &   $��Service��65   $	 200��6F    6R   �� *	Service+5�   #   5�      5�      5�      5�      5�      5�      5�      