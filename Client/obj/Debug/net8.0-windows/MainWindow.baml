   M S B A M L   `   `   `  ����A  =Client, Version=1.0.0.0, Culture=neutral, PublicKeyToken=nullclr-namespace:ClientClient  5   	       Client.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   &localclr-namespace:Client     5   	     � Title$  
MainWindow��5   	   $	��450��6   $	��800��6)   .��5	      �  � RowDefinitions 5
   
   �� 5      $
��Auto �6   �� 5      $��* �6   �� 5      $
��Auto �6    � ColumnDefinitions 5   
   �� 5      $��* �6   �� 5      $��* �6   .+�5   
   ��  �� Orientation$ 
Horizontal=�6   $��10q�6.   $��0��6:   $��2��6G   .+�5      �� -   5         ��Name$ ImagePathTextBox��5      $	��500��60   $��0,0,10,0q�6<   $��Center=�5       �� 
IsReadOnly$
 True��61   �� 5      -   5         ��Name$ BrowseButton��5      $	��100��6+   $��
Обзор��67   5      5      �� 5   
   $��1��6   $��0��6   $��10q�6.   
�������$��1q�6M   ._�5      �� -   5         ��Name$ ImageDisplay1��5      $��Uniform=�6+   5       �� 5#   
   $��1��6   $��1��6   $��10q�6.   
�������$��1q�6M   ._�5$      �� -   5        $ ImageDisplay2��5$      $��Uniform=�6+   5%      �� 5(   
   -   5        $ SubmitButton��5(      $��2��6'   $��2��64   $��10q�6H   $��Отправить��6T   $	��100��5)      $��30��6   $��Right=�6)   5*      5+      5,      