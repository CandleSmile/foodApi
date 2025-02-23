﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BusinessLayer.Dto.DBLoad
{
    public class IngredientDb
    {
        [JsonPropertyName("strIngredient")]
        [Required]
        public string? Name { get; set; }

        [JsonPropertyName("strDescription")]
        public string? Description { get; set; }
    }
}
