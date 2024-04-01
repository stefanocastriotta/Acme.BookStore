using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace Acme.BookStore.Controllers;

public class HomeController : AbpController
{
    private readonly IFeatureManager _featureManager;
    private readonly IConfiguration _configuration;

    public HomeController(IFeatureManager featureManager, IConfiguration configuration)
    {
        _featureManager = featureManager;
        _configuration = configuration;
    }
    public ActionResult Index()
    {
        return Redirect("~/swagger");
    }

    public async Task<ActionResult> Test([FromQuery] string featureName) 
    {
        await foreach(var feature in _featureManager.GetFeatureNamesAsync())
        {

        }
       

        return Ok(await _featureManager.IsEnabledAsync(featureName));
    }
}
