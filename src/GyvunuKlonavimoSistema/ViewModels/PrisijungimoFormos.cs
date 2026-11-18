using System.ComponentModel.DataAnnotations;

namespace GyvunuKlonavimoSistema.ViewModels;

public class PrisijungimoForma
{
    [Display(Name = "El. paštas"), Required, EmailAddress]
    public string ElPastas { get; set; } = "";

    [Display(Name = "Slaptažodis"), Required, DataType(DataType.Password)]
    public string Slaptazodis { get; set; } = "";
}

public class RegistracijosForma
{
    [Display(Name = "Vardas"), Required, StringLength(50)]
    public string Vardas { get; set; } = "";

    [Display(Name = "Pavardė"), Required, StringLength(50)]
    public string Pavarde { get; set; } = "";

    [Display(Name = "El. paštas"), Required, EmailAddress, StringLength(100)]
    public string ElPastas { get; set; } = "";

    [Display(Name = "Telefono numeris"), Required, RegularExpression(@"\+?[0-9 ]{8,15}")]
    public string TelefonoNumeris { get; set; } = "";

    [Display(Name = "Asmens kodas"), Required, RegularExpression("[0-9]{11}")]
    public string AsmensKodas { get; set; } = "";

    [Display(Name = "Slaptažodis"), Required, MinLength(8), DataType(DataType.Password)]
    public string Slaptazodis { get; set; } = "";

    [Display(Name = "Pakartokite slaptažodį"), Required, DataType(DataType.Password)]
    public string Slaptazodis2 { get; set; } = "";
}
