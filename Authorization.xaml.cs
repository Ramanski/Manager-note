using System;
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
using System.Windows.Shapes;
using CodeFirst;

namespace Mannote
{
    /// <summary>
    /// Interaction logic for Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        public Authorization()
        {
            InitializeComponent();
            tbLogin.Focus();
        }

        private void bGuest_Click(object sender, RoutedEventArgs e)
        {
            App.priveleges = new Administration.Privelege
            {
                EditOperations = false,
                EditPlans = false,
                FormTrains = false
            };
            this.Close();
        }

        private void bAuthorize_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.AppStarting;
            AdminContext adminContext = new AdminContext();
            var LogPerson = adminContext.Logins.Where(l => l.login.Equals(tbLogin.Text));
            if (!LogPerson.Any())
                MessageBox.Show("Не найдено пользователя с таким логином");
            else
            {
                if (LogPerson.SingleOrDefault().password.Equals(pbPassword.Password))
                {
                    var user = LogPerson.SingleOrDefault().person;
                    App.user = user;
                    App.priveleges = user.post.privelege;
                    Mouse.OverrideCursor = Cursors.Arrow;
                    this.Close();
                }
                else
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                    MessageBox.Show("Неверный пароль");
                } 
            }
        }

        private void tb_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                if (tbLogin.Text == "")
                {
                    bGuest_Click(null, null);
                }
                else
                {
                    bAuthorize_Click(null, null);
                } 
        }
    }
}
