   M S B A M L   `   `   `  ����  MerlinBackOffice-    'MerlinBackOffice.Pages.EditEmployeePage   5      W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5      R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5      Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5      @d2http://schemas.microsoft.com/expression/blend/2008     5        N� Title$  Edit Employee��5       � 	Resources 5	       �� 
TargetType �� Property �� Value��%T  '��       '�� K     '�� �     '�� �     �� 5   
   " ��6   .�5      ��  � ��$ Inter)�6+   5      �� 5   
   " ��6   .�5      ��  � ��$ Inter)�6+   5      �� 5   
   " ��6   .�5      ��  � ��$ Inter)�6+   5      �� 5   
   " ��6   .�5      ��  � ��$ Inter)�6+   5      5      .��5      �  � RowDefinitions 5   
   �� 5      $
��Auto �6   �� 5      $��* �6   .+�5    
   �� $��0��6   ���#EDEDED �� Padding$ 10q�63   ._�5!      �� .+�5"      �� $��Edit Employee��6   $��24(�61   $
��Bold"�6?   ���#3333336Q   �� 5#      $I��CSearch for an employee, update their details, and save the changes.��6   $��14(�5$      ���#555555$��0,5,0,0q�6?   5%      5&      �� 5)   
   $��1��6   $��10q�6$   $
��Auto=�60   .��5*      �� .+�5,      ��  �� Orientation$ 
Horizontal=�6   $��0,0,0,10q�66   .+�5-      �� $��Search Employee by ID:��6    $��Center=�6>   $��0,0,10,0q�6Y   �� 5.      -   5         ��Name$ SearchEmployeeTextBox��5.      $	��200��6=    !  LightMinimalistSearchBoxStyle#��[  6I   �� 5/      -   5        $��Search��5/      $	��100��6.   $��10,0,0,0q�6:    $  LightMinimalistButtonStyle_Short#��[ 6L   6�   50      �� 53      $��0,10,0,0q�6   .+�54      �� $��Employee Details:��6    $
��Bold"�69   $��0,0,0,10q�6K   �� 56      $��First Name:��6    $��0,5,0,2q�63   �� 57      -   5        $ FirstNameTextBox��57      #��[  68   �� 59      $��
Last Name:��6    $��0,10,0,2q�62   �� 5:      -   5        $ LastNameTextBox��5:      #��[  67   �� 5<      $��Primary Location:��6    $��0,10,0,2q�69   �� 5=      -   5        	 ��Name$	 PrimaryLocationComboBox��5=         LightMinimalistComboBoxStyle#��[ 6@   �� 5?      $��Wage:��6    $��0,10,0,2q�6-   �� 5@      -   5        $ WageTextBox��5@      #��[  63   �� 5B      $ 
Horizontal=�6!   $��0,10,0,2q�6:   .+�5C      �� $��Tip Eligible:��6$   $��0,0,10,0q�69   $��Center=�6K   	� 5D      -   5        
 	�Name$
 rbTipYes��5D   &   $	��Yes��68    	� 	GroupName$ TipEligibility��6F   6a   	� 5E      -   5        $
 rbTipNo��5E   &   $��No��67   $ TipEligibility��6D    d� 	IsChecked$
 TrueY�6y   5F      �� 5H      $��Commission Rate (%):��6    $��0,10,0,2q�6<   �� 5I      -	   5        $ CommissionRateTextBox��5I      #��[  6=   �� 5K      $��Commission Limit:��6    $��0,10,0,2q�69   �� 5L      -
   5        $ CommissionLimitTextBox��5L      #��[  6>   5M      �� 5P      -   5        $��Save Changes��5P      $	��200��60   $��0,20,0,0q�6<   $��Center=�6N   #��[ 6k   6�   5Q      5R      5S      5T      