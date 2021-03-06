﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ModernPhysics.Web.Data;
using Microsoft.EntityFrameworkCore;
using ModernPhysics.Models;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using ModernPhysics.Web.Utils;

namespace ModernPhysics.Web.Areas.Admin.Pages.Manage.Posts
{
    public class EditModel : PageModel
    {

        private WebAppDbContext _context;
        private ICharacterParser _parser;
        public EditModel(WebAppDbContext context, ICharacterParser parser)
        {
            _context = context;
            _parser = parser;
        }

        [BindProperty(SupportsGet = true)]
        public InputPostModel Input { get; set; }
        public List<SelectListItem> Categories { get; set; }
        public List<SelectListItem> ContentTypes { get; set; }
        public string BaseUrl { get; set; }

        [TempData]
        public string Result { get; set; }

        public async Task<IActionResult> OnGet(Guid? id)
        {
            //TODO: Change into ErrorMessage and return to list
            if (id == null)
            {
                return RedirectToPage("/NotFound");
            }

            var post = await _context.Posts
                .Include(p => p.Category)
                .Where(p => p.IsDeleted == false)
                .FirstOrDefaultAsync(p => p.Id.Equals(id));

            if (post == null)
            {
                return RedirectToPage("/NotFound");
            }  

            Categories = GetCategories();
            BaseUrl = GetBaseUrl();
            ContentTypes = GetContentTypes();

            Input = new InputPostModel
            {
                Title = post.Title,
                FriendlyUrl = post.FriendlyUrl,
                Shortcut = post.Shortcut,
                Content = post.Content,
                IsPublished = post.IsPublished,
                Category = post.Category.FriendlyName,
                ContentType = post.ContentType
            };

            var category = Categories.FirstOrDefault(p => 
                p.Value.Equals(post.Category.FriendlyName));
            
            if(category != null)
            {
                category.Selected = true;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(Guid id)
        {
            Categories = GetCategories();
            BaseUrl = GetBaseUrl();
            ContentTypes = GetContentTypes();
            
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if(string.IsNullOrEmpty(Input.FriendlyUrl))
            {
                Input.FriendlyUrl = Regex.Replace(Input.Title, "[ !?\"#$%&'()*+,./:;<=>@[\\]^`{|}~]", "-");
                Input.FriendlyUrl = _parser.ParsePolishChars(Input.FriendlyUrl);
            }

            if(await _context.Posts.AnyAsync(p =>
                p.FriendlyUrl.Equals(Input.FriendlyUrl) &&
                p.Category.Name.Equals(Input.Category) &&
                p.Id.Equals(id) == false))
                {
                    Result = "Ten url jest już zajęty!";
                    return Page();
                }

            var post = await _context.Posts.Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id.Equals(id));

            var category = await _context.Categories
                .FirstOrDefaultAsync(p => p.FriendlyName.Equals(Input.Category));

            post.Title = Input.Title;
            post.FriendlyUrl = Input.FriendlyUrl;
            post.Shortcut = Input.Shortcut;
            post.Content = SanitizeHtml(Input.Content);
            post.IsPublished = Input.IsPublished;
            post.Category = category;
            post.ModifiedBy = User.Identity.Name;
            post.ContentType = Input.ContentType;
            post.IsDeleted = false;

            _context.Categories.Update(category);
            _context.Posts.Update(post);

            await _context.SaveChangesAsync();

            return new RedirectToPageResult("/Manage/Posts", new { area = "Admin" });
        }

        private string GetBaseUrl() => $"{Request.Scheme}://{Request.Host}";

        private List<SelectListItem> GetCategories()
        {
            var list = new List<SelectListItem>();

            list = _context.Categories
                .Select(p => new SelectListItem
                {
                    Value = p.FriendlyName,
                    Text = p.Name
                }).ToList();

            return list;
        }

        private List<SelectListItem> GetContentTypes()
        {
            var list = Enum.GetValues(typeof(ContentType))
                .Cast<ContentType>()
                .Select(t => new SelectListItem {
                    Text = t.ToString(),
                    Value = ((int)t).ToString()
                    }).ToList();

            return list;
        }

        private string ReplaceRegex(string source, string regex, string replaceWith)
        {
            var matches = Regex.Matches(source, regex);
            for (int i = 0; i < matches.Count; i++)
            {
                source = source.Replace(matches[i].Value, replaceWith);
            }
            return source;
        }

        private string SanitizeHtml(string html)
        {
            if(string.IsNullOrEmpty(html) == false)
            {
                html = ReplaceRegex(html, @"<script(.*?)>(.*?)</script>", "");
                html = ReplaceRegex(html, @"<object(.*?)>(.*?)</object>", "");
                html = ReplaceRegex(html, @"<link(.*?)>(.*?)</link>", "");
                html = ReplaceRegex(html, @"<embed(.*?)>(.*?)</embed>", "");
            }
            return html;
        }
    }
}