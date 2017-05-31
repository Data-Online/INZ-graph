using System.Web;
using System.Web.Optimization;

namespace DO_Manage
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new StyleBundle("~/Content/css").Include(
                "~/Content/Site.css"));
        }
    }
}
