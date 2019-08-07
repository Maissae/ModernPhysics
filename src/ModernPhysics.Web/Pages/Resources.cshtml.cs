﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ModernPhysics.Models;
using ModernPhysics.Web.Data;

namespace ModernPhysics.Web.Pages
{
    public class ResourcesModel : PageModel
    {
        private WebAppDbContext _context;

        public ResourcesModel(WebAppDbContext context)
        {
            _context = context;
        }

        public IEnumerable<Post> Posts { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Category { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public void OnGet(string category)
        {
            if(string.IsNullOrEmpty(category))
            {
                Posts = _context.Posts.Include(p => p.Category).Where(p => p.IsPublished == true);
            }
            else
            {
                Posts = _context.Posts.Include(p => p.Category)
                    .Where(p => p.Category.FriendlyName.Equals(category) && p.IsPublished == true);
            }

            Categories = _context.Categories
                .Select(p => new SelectListItem
                {
                    Value = p.FriendlyName,
                    Text = p.Name
                }).ToList();

            Categories.Add(new SelectListItem
            {
                Value = null,
                Text = "Wszystkie"
            });

            //Categories.FirstOrDefault(p => string.IsNullOrEmpty(category)).Selected = true;

            //TODO: Change from == to .IsNullOrEmpty()
            Categories.FirstOrDefault(p => p.Value == category).Selected = true;
        }

        public IActionResult OnPost(string category, string search)
        {
            if(!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            if (!string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(search))
            {
                Posts = _context.Posts.Include(p => p.Category)
                .Where(p => p.Category.FriendlyName.Equals(category))
                .Where(p => p.IsPublished == true)
                .Where(p => p.Content.Contains(search));
            }

            else if(!string.IsNullOrEmpty(category) && string.IsNullOrEmpty(search))
            {
                Posts = _context.Posts.Include(p => p.Category)
                .Where(p => p.Category.FriendlyName.Equals(category))
                .Where(p => p.IsPublished == true);
            }

            else if(string.IsNullOrEmpty(category) && !string.IsNullOrEmpty(search))
            {
                Posts = _context.Posts.Include(p => p.Category)
                .Where(p => p.Content.Contains(search))
                .Where(p => p.IsPublished == true);
            }

            else
            {
                Posts = _context.Posts.Include(p => p.Category);
            }

            Categories = _context.Categories
                .Select(p => new SelectListItem
                {
                    Value = p.FriendlyName,
                    Text = p.Name
                }).ToList();

            Categories.Add(new SelectListItem
            {
                Value = null,
                Text = "Wszystkie"
            });

            //TODO: Change from == to .IsNullOrEmpty()
            Categories.FirstOrDefault(p => p.Value == category).Selected = true;

            Search = search;
            Category = category;

            return Page();
        }
    }
}