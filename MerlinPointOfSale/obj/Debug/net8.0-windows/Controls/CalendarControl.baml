   M S B A M L   `   `   `  ����L  HMerlinPointOfSale, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullK*clr-namespace:MerlinPointOfSale.ConvertersMerlinPointOfSale.Converters  5      0    *MerlinPointOfSale.Controls.CalendarControl   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5      <local*clr-namespace:MerlinPointOfSale.Converters     5        � 	Resources  5	      :   4MerlinPointOfSale.Converters.DateVisibilityConverter   DateVisibilityConverter  TransitionOutForward �� TargetProperty	 W� To g� Duration W� EasingFunction2  ,System.Windows.Media.Animation.QuadraticEase7  1System.Windows.Media.Animation.EasingFunctionBase   
EasingMode  TransitionInForward W� From g� 	BeginTime  TransitionOutBackward  TransitionInBackward  MonthTransitionOutForward  MonthTransitionInForward  MonthTransitionOutBackward  MonthTransitionInBackward��%]
  &	        &	      &	 A    &	 �    &	 �    &	     &	 A    &	 �    &	 �      5
   
   6(   �� 5   
   .��5      W� $ Opacity�6    05      $ 0:0:0.2D�6!    5        5      $ EaseIn=�6$   5      W� 5      $, &RenderTransform.(TranslateTransform.X)�6    -1005      $ 0:0:0.4D�6$    5        5      $ EaseIn=�6$   5      5      �� 5   
   .��5      W� $, &RenderTransform.(TranslateTransform.X)�6    1005       06%   $ 0:0:0.2D�6,    5        5       $ EaseOut=�6$   5"      W� 5#      $ Opacity�6    05$       16#   $ 0:0:0.4D�6*    0:0:0.16=    5%        5&      $ EaseOut=�6$   5(      5)      �� 5,   
   .��5-      W� $ Opacity�6    05.      $ 0:0:0.2D�6!    5/        50      $ EaseIn=�6$   52      W� 53      $, &RenderTransform.(TranslateTransform.X)�6    10054      $ 0:0:0.4D�6#    55        56      $ EaseIn=�6$   58      59      �� 5<   
   .��5=      W� $, &RenderTransform.(TranslateTransform.X)�6    -1005>       06&   $ 0:0:0.4D�6-    5?        5@      $ EaseOut=�6$   5B      W� 5C      $ Opacity�6    05D       16#   $ 0:0:0.4D�6*    0:0:0.16=    5E        5F      $ EaseOut=�6$   5H      5I      �� 5K   
   .��5L      W� $ Opacity�6    06B   $ 0:0:0.2D�6I    5M        5N      $ EaseIn=�6$   5P      W� 5Q      $, &RenderTransform.(TranslateTransform.X)�6    -1006a   $ 0:0:0.4D�6k    5R        5S      $ EaseIn=�6$   5U      5V      �� 5Y   
   .��5Z      W� $, &RenderTransform.(TranslateTransform.X)�6    1006a    06l   $ 0:0:0.4D�6s    5[        5\      $ EaseOut=�6$   5^      W� 5_      $ Opacity�6    06B    16K   $ 0:0:0.4D�6R    0:0:0.16e    5`        5a      $ EaseOut=�6$   5c      5d      �� 5g   
   .��5h      W� $ Opacity�6    06B   $ 0:0:0.2D�6I    5i        5j      $ EaseIn=�6$   5l      W� 5m      $, &RenderTransform.(TranslateTransform.X)�6    1006a   $ 0:0:0.4D�6j    5n        5o      $ EaseIn=�6$   5q      5r      �� 5u   
   .��5v      W� $, &RenderTransform.(TranslateTransform.X)�6    -1006a    06m   $ 0:0:0.4D�6t    5w        5x      $ EaseOut=�6$   5z      W� 5{      $ Opacity�6    06B    16K   $ 0:0:0.4D�6R    0:0:0.16e    5|        5}      $ EaseOut=�6$   5      5�      5�      .��5�      � .+�5�   
   � -   5         �Name$ 	MonthView��5�      $y�Visible=�6"   	 J� Opacity$	 1Q�67   z�5�      T� 5�      
 T� X$
 0Q�6%   .+�5�      �� 
������ ._�5�      � .+�5�      ��  �� Orientation$ 
Horizontal=�6!   $��Right=�6:   $	��Top=�6V   $��	5,-75,5,0q�6n   .+�5�      �� -   5        $	��◀��5�   !    	 IconButtonStyle#��[	 5�   !   $��Center=�5�   !   $��5q�5�   !   �� 5�      -   5         ��Name$ MonthNameTextBlock��5�   $   $��Inter)�5�   $   $��Regular"�5�   $   $��45(�5�   $   ���#FFFFFF$
��20,0q�5�   $   $��Center=�5�   $   ��5�   $   �� *DisplayMonth+z�5�      T� 5�   "   $
 0Q�65   5�      �� 5�      -   5        $	��▶��5�   !   #��[	 5�   !   $��Center=�5�   !   $��5q�5�   !   5�      A� 5�       A� Rows$ 1��6"    A� Columns$ 7��6+   $��5,10,5,0q�67   .+�5�      �� $	��Sun��6$   $��Center=�6/   $
��Bold"�6L    
 merlinCyan_brush#��[
 6^   $��Inter)�6�   �� 5�      $	��Mon��6$   $��Center=�6/   $
��Bold"�6L   #��[
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
 6^   $��Inter)�6�   5�      �� 5�      -   5         ��Name$ DayButtonsContainer��5�   #   ��6@   �� *Dates+z�5�      T� 5�      $
 0Q�61   ��5�      �� 5�      .R�5�   "   A� $ 6��6.   $ 7��67   $��5,50,5,0q�6C   5�      ��5�      �� 5�      .R�5�   "   � .+�5�   &   �� -   5          DateButtonStyle#��[ 5�   -   ��5�   -   �� *
Date.Day+
 � Tag 5�   -   �� y�5�   -   �� *Date+ �� 	Converter# [  5�   #   5�      5�      5�      5�      5�      � 5�   
   -   5        $ DayView��5�      $y�	Collapsed=�6    $	 0Q�67   z�5�      T� 5�      $
 0Q�6%   .+�5�      �  � RowDefinitions 5�      �� 5�      $
��Auto �6$   �� 5�      $��* �6$   �� 5�      $
��Auto �6$   .+�5�      �� $ 
Horizontal=�6   $	��Top=�66   $��0��6N   $��10q�6[   .+�5�      �� -   5        $��◀ Back��5�      #��[	 6L   $
��Left=�5�      $��Center=�68   $��0,0,10,0q�6S   �� 5�      -	   5        $ DayViewHeader��5�       $��20(�67   $
��Bold"�6E   ���#FFFFFF6W   5�      �� 5�      $��1��6   $��10q�6,   .��5�      �� .+�5�      �� $��Appointments��6$   $��16(�68   $
��Bold"�6F   ���#00C6FF$��5q�6m   &   System.Windows.Controls.DataGrid  5�      -
   5          Name$ DayAppointmentsDataGrid��5�   #      AutoGenerateColumns$ False��5�   #   $��10q�5�   #   
������    GridLinesVisibility$
 None=�5�   #   #��[
 5�   #   $��0,1,0,0q�5�   #      HeadersVisibility$ Column=�5�   #      
IsReadOnly$
 True��5�   #      SelectionMode$ Single=�5�   #   ��5�   #   �� *Appointments+  5�      )  #System.Windows.Controls.DataGridRow �� 
TargetType �� Property �� Value �� Triggers P� Property P� Value  ellipseButtonBg_merlinCyan=  7System.Windows.Controls.Primitives.DataGridColumnHeader*  $System.Windows.Controls.DataGridCell��%  '        7  7  '      7 
 '  f    �� 5�   "   "  6(   .�5�   &   ��  � ��
 �����6C   �� 5�   &    � ��
 ���� 6C   �� 5�   &    � ��$ Inter)�6C   �� 5�   &    � ��$ 14(�6A   �� 5�   &    � ��
 ���� 6D   �� 5�   &    � ��$ 0,5,0,5q�6?   �� 5�   &    � ��$ Center=�6J    5�   &   P� 5�   *    �@ 
IsSelected$
 True��6H   . �5�   .   ��  � ��8   6K   �� 5�   .    � ��
 �����6K   �� 5�   .    � ��8  6L   �� 5�   .    � ��$ 0,0,0,2q�6P   5   +   5  #   �� 5  "   "  6(   .�5  &   ��  � �� �#FFFFFF6C   �� 5  &    � ��$ Inter)�6C   �� 5  &    � ��$
 Bold"�6C   �� 5	  &    � ��$ 16(�6A   �� 5
  &    � ��
 ���� 6C   �� 5  &    � ��8   6D   �� 5  &    � ��$ 0,0,0,1q�6H   �� 5  &    � ��$ 10,15,10,15q�6@   5  #   �� 5  "   "  6(   .�5  &   ��  � ��
 �����6C   �� 5  &    � ��
 ���� 6C   �� 5  &    � ��$ Inter)�6C   �� 5  &    � ��$ 14(�6A   �� 5  &    � ��$ Center=�6J   �� 5  &    � ��$ Center=�6S   �� 5  &    � ��$ Center=�6Q   �� 5  &    �@�FocusVisualStyle 6I   Z� �� 5  &    � ��$ 0q�6H   5  #   5        Columns 5     0  *System.Windows.Controls.DataGridTextColumn  5   "   ,	  &System.Windows.Controls.DataGridColumn  	  Header$
  Time��65   ! 	  Width5
  /System.Windows.Controls.DataGridLengthConverter$! 80
 6c   1  +System.Windows.Controls.DataGridBoundColumn"   Binding" 6C   �� *DisplayTime+6c     5!  "   $  Client��65   $	! 150
 6d   " 6E   �� *
ClientName+6d     5"  "   $  Employee��65   $	! 150
 6h   " 6G   �� *EmployeeName+6h     5#  "   $  Services��65   $	! 200
 6k   " 6G   �� *ServicesSummary+6k     5$  "   $  Add-Ons��65   $	! 200
 6h   " 6F   �� *AddOnsSummary+6h     5%  "   $
  Fees��65   $	! 100
 6c   " 6C   �� *FeesSummary+6c   4  .System.Windows.Controls.DataGridTemplateColumn  5&  "   $  Actions��69   $	! 100
 6J   #   CellTemplate# 5'  &   �� 5(  *   .R�5)  .   �� -   5        $��	Check Out��5)  5     LightMinimalistButtonStyle#��[ 5,      5+     �� 5,     5-  +   5/  #   51     54     55     �� 58     -   5        $��Add Appointment��58       ellipseButton4#��[ 6V   $��2��6~   $��Right=�6�   $��15q�6�   59     5:     5;     5<     