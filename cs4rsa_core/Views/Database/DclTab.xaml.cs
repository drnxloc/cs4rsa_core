﻿using Cs4rsa.Cs4rsaDatabase.Models;
using Cs4rsa.ViewModels.Database;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace Cs4rsa.Views.Database
{
    public partial class DclTab : UserControl
    {
        public DclTab()
        {
            InitializeComponent();
        }

        private async void BtnViewCache_Clicked(object sender, RoutedEventArgs e)
        {
            Keyword kw = (Keyword)((Hyperlink)sender).DataContext;
            await ((DclTabViewModel)DataContext).ViewCacheCommand.ExecuteAsync(kw.CourseId);
        }
    }
}
