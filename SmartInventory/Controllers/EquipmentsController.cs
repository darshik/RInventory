using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using Microsoft.Office.Interop.Excel;
using SmartInventory;
using DataTable = DocumentFormat.OpenXml.Drawing.Charts.DataTable;
using Worksheet = Microsoft.Office.Interop.Excel.Worksheet;

namespace SmartInventory.Controllers
{
    [SessionExpire]
    public class EquipmentsController : Controller
    {
        private InventoryEntities db = new InventoryEntities();

        // GET: Equipments
        public ActionResult Index()
        {
            return View(db.Equipments.ToList());
        }

        [HttpPost]
        public ActionResult Pdf_Export_Save(string contentType, string base64, string fileName)
        {
            var fileContents = Convert.FromBase64String(base64);

            return File(fileContents, contentType, fileName);
        }

        // GET: Equipments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Equipment equipment = db.Equipments.Find(id);
            if (equipment == null)
            {
                return HttpNotFound();
            }
            return View(equipment);
        }

        // GET: Equipments/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Equipments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "NoOf,Equipment1,ID,BarCode,RoomNumber,Model,PurchaseDate,UNBCCode,SerialNo,PO,PurchasePrice,TotalPrice,Vendor,ModelBrandInfo,TypeOfAnalysis,PotentialUse,IsInGoodUse,CurrentUsersOfEquipment,Revenue,OtherUsefulInfo,Contact")] Equipment equipment)
        {
            // if (ModelState.IsValid)
            {
                db.Equipments.Add(equipment);
                db.SaveChanges();
                TempData["Success"] = "Item created successfully";
                return RedirectToAction("Create", "Equipments");
            }

            return View(equipment);
        }

        // GET: Equipments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Equipment equipment = db.Equipments.Find(id);
            if (equipment == null)
            {
                return HttpNotFound();
            }
            return View(equipment);
        }

        // POST: Equipments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        //[HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "NoOf,Equipment1,ID,RoomNumber,BarCode,Model,PurchaseDate,UNBCCode,SerialNo,PO,PurchasePrice,TotalPrice,Vendor,ModelBrandInfo,TypeOfAnalysis,PotentialUse,IsInGoodUse,CurrentUsersOfEquipment,Revenue,OtherUsefulInfo,Contact")] Equipment equipment, string returnUrl)
        {
            if (ModelState.IsValid)
            {
                db.Entry(equipment).State = EntityState.Modified;
                db.SaveChanges();
                return Redirect(returnUrl);
            }
            return View(equipment);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public ActionResult Edit([DataSourceRequest] DataSourceRequest request, Equipment equipment)
        {
            if (equipment != null && ModelState.IsValid)
            {
                db.Entry(equipment).State = EntityState.Modified;
                db.SaveChanges();
            }

            return Json(new[] { equipment }.ToDataSourceResult(request, ModelState));
        }

        // GET: Equipments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Equipment equipment = db.Equipments.Find(id);
            if (equipment == null)
            {
                return HttpNotFound();
            }
            return View(equipment);
        }

        // POST: Equipments/Delete/5
        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id, string returnUrl)
        {
            Equipment equipment = db.Equipments.Find(id);
            db.Equipments.Remove(equipment);
            db.SaveChanges();
            return Redirect(returnUrl);

        }

        [HttpPost]
        public ActionResult UploadBulkEquipments(HttpPostedFileBase equipmentExcel)
        {
            try
            {
                if (equipmentExcel == null) return RedirectToAction("Create");

                if (equipmentExcel.FileName.ToLower().EndsWith(".xlsx"))
                {
                    List<Equipment> equipments = ReadExcelData(equipmentExcel);
                    db.Equipments.AddRange(equipments);
                    db.SaveChanges();
                    TempData["BulkDataSuccess"] = "Equipments imported successfully";
                }
                else
                {
                    TempData["BulkDataError"] = "File Type not supported";
                }

            }
            catch (Exception)
            {
                    TempData["BulkDataError"] = "Something bad happened!!";
                    return RedirectToAction("Create");
            }
           
            return RedirectToAction("Create");
        }

       

        private List<Equipment> ReadPostedExcel(HttpPostedFileBase equipmentExcel)
        {
            var equipments=new List<Equipment>();
            var excelFilePath = Server.MapPath("/TempExcel/")+ Guid.NewGuid() + ".xlsx";
            equipmentExcel.SaveAs(excelFilePath);
 

            var xlApp = new Application();
            var xlWorkBook = xlApp.Workbooks.Open(excelFilePath, 0, true, 5, String.Empty, String.Empty, true, XlPlatform.xlWindows, "\t", false, false, 0, true, 1, 0);
            var xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.Item[1];

            try
            {

                var range = xlWorkSheet.UsedRange;

                //Locate header
                var headerRowIndex = 1;
              
                //Write all Equiment data
                for (var rowIndex = headerRowIndex + 1; rowIndex <= range.Rows.Count; rowIndex++)
                {
                    equipments.Add(new Equipment()
                    {
                        Equipment1 = range.Cells[rowIndex, 1].Value2+"",
                        BarCode = range.Cells[rowIndex, 2].Value2 + "",
                        NoOf = range.Cells[rowIndex, 3].Value2 + "",
                        RoomNumber = range.Cells[rowIndex, 4].Value2 + "",
                        Model = range.Cells[rowIndex, 5].Value2 + "",
                        PurchaseDate = range.Cells[rowIndex, 6].Value2 + "",
                        UNBCCode = range.Cells[rowIndex, 7].Value2 + "",
                        SerialNo = range.Cells[rowIndex, 8].Value2 + "",
                        PO = range.Cells[rowIndex, 9].Value2 + "",
                        PurchasePrice = range.Cells[rowIndex, 10].Value2 + "",
                        TotalPrice = range.Cells[rowIndex, 11].Value2 + "",
                        Contact = range.Cells[rowIndex, 12].Value2 + "",
                        Vendor = range.Cells[rowIndex, 13].Value2 + "",
                        ModelBrandInfo = range.Cells[rowIndex, 14].Value2 + "",
                        TypeOfAnalysis = range.Cells[rowIndex, 15].Value2 + "",
                        PotentialUse = range.Cells[rowIndex, 16].Value2 + "",
                        IsInGoodUse = range.Cells[rowIndex, 17].Value2 + "",
                        CurrentUsersOfEquipment = range.Cells[rowIndex, 18].Value2 + "",
                        Revenue = range.Cells[rowIndex, 19].Value2 + "",
                        OtherUsefulInfo = range.Cells[rowIndex, 20].Value2 + "",
                    });
                }
            }
            finally
            {
                ReleaseObject(xlWorkSheet);
                ReleaseObject(xlWorkBook);
                ReleaseObject(xlApp);
            }


            return equipments;
        }

        private List<Equipment> ReadExcelData(HttpPostedFileBase equipmentExcel)
        {
            var equipments = new List<Equipment>();
            var excelFilePath = Server.MapPath("/TempExcel/") + Guid.NewGuid() + ".xlsx";
            equipmentExcel.SaveAs(excelFilePath);

            System.Data.DataTable dt = new System.Data.DataTable();

            //Open the Excel file in Read Mode using OpenXml.
            using (SpreadsheetDocument doc = SpreadsheetDocument.Open(excelFilePath, false))
            {
                //Read the first Sheets from Excel file.
                Sheet sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();

                //Get the Worksheet instance.
                DocumentFormat.OpenXml.Spreadsheet.Worksheet worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;

                //Fetch all the rows present in the Worksheet.
                IEnumerable<Row> rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();

                //Create a new DataTable.
                
                //Loop through the Worksheet rows.
                foreach (Row row in rows)
                {
                    //Use the first row to add columns to DataTable
                    if (row.RowIndex.Value == 1)
                    {
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            dt.Columns.Add(GetValue(doc, cell));
                        }
                    }
                    else
                    {
                        //Add rows to DataTable.
                        dt.Rows.Add();
                        int i = 0;
                        foreach (Cell cell in row.Descendants<Cell>())
                        {
                            dt.Rows[dt.Rows.Count - 1][i] = GetValue(doc, cell);
                            i++;
                        }
                    }
                }
               
            }

            foreach (DataRow row in dt.Rows)
            {
                equipments.Add(new Equipment()
                {
                    Equipment1 = row[0]+ "",
                    BarCode = row[1] + "",
                    NoOf = row[2] + "",
                    RoomNumber = row[3] + "",
                    Model = row[4] + "",
                    PurchaseDate = row[5] + "",
                    UNBCCode = row[6] + "",
                    SerialNo = row[7] + "",
                    PO = row[8] + "",
                    PurchasePrice = row[9] + "",
                    TotalPrice = row[10] + "",
                    Contact = row[11] + "",
                    Vendor = row[12] + "",
                    ModelBrandInfo = row[13] + "",
                    TypeOfAnalysis = row[14] + "",
                    PotentialUse = row[15] + "",
                    IsInGoodUse = row[16] + "",
                    CurrentUsersOfEquipment = row[17] + "",
                    Revenue = row[18] + "",
                    OtherUsefulInfo = row[19] + "",
                });
            }

            return equipments;
        }

        private string GetValue(SpreadsheetDocument doc, Cell cell)
        {
            if (cell.CellValue != null)
            {
                string value = cell.CellValue.InnerText;
                if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString)
                {
                    return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements.GetItem(int.Parse(value)).InnerText;
                }
                return value;
            }
            return "";
        }



        static private void ReleaseObject(object obj)
        {
            try
            {
                Marshal.ReleaseComObject(obj);
            }
            catch (Exception)
            {
                // ignored
            }
            finally
            {
                GC.Collect();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
