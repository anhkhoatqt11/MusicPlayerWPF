﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace KMusic
{
    public class HomeNavButton : Grid
    {
        static HomeNavButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(HomeNavButton), new FrameworkPropertyMetadata(typeof(HomeNavButton)));
        }
    }
}
