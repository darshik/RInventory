using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

namespace SmartInventory.Controllers
{
    [SessionExpire]
    public class SearchController : Controller
    {

        private List<string> _masterTypes; 
        public SearchController()
        {
            _masterTypes=new List<string>(){"barcode","equipments","model","contact"};
        }

        private readonly InventoryEntities _entities = new InventoryEntities();
        // GET: Search
        //public ActionResult Index(string s,string t,string selected)
        //{
        //    ViewBag.search = s;
           
        //    var filteredList = new List<Equipment>();

        //    //If you have selected some filters
        //    if (t != null)
        //    {
        //        t = t.Trim(',');
        //        var types = t.Split(',').ToList();

        //        if (types.Contains(selected))
        //            types.Remove(selected);
        //        else
        //            types.Add(selected);

        //        ViewBag.type = String.Join(",", types);

        //        if (s == null || s.Equals(String.Empty))
        //        {
        //            return View(new List<Equipment>());
        //        }

        //        if (types.Any())
        //        {
        //            foreach (var filterType in types)
        //            {
        //                foreach (var item in FilterBy(filterType, s).Where(item => !filteredList.Contains(item)))
        //                    filteredList.Add(item);
        //            }
        //        }
        //        else
        //        {
        //            foreach (var type in _masterTypes)
        //            {
        //                foreach (var item in FilterBy(type, s).Where(item => !filteredList.Contains(item)))
        //                    filteredList.Add(item);
        //            }
        //        }
               
        //    }
        //    else
        //    {
        //        //If no filters are selected
        //        if (s != null && !s.Equals(String.Empty))
        //        {
        //            foreach (var type in _masterTypes)
        //            {
        //                foreach (var item in FilterBy(type, s).Where(item => !filteredList.Contains(item)))
        //                    filteredList.Add(item);
        //            }
        //        }
        //        ViewBag.Equipment = filteredList;
        //        return View(filteredList);
        //    }

        //    ViewBag.Equipment = filteredList;
        //    return View(filteredList);
        //}

        public ActionResult Index(string s)
        {
            ViewBag.search = s;
            var filteredList = new List<Equipment>();


            if (s != null && !s.Equals(String.Empty))
            {
                foreach (var type in _masterTypes)
                {
                    foreach (var item in FilterBy(type, s).Where(item => !filteredList.Contains(item)))
                        filteredList.Add(item);
                }
            }
            ViewBag.Equipment = filteredList;
            return View(filteredList);

        }

        private List<Equipment> FilterBy(string filterType,string filterText)
        {
            filterText = filterText.ToLower();

            if (filterType.Equals("barcode"))
            {
                return _entities.Equipments.Where(x => x.BarCode != null && x.BarCode.ToLower().Contains(filterText)).ToList();
            }

            if (filterType.Equals("equipments"))
            {
                return _entities.Equipments.Where(x => x.Equipment1 != null && x.Equipment1.ToLower().Contains(filterText)).ToList();
            }
            if (filterType.Equals("contact"))
            {
                return _entities.Equipments.Where(x => x.Contact != null && x.Contact.ToLower().Contains(filterText)).ToList();
            }

            if (filterType.Equals("model"))
            {
                return _entities.Equipments.Where(x => x.Model != null && x.Model.ToLower().Contains(filterText)).ToList();
            }
            return new List<Equipment>();
        }

        public ActionResult Delete(int id)
        {
            var entity = _entities.Equipments.Find(id);
            _entities.Equipments.Remove(entity);
            _entities.SaveChanges();
            return RedirectToAction("Index");

        }

        public ActionResult AddEntry()
        {
            return View();
        }


        public ActionResult Excel_Export_Read([DataSourceRequest]DataSourceRequest request)
        {
            return Json(_entities.Equipments.ToList().ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult Excel_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }
    }

    public class SessionExpireAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            HttpContext ctx = HttpContext.Current;
            // check  sessions here
            if (HttpContext.Current.Session["EmailId"] == null)
            {
                filterContext.Result = new RedirectResult("~/UserAccount/Login");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}