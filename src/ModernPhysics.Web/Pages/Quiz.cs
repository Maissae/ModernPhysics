﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using ModernPhysics.Models;
using ModernPhysics.Web.Data;
using Westwind.AspNetCore.Markdown;

namespace ModernPhysics.Web.Pages
{
    public class QuizModel : PageModel
    {
        private WebAppDbContext _context;

        public QuizModel(WebAppDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public string Category { get; set; }
        [BindProperty(SupportsGet = true)]
        public string PostUrl { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }
        [BindProperty]
        public InputQuizModel Input { get; set; }
        public List<Category> Categories { get; set; }
        public bool? Result { get; set; }

        public IActionResult OnGet(string quizurl)
        {
            Result = null;
            var quiz = _context.Quizzes
                .Include(p => p.Questions)
                .ThenInclude(p => p.Answers)
                .FirstOrDefault(p => p.FriendlyUrl.Equals(quizurl));

            if(quiz == null || quiz.IsDeleted == true)
            {
                return RedirectToPage("/NotFound");
            }

            if(quiz.IsPublished == false && User.Identity.IsAuthenticated == false)
            {
                return RedirectToPage("/NotFound");
            }
            
            Categories = _context.Categories.Include(p => p.Posts).ToList();
            foreach(var _category in Categories)
            {
                _category.Posts = _category.Posts.Where(p => p.IsPublished == true).ToList();
            }

            Input = new InputQuizModel {
                Id = quiz.Id,
                Title = quiz.Title,
                FriendlyUrl = quiz.FriendlyUrl,
                Questions = new List<Question>()
            };
            foreach(var question in quiz.Questions)
            {
                var _question = new Question {
                    Id = question.Id,
                    Text = question.Text,
                    Answers = new List<Answer>()
                };

                foreach(var answer in question.Answers)
                {
                    var _answer = new Answer {
                        Id = answer.Id,
                        Text = answer.Text
                    };

                    _question.Answers.Add(_answer);
                }
                Input.Questions.Add(_question);
            }
            
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string quizurl)
        {
            Categories = _context.Categories.Include(p => p.Posts).ToList();
            foreach(var _category in Categories)
            {
                _category.Posts = _category.Posts.Where(p => p.IsPublished == true).ToList();
            }

            var quiz = _context.Quizzes
                .Include(p => p.Questions)
                .ThenInclude(p => p.Answers)
                .FirstOrDefault(p => p.FriendlyUrl.Equals(quizurl));

            if(quiz == null || quiz.IsDeleted == true)
            {
                return RedirectToPage("/NotFound");
            }

            if(quiz.IsPublished == false && User.Identity.IsAuthenticated == false)
            {
                return RedirectToPage("/NotFound");
            }

            if (!ModelState.IsValid)
            {               
                return Page();
            }

            Input.Title = quiz.Title;
            Input.FriendlyUrl = quiz.FriendlyUrl;

            bool solvedCorrectly = true;
            foreach(var question in Input.Questions)
            {
                var _question = quiz.Questions.FirstOrDefault(q => q.Id == question.Id);
                if(_question != null)
                {
                    foreach(var answer in question.Answers)
                    {
                        var _answer = _question.Answers.FirstOrDefault(a => a.Id == answer.Id);
                        if(_answer != null)
                        {
                            if(answer.IsCorrect == true && _answer.IsCorrect == true) 
                            {
                                answer.Result = 1;
                            }
                            else if(answer.IsCorrect != _answer.IsCorrect) 
                            {
                                answer.Result = -1;
                                solvedCorrectly = false;
                                Result = false;
                            }
                        }
                    }
                }
            }

            quiz.TimesSolved++;
            if(solvedCorrectly) {
                quiz.TimesSolvedCorrectly++;
                Result = true;
            }

            _context.Quizzes.Update(quiz);
            await _context.SaveChangesAsync();

            return Page();
        }
    }

    public class InputQuizModel
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string FriendlyUrl { get; set; }
        public List<Question> Questions { get; set; }
    }

    public class Question
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public List<Answer> Answers { get; set; }
    }

    public class Answer
    {
        public Guid Id { get; set; }
        public string Text { get; set; }
        public bool IsCorrect { get; set; }
        public int Result { get; set; }
    }
}