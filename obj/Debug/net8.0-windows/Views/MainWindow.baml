   M S B A M L   `   `   `  ����F  BWinRep-Code, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null)clr-namespace:WinRep_CodeWinRep_Code  5   	       WinRep_Code.MainWindow   5   	   W SPresentationCore, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35\ XPresentationFramework, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35e aSystem.Windows.Controls.Ribbon, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089R NWindowsBase, Version=8.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35F 9http://schemas.microsoft.com/winfx/2006/xaml/presentation     5   	   R NSystem.Xaml, Version=8.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	   @d2http://schemas.microsoft.com/expression/blend/2008     5   	   Jmc;http://schemas.openxmlformats.org/markup-compatibility/2006     5   	   +localclr-namespace:WinRep_Code     5   	     � Title$  
MainWindow��5   	   $
��1385��5	   	   $	��755��6    � AllowsTransparency$
 True��5
   	   
������  � WindowStyle$
 None=�5   	   .��5      �� -   5        ���#1E1E1E �� CornerRadius$ 12��5      ���#333$��1q�5      ._�5   
   �  � RowDefinitions 5      �� 5      $
��Auto �6    �� 5      $��75 �6    �� 5      $	��678 �6    .+�5      �� $��Right=�6   $	��Top=�5       �� Orientation$ 
Horizontal=�5      $��1��5      $��0,10,5,0q�5      .+�5!      �� -   5        $	��−��5!      $��50��5"      $��30��6$      MinimizeButtonStyle#��[  5$      ���	#FF1E1E1E
����������	#FF333333$��1q�5(      $��0,0,5,0q�5)      ��5#      �� *CloseCommand+5*      �� 5,      -   5        $��X��5,      $��50��5-      $��30��6$     CloseButtonStyle#��[ 5/      ���	#FF1E1E1E
����������	#FF333333$��1q�53      $��0,0,5,0q�54      ��5.      �� *CloseCommand+55      56      �� 59      $��1��6   $
��Left=�5:      $ 
Horizontal=�5;      $��0,2,0,0q�5<      .+�5>      	� $��Tweaks��6     NavigationButtonStyle#��[ 5@      $��40��5A      $	��140��6*   $��25,0,0,0q�5B      ��5?      �� *NavigateToTweaksCommand+5B      	� 5D      $��Install��6   #��[ 5F      $��40��5G      $	��140��6*   $��15,0,0,0q�5H      ��5E      �� *NavigateToInstallCommand+5H      	� 5J      $
��Scan��6   #��[ 5L      $��40��5M      $	��140��6*   $��15,0,0,0q�5N      ��5K      �� *NavigateToScanCommand+5N      5O      �� 5P      $��2��6   $��
10,0,10,10q�5R      ���#1E1E1E���#333333$��1q�5U      ��5Q      �� *NavService.CurrentViewModel+5U      5W      5X      5Z      