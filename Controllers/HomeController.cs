﻿using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using BooksApp.Models;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BooksApp.Controllers;

public class HomeController : Controller
{

    public HomeController()
    {

    }

    public IActionResult Index(string searchString, string category)
    {
        var products = Repository.Products;

        if (!string.IsNullOrEmpty(searchString))
        {
            ViewBag.SearchString = searchString;
            products = products.Where(p => p.Name!.ToLower().Contains(searchString)).ToList();
        }

        if (!string.IsNullOrEmpty(category) && category != "0")
        {
            products = products.Where(p => p.CategoryId == int.Parse(category)).ToList();
        }

        // ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId","Name",category);

        var model = new ProductViewModel
        {
            Products = products,
            Categories = Repository.Categories,
            SelectedCategory = category
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Create()
    {
        ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId", "Name");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Product model, IFormFile imageFile)
    {
        var allowenExtensions = new[] { ".jpg", ".png", ".jpeg" };
        const long maxFileSize = 2 * 1024 * 1024; // max 2 MB in bytes

        if (imageFile != null)
        {
            // Dosya boyutu hata mesajı
            if (imageFile.Length > maxFileSize)
            {
                ModelState.AddModelError("", "Dosya boyutu 2MB'ı geçmemelidir.");
            }
            else
            {
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowenExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Geçerli bir resim türü seçiniz.");
                }
                else
                {
                    var randomFileName = string.Format($"{Guid.NewGuid().ToString()}{extension}");
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", randomFileName);
                    try
                    {
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        model.Image = randomFileName;
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Dosya yüklenirken bir hata oluştu!");
                    }
                }
            }

        }
        else
        {
            ModelState.AddModelError("", "Bir resim seçiniz!");
        }
        if (ModelState.IsValid)
        {
            model.ProductId = Repository.Products.Count + 1;
            Repository.CreateProduct(model);
            return RedirectToAction("Index");
        }

        ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId", "Name");
        return View(model);
    }

    [HttpGet]
    public IActionResult Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var entity = Repository.Products.FirstOrDefault(p => p.ProductId == id);
        if (entity == null)
        {
            return NotFound();
        }
        ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId", "Name");
        return View(entity);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(int id, Product model, IFormFile? imageFile)
    {

        if (id != model.ProductId)
        {
            return NotFound();
        }

        var allowenExtensions = new[] { ".jpg", ".png", ".jpeg" };
        const long maxFileSize = 2 * 1024 * 1024; // max 2 MB in bytes

        if (imageFile != null)
        {
            // Fotoğraf boyutu hata mesajı
            if (imageFile.Length > maxFileSize)
            {
                ModelState.AddModelError("", "Dosya boyutu 2MB'ı geçmemelidir.");
            }
            else
            {
                var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();
                if (!allowenExtensions.Contains(extension))
                {
                    ModelState.AddModelError("", "Geçerli bir resim türü seçiniz.");
                }
                else
                {
                    var randomFileName = string.Format($"{Guid.NewGuid().ToString()}{extension}");
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/img", randomFileName);
                    try
                    {
                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            await imageFile.CopyToAsync(stream);
                        }
                        model.Image = randomFileName;
                    }
                    catch
                    {
                        ModelState.AddModelError("", "Dosya yüklenirken bir hata oluştu!");
                    }
                }
            }
        }
        else
        {
            ModelState.AddModelError("", "Bir resim seçiniz!");
        }
        if (ModelState.IsValid)
        {
            Repository.EditProduct(model);
            return RedirectToAction("index");
        }

        ViewBag.Categories = new SelectList(Repository.Categories, "CategoryId", "Name");
        return View(model);

    }

    public IActionResult Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }
        var entity = Repository.Products.FirstOrDefault(p => p.ProductId == id);
        if (entity == null)
        {
            return NotFound();
        }
        Repository.DeleteProduct(entity);
        return RedirectToAction("index");
    }

    public IActionResult ETicaret()
    {
        var activeProducts = Repository.Products.Where(p => p.IsActive).ToList();

        var model = new ProductViewModel { Products = activeProducts };
        return View(model);
    }
}
