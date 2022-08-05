using System.Web;
using System.Web.Optimization;

namespace Apv
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            //bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
            //            "~/Scripts/jquery-{version}.js"));
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/AdminLTE/bower/jquery/jquery.min.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            //bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
            //          "~/Scripts/bootstrap.js",
            //          "~/Scripts/respond.js"));
            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/AdminLTE/bower/bootstrap/bootstrap.min.js",
                      "~/Scripts/bootbox/bootbox.min.js",
                      "~/Scripts/AdminLTE/bower/chart.js/Chart.min.js",
                      "~/Scripts/AdminLTE/bower/raphael/raphael.min.js",
                      "~/Scripts/AdminLTE/bower/morris.js/morris.min.js",
                      "~/Scripts/AdminLTE/bower/select2/select2.full.min.js",
                      "~/Scripts/AdminLTE/bower/datatables.net/jquery.dataTables.min.js",
                      "~/Scripts/AdminLTE/bower/datatables.net-bs/dataTables.bootstrap.min.js",
                      "~/Scripts/AdminLTE/bower/datatables.net/dataTables.searchPanes.min.js",
                      "~/Scripts/AdminLTE/bower/datatables.net/dataTables.select.min.js",
                      "~/Scripts/AdminLTE/bower/datatables.net/dataTables.buttons.min.js",
                      "~/Scripts/AdminLTE/bower/datatables.net/dataTables.rowReorder.min.js",
                      "~/Scripts/AdminLTE/bower/datatables.net/jszip.min.js",
                      "~/Scripts/AdminLTE/bower/datatables.net/buttons.html5.min.js",
                      "~/Scripts/AdminLTE/bower/moment/moment.min.js",
                      "~/Scripts/AdminLTE/bower/bootstrap-datepicker/bootstrap-datepicker.min.js",
                      "~/Scripts/AdminLTE/bower/bootstrap-daterangepicker/daterangepicker.js",
                      "~/Scripts/AdminLTE/bower/jquery-slimscroll/jquery.slimscroll.min.js",
                      "~/Scripts/AdminLTE/bower/fastclick/fastclick.js",
                      "~/Scripts/AdminLTE/dist/adminlte.min.js",
                      "~/Scripts/sweetalert/sweetalert.min.js",
                      "~/Scripts/GantChart/date.js",
                      "~/Scripts/GantChart/jquery.ganttView.js",
                      "~/Scripts/GanttChart/loader.js",
                      "~/Scripts/AdminLTE/bower/ckeditor/ckeditor.js",
                      "~/Scripts/AdminLTE/bower/jquery-knob/jquery.knob.js"));

            //bundles.Add(new StyleBundle("~/Content/css").Include(
            //          "~/Content/bootstrap.css",
            //          "~/Content/site.css"));
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/AdminLTE/bower/bootstrap/css/bootstrap.min.css",
                      "~/Content/AdminLTE/bower/font-awesome/css/font-awesome.min.css",
                      "~/Content/AdminLTE/bower/Ionicons/css/ionicons.min.css",
                      "~/Content/AdminLTE/bower/datatables.net-bs/dataTables.bootstrap.min.css",
                      "~/Content/AdminLTE/bower/datatables.net-bs/searchPanes.dataTables.min.css",
                      "~/Content/AdminLTE/bower/datatables.net-bs/select.dataTables.min.css",
                      "~/Content/AdminLTE/bower/datatables.net-bs/buttons.dataTables.min.css",
                      "~/Content/AdminLTE/bower/datatables.net-bs/rowReorder.dataTables.min.css",
                      "~/Content/AdminLTE/bower/morris.js/morris.css",
                      "~/Content/AdminLTE/bower/select2/select2.min.css",
                      "~/Content/AdminLTE/bower/bootstrap-daterangepicker/daterangepicker.css",
                      "~/Content/AdminLTE/bower/bootstrap-datepicker/bootstrap-datepicker.min.css",
                      "~/Content/AdminLTE/dist/css/AdminLTE.min.css",
                      "~/Content/AdminLTE/dist/css/skins/_all-skins.min.css",
                      "~/Content/sweetalert/sweetalert.css",
                      "~/Content/GantChart/jquery.ganttView.css",
                      "~/Content/ModalLoading.css",
                      "~/Content/ProgressNavbar.css"
                      ));
        }
    }
}
