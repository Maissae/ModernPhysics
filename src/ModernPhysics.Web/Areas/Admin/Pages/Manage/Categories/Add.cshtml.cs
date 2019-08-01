﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ModernPhysics.Models;
using ModernPhysics.Web.Data;

namespace ModernPhysics.Web.Areas.Admin.Pages.Manage.Categories
{
    public class AddModel : PageModel
    {
        private WebAppDbContext _context;
        public AddModel(WebAppDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            public string Name { get; set; }
            public string FriendlyName { get; set; }
            public string Icon { get; set; }
            public bool UseCustomFriendlyName { get; set; }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return new BadRequestResult();
            }

            string friendlyName;
            if(Input.UseCustomFriendlyName == true)
            {
                friendlyName = Input.FriendlyName;
            }
            else
            {
                friendlyName = Input.FriendlyName.Trim().Replace(' ', '-').ToLower();
            }
            
            var category = new Category {
                Name = Input.Name,
                FriendlyName = friendlyName,
                Icon = Input.Icon
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return new RedirectToPageResult("/Manage/Categories", new { area = "Admin" });
        }
    }
}