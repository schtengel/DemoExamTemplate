using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using ShoesShopApp.Models;
using System.Windows.Controls;

namespace ShoesShopApp.ViewModels;

public partial class LoginViewModel
{
    public string Login { get; set; } = "";
    [RelayCommand] private void SignIn(PasswordBox pwd)
    {
        using var db = new AppDbContext();
        var u = db.Users.Include(x => x.Role).FirstOrDefault(x => x.Login == Login);
        if (u?.Password != pwd.Password) Msg.Error("Неверный логин или пароль.");
        else Nav.To(new MainWindow(u));
    }
    [RelayCommand] private void Guest() => Nav.To(new MainWindow(null));
}