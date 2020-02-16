using System.Linq;
using System.Windows;
using System.Windows.Input;
using CodeFirst;

namespace Mannote
{
    /// <summary>
    /// Interaction logic for Authorization.xaml
    /// </summary>
    public partial class Authorization : Window
    {
        private static NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();

        public Authorization()
        {
            logger.Debug("Запуск авторизации пользователя");
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
            logger.Info("Пользователь вошел в систему как гость");
            this.Close();
        }

        private void bAuthorize_Click(object sender, RoutedEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.AppStarting;
            AdminContext adminContext = new AdminContext();
            logger.Debug("Поиск введенных логина и пароля в БД");
            var LogPerson = adminContext.Logins.Where(l => l.login.Equals(tbLogin.Text));
            if (!LogPerson.Any())
            {
                MessageBox.Show("Не найдено пользователя с таким логином");
                logger.Info($"В БД не найдено пользователя с логином \"{tbLogin.Text}\"");
            }
            else
            {
                if (LogPerson.SingleOrDefault().password.Equals(pbPassword.Password))
                {
                    var user = LogPerson.SingleOrDefault().person;
                    App.user = user;
                    App.priveleges = user.post.privelege;
                    Mouse.OverrideCursor = Cursors.Arrow;
                    logger.Info($"Пользователь под Id №{user.PersonId} в должности {user.post.Name} вошел в систему");
                    this.Close();
                }
                else
                {
                    Mouse.OverrideCursor = Cursors.Arrow;
                    logger.Info("Введен неверный пароль");
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
