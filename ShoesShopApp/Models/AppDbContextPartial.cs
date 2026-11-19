using ShoesShopApp.ViewModels;

namespace ShoesShopApp.Models
{
    public partial class AppDbContext
    {
        public bool TrySave()
        {
            try
            {
                SaveChanges();
                return true;
            }
            catch (Exception ex)
            {
                Msg.Error("Не удалось сохранить данные: " + ex.GetBaseException().Message);
                return false;
            }
        }
    }
}
