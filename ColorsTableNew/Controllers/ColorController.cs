using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ColorsTableNew.Models;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace ColorsTableNew.Controllers
{
    public class ColorController : Controller
    {
        private readonly ColorDbContext _context;

        public ColorController(ColorDbContext context)
        {
            _context = context;
        }

        // GET: Color
        public async Task<IActionResult> Index()
        {
            return View(await _context.Colors.ToListAsync());
        }

        // For AJAX table refresh
        public async Task<IActionResult> GetColorTable()
        {
            var colors = await _context.Colors.ToListAsync();
            return PartialView("_ColorTable", colors);
        }

        // GET: Color/Create
        public IActionResult Create()
        {
            var model = new ColorModel(); // Initialize with default values
            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ColorID,ColorName,Price,DisplayOrder,IsInStock")] ColorModel colorModel)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    _context.Add(colorModel);
                    await _context.SaveChangesAsync();
                    var colors = await _context.Colors.ToListAsync();
                    return Json(new { isValid = true, html = await this.RenderViewAsync("_ColorTable", colors, true) });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while saving: " + ex.Message);
            }

            return Json(new { isValid = false, html = await this.RenderViewAsync("_CreateEdit", colorModel, true) });
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var colorModel = await _context.Colors.FindAsync(id);
            if (colorModel == null)
            {
                return NotFound();
            }
            return PartialView("_CreateEdit", colorModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ColorID,ColorName,Price,DisplayOrder,IsInStock")] ColorModel colorModel)
        {
            if (id != colorModel.ColorID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(colorModel);
                    await _context.SaveChangesAsync();
                    var colors = await _context.Colors.ToListAsync();
                    return Json(new { isValid = true, html = await this.RenderViewAsync("_ColorTable", colors, true) });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ColorModelExists(colorModel.ColorID))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return Json(new { isValid = false, html = await this.RenderViewAsync("_CreateEdit", colorModel, true) });
        }

        // GET: Color/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var colorModel = await _context.Colors.FirstOrDefaultAsync(m => m.ColorID == id);
            if (colorModel == null)
            {
                return NotFound();
            }

            return PartialView("_Delete", colorModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var colorModel = await _context.Colors.FindAsync(id);
            if (colorModel != null)
            {
                _context.Colors.Remove(colorModel);
                await _context.SaveChangesAsync();
            }
            var colors = await _context.Colors.ToListAsync();
            return Json(new { isValid = true, html = await this.RenderViewAsync("_ColorTable", colors, true) });
        }

        private bool ColorModelExists(int id)
        {
            return _context.Colors.Any(e => e.ColorID == id);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateOrder([FromBody] List<OrderUpdateModel> items)
        {
            try
            {
                foreach (var item in items)
                {
                    var color = await _context.Colors.FindAsync(item.Id);
                    if (color != null)
                    {
                        color.DisplayOrder = item.Order;
                    }
                }
                await _context.SaveChangesAsync();
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public class OrderUpdateModel
        {
            public int Id { get; set; }
            public int Order { get; set; }
        }
    }

    public static class ControllerExtensions
    {
        public static async Task<string> RenderViewAsync<TModel>(this Controller controller, string viewName, TModel model, bool partial = false)
        {
            ArgumentNullException.ThrowIfNull(controller);

            if (string.IsNullOrEmpty(viewName))
            {
                viewName = controller.ControllerContext.ActionDescriptor.ActionName;
            }

            controller.ViewData.Model = model;

            using var writer = new StringWriter();
            var viewEngine = controller.HttpContext.RequestServices.GetRequiredService<ICompositeViewEngine>();

            if (viewEngine == null)
            {
                throw new InvalidOperationException("ICompositeViewEngine service not found.");
            }

            var viewResult = viewEngine.FindView(controller.ControllerContext, viewName, !partial);

            if (!viewResult.Success)
            {
                throw new InvalidOperationException($"A view with the name {viewName} could not be found");
            }

            var viewContext = new ViewContext(
                controller.ControllerContext,
                viewResult.View,
                controller.ViewData,
                controller.TempData,
                writer,
                new HtmlHelperOptions()
            );

            await viewResult.View.RenderAsync(viewContext);
            return writer.ToString();
        }
    }

}