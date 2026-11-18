using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.TagHelpers;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace GyvunuKlonavimoSistema.Infrastructure;

// <div laukas-for="Vardas"> -> <div class="laukas"> arba <div class="laukas netinkamas">,
// kai laukas užpildytas neteisingai
[HtmlTargetElement("div", Attributes = "laukas-for")]
public class LaukasTagHelper : TagHelper
{
    [HtmlAttributeName("laukas-for")]
    public ModelExpression For { get; set; } = null!;

    [ViewContext, HtmlAttributeNotBound]
    public ViewContext ViewContext { get; set; } = null!;

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        output.AddClass("laukas", HtmlEncoder.Default);
        var busena = ViewContext.ViewData.ModelState.GetFieldValidationState(For.Name);
        if (busena == ModelValidationState.Invalid)
            output.AddClass("netinkamas", HtmlEncoder.Default);
    }
}
