using RepairTracker.Database;
using RepairTracker.Forms;

namespace RepairTracker;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        DbContext.Initialize();
        Application.Run(new MainMenuForm());
    }
}
