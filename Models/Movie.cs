﻿using System.ComponentModel.DataAnnotations;

namespace CRUD.Models
{
    public class Movie
    {
        public int MovieId { get; set; }
        [Required]
        [MaxLength(250)]
        public string Title { get; set; }
        public int Year { get;set; }
        public double Rate { get; set; }
        [MaxLength(2500)]
        public string StoryLine { get; set; }
        public byte[] Poster { get ; set; } 
        public byte GenreId { get; set; }   
        public Genre Genre { get; set; }
    }
}
