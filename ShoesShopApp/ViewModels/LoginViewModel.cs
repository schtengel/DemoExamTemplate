using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;
using System.Windows.Controls;

namespace ShoesShopApp.ViewModels
{
    public partial class LoginViewModel
    {
        public string Login { get; set; } = "";

        [RelayCommand]
        private void SignIn(PasswordBox pwd)
        {
            using var db = new AppDbContext();
            var user = db.Users.Include(u => u.Role).FirstOrDefault(u => u.Login == Login);

            if(user?.Password == pwd.Password) Nav.To(new MainWindow(user));
            else Msg.Error("Неверный логин или пароль.");
        }

        [RelayCommand]
        private void Guest() => Nav.To(new MainWindow(null));
    }
}
