//------------------------------------------------------------------------------
// <copyright file="ExplorerToolWindowControl.xaml.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Controls;
using ErikEJ.SqlCeScripting;

namespace ErikEJ.SqlCeToolbox.ToolWindows
{
    /// <summary>
    /// Interaction logic for ExplorerToolWindowControl.
    /// </summary>
    public partial class ExplorerControl : UserControl
    {
        public static List<DbDescription> DescriptionCache { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExplorerToolWindowControl"/> class.
        /// </summary>
        public ExplorerControl()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Handles click on the button by displaying a message box.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="e">The event args.</param>
        [SuppressMessage("Microsoft.Globalization", "CA1300:SpecifyMessageBoxOptions", Justification = "Sample code")]
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1300:ElementMustBeginWithUpperCaseLetter", Justification = "Default event handler naming pattern")]
        private void button1_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                string.Format(System.Globalization.CultureInfo.CurrentUICulture, "Invoked '{0}'", this.ToString()),
                "ExplorerToolWindow");
        }

        internal void RefreshTables(DatabaseInfo databaseInfo)
        {
            throw new NotImplementedException();
        }

        internal void BuildDatabaseTree()
        {
            throw new NotImplementedException();
        }
    }
}