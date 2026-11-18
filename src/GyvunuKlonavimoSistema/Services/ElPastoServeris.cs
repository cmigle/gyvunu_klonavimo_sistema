using System.Net.Mail;
using System.Text;

namespace GyvunuKlonavimoSistema.Services;

public interface IElPastoServeris
{
    bool Siusti(Laiskas laiskas);
}

public class ElPastoNustatymai
{
    public string Siuntejas { get; set; } = "sistema@klonavimas.lt";
    // Nenurodžius SMTP serverio laiškai įrašomi kaip .eml failai į Katalogas
    public string? SmtpServeris { get; set; }
    public int SmtpPrievadas { get; set; } = 25;
    public string Katalogas { get; set; } = "laiskai";
}

// «actor» El. pašto serveris pasiekiamas per SMTP
public class SmtpElPastoServeris(ElPastoNustatymai nustatymai, IWebHostEnvironment aplinka, ILogger<SmtpElPastoServeris> log) : IElPastoServeris
{
    public bool Siusti(Laiskas laiskas)
    {
        try
        {
            using var zinute = new MailMessage(nustatymai.Siuntejas, laiskas.Gavejas, laiskas.Tema, laiskas.Tekstas)
            {
                BodyEncoding = Encoding.UTF8,
                SubjectEncoding = Encoding.UTF8
            };
            using var klientas = KurtiKlienta();
            klientas.Send(zinute);
            log.LogInformation("Laiškas „{Tema}“ išsiųstas {Gavejas}", laiskas.Tema, laiskas.Gavejas);
            return true;
        }
        catch (Exception e) when (e is SmtpException or IOException or InvalidOperationException)
        {
            log.LogWarning(e, "Nepavyko išsiųsti laiško {Gavejas}", laiskas.Gavejas);
            return false;
        }
    }

    private SmtpClient KurtiKlienta()
    {
        if (!string.IsNullOrWhiteSpace(nustatymai.SmtpServeris))
            return new SmtpClient(nustatymai.SmtpServeris, nustatymai.SmtpPrievadas);
        var katalogas = Path.Combine(aplinka.ContentRootPath, nustatymai.Katalogas);
        Directory.CreateDirectory(katalogas);
        return new SmtpClient { DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory, PickupDirectoryLocation = katalogas };
    }
}
